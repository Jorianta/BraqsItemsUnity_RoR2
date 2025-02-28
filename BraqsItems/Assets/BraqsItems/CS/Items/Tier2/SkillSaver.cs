using RoR2;
using R2API;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using RoR2.Skills;
using System;
using static BraqsItems.Util.Helpers;
using RiskOfOptions.Resources;

namespace BraqsItems
{
    public static class SkillSaver
    {
        public static ItemDef itemDef;
        public static GameObject pillEffect;
        public static GameObject restockEffect;

        internal static void Init()
        {
            if (!ConfigManager.SkillSaver_isEnabled.Value) return;

            Log.Info("Initializing Rhino Pill Item");
            //ITEM//
            itemDef = GetItemDef("SkillSaver");

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            //EFFECT// 
            GenerateEffect();

            Hooks();

            Log.Info("Rhino Pill Initialized");
        }

        private static void GenerateEffect()
        {

            try
            {
                pillEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/CoinImpact.prefab").WaitForCompletion().InstantiateClone("BluePills");

                var coin = pillEffect.transform.Find("CoinParticle").gameObject.GetComponent<ParticleSystem>();
                var coinMain = coin.main;
                coinMain.startSize = 0.30f;

                var coinRender = pillEffect.transform.Find("CoinParticle").gameObject.GetComponent<ParticleSystemRenderer>();
                var coinMaterial = coinRender.material;
                coinMaterial.SetColor("_Color", new Color(0f, 0.50f, 1f));

                ContentAddition.AddEffect(pillEffect);
            }
            catch (Exception e)
            {
                Log.Warning("SkillSaver:GenerateEffect() ; Could not edit coin impact." +
                    "\n" + e);
            }
            try
            {
                restockEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/EquipmentRestockEffect.prefab").WaitForCompletion().InstantiateClone("SkillStock");

                var light = restockEffect.transform.Find("Light").gameObject.GetComponent<Light>();
                light.color = new Color(0.1f, 0.60f, 1f);

                var particles = restockEffect.transform.Find("MotionEmitter").gameObject;

                var squareRender = particles.transform.Find("SquaresEmitter").gameObject.GetComponent<ParticleSystemRenderer>();
                var squareMaterial = squareRender.material;
                squareMaterial.SetColor("_TintColor", new Color(0f, 0.50f, 1f));

                var flashRender = particles.transform.Find("FlashEmitter").gameObject.GetComponent<ParticleSystemRenderer>();
                var flashMaterial = flashRender.material;
                flashMaterial.SetColor("_TintColor", new Color(0.1f, 0.60f, 1f));

                ContentAddition.AddEffect(restockEffect);
            }
            catch (Exception e)
            {
                Log.Warning("SkillSaver:GenerateEffect() ; Could not edit restock effect." +
                    "\n" + e);
            }
        }

        private static void Hooks()
        {
            IL.RoR2.EquipmentSlot.OnEquipmentExecuted += EquipmentSlot_OnEquipmentExecuted;
            IL.RoR2.Skills.SkillDef.OnExecute += SkillDef_OnExecute;
            On.RoR2.Skills.MercDashSkillDef.OnExecute += MercDashSkillDef_OnExecute;
            //On.RoR2.GenericSkill.DeductStock += GenericSkill_DeductStock;
        }

        private static void GenericSkill_DeductStock(On.RoR2.GenericSkill.orig_DeductStock orig, GenericSkill self, int count)
        {
            if(self.characterBody && self.characterBody.inventory && self.characterBody.master)
            {
                int stack = self.characterBody.inventory.GetItemCount(itemDef);

                if (stack > 0)
                {
                    int ogCount = count;
                    float chance = ((stack) * ConfigManager.SkillSaver_chance.Value / ((stack) * ConfigManager.SkillSaver_chance.Value + 100)) * 100;

                    for (int i = 0; i < count; i++) count -= RoR2.Util.CheckRoll(chance, self.characterBody.master)? 1 : 0;

                    if(ogCount != count)
                    {
                        RoR2.Util.PlaySound("Play_UI_equipment_activate", self.characterBody.gameObject);

                        EffectData effectData = new EffectData
                        {
                            scale = 1,
                            origin = self.characterBody.corePosition,
                        };
                        EffectManager.SpawnEffect(pillEffect, effectData, true);

                        effectData.SetNetworkedObjectReference(self.characterBody.gameObject);
                        EffectManager.SpawnEffect(restockEffect, effectData, true);
                    }
                }
            }

            orig(self, count);
        }

        //mercdash is a special case, and needs two extra lines of code. because mercdashskilldef calls base.execute() so early, I can't just handle it in the other hook. Isn't programming fun?
        private static void MercDashSkillDef_OnExecute(On.RoR2.Skills.MercDashSkillDef.orig_OnExecute orig, MercDashSkillDef self, GenericSkill skillSlot)
        {
            orig(self, skillSlot);

            CharacterBody characterBody = skillSlot.characterBody;

            if (!characterBody) return;

            int stack = characterBody.inventory.GetItemCount(itemDef);

            if (stack > 0)
            {
                float chance = ((stack) * ConfigManager.SkillSaver_chance.Value / ((stack) * ConfigManager.SkillSaver_chance.Value + 100)) * 100;
                Log.Debug("Chance: " + chance);

                if (RoR2.Util.CheckRoll(chance, characterBody.master))
                {
                    RoR2.Util.PlaySound("Play_UI_equipment_activate", characterBody.gameObject);

                    EffectData effectData = new EffectData
                    {
                        scale = 1,
                        origin = characterBody.corePosition,
                    };
                    EffectManager.SpawnEffect(pillEffect, effectData, true);

                    effectData.SetNetworkedObjectReference(characterBody.gameObject);
                    EffectManager.SpawnEffect(restockEffect, effectData, true);

                    MercDashSkillDef.InstanceData instanceData = (MercDashSkillDef.InstanceData)skillSlot.skillInstanceData;

                    //Proccing on dash lets you do that same level of dash again.
                    skillSlot.stock += 1;
                    Log.Debug("Saving merc dash");
                    instanceData.waitingForHit = false;
                    instanceData.hasExtraStock = true;
                    
                }
            }
        }

        private static void EquipmentSlot_OnEquipmentExecuted(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            Log.Debug("SkillSaver: adding equip save chance");
            try
            {
                c.GotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<EquipmentSlot>("inventory"),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<EquipmentSlot>("get_activeEquipmentSlot"),
                x => x.MatchLdcI4(1)
                );
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<int, EquipmentSlot, int>>((i, c) => TryPreventStockConsumption(i, c.characterBody));
            }
            catch (Exception e) { ErrorHookFailed("Add equip save chance", e); }
        }

        private static void SkillDef_OnExecute(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            Log.Debug("SkillSaver: adding skill save chance");
            try
            {
                c.GotoNext(
                MoveType.After,
                x => x.MatchLdarg(1),
                x => x.MatchDup(),
                x => x.MatchCallOrCallvirt<GenericSkill>("get_stock"),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<SkillDef>("stockToConsume")
                );
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Func<int, GenericSkill, int>>((i, c) => 
                { 
                    if (c.skillDef is MercDashSkillDef || !(c.baseRechargeInterval > 0 || c.rechargeStock == 0)) return i;
                    return TryPreventStockConsumption(i, c.characterBody); 
                });
            }
            catch (Exception e) { ErrorHookFailed("Add skill save chance", e); }
        }

        private static int TryPreventStockConsumption(int stocksToConsume, CharacterBody characterBody)
        {
            Log.Debug("SkillSaver:TryPreventStockConsumption");
            if (stocksToConsume > 0 && characterBody.master && characterBody.inventory)
            {
                int stack = characterBody.inventory.GetItemCount(itemDef);

                if (stack > 0)
                {
                    float chance = ((stack) * ConfigManager.SkillSaver_chance.Value / ((stack) * ConfigManager.SkillSaver_chance.Value + 100)) * 100;
                    Log.Debug("Chance: " + chance);

                    if (RoR2.Util.CheckRoll(chance, characterBody.master))
                    {
                        RoR2.Util.PlaySound("Play_UI_equipment_activate", characterBody.gameObject);

                        EffectData effectData = new EffectData
                        {
                            scale = 1,
                            origin = characterBody.corePosition,
                        };
                        EffectManager.SpawnEffect(pillEffect, effectData, true);

                        effectData.SetNetworkedObjectReference(characterBody.gameObject);
                        EffectManager.SpawnEffect(restockEffect, effectData, true);

                        return 0;
                    }
                }
            }

            return stocksToConsume;
        }
    }
}