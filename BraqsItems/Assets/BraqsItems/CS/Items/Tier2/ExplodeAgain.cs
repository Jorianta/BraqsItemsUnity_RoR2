using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using RoR2.Projectile;
using System;
using static BraqsItems.Util.Helpers;

namespace BraqsItems
{
    internal class ExplodeAgain
    {
        public static ItemDef itemDef;

        public static GameObject bomblettePrefab;

        internal static void Init()
        {
            if (!ConfigManager.ExplodeAgain_isEnabled.Value) return;

            Log.Info("Initializing Bomblette Item");
            //ITEM//
            itemDef = GetItemDef("ExplodeAgain");

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            //EFFECTS//
            bomblettePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/CryoCanisterBombletsProjectile.prefab").WaitForCompletion();
            if(!bomblettePrefab.TryGetComponent(out ProjectileExplosion explosion)) Log.Debug("couldn't load bomblette explosion!!!!");
            explosion.blastProcCoefficient = 0;

            Hooks();

            Log.Info("Bomblette Initialized");
        }

        public static void Hooks()
        {
            On.RoR2.BlastAttack.Fire += BlastAttack_Fire;
        }

        public static BlastAttack.Result BlastAttack_Fire(On.RoR2.BlastAttack.orig_Fire orig, BlastAttack self)
        {

            if (NetworkServer.active && self.attacker && self.attacker.TryGetComponent(out CharacterBody body) && body.inventory)
            {
                var items = body.inventory.GetItemCount(itemDef);

                if(items > 0 && self.procCoefficient > 0) FireChildExplosions(self, body, items);
            }

            return orig(self);
        }

        //Much of this code was taken from the molten perf, thanks hopoo
        protected static void FireChildExplosions(BlastAttack self, CharacterBody body, int items)
        {
            Log.Debug("ExplodeAgain:FireChildExplosions");
            Vector3 vector2 = self.position;
            Vector3 vector3 = Vector3.up;

            int maxbombs = (items - 1) * ConfigManager.ExplodeAgain_maxBombsPerStack.Value + ConfigManager.ExplodeAgain_maxBombsBase.Value;

            EffectData effectData = new EffectData
            {
                scale = 1f,
                origin = vector2
            };

            GameObject bomblette = bomblettePrefab;
            ProjectileExplosion explosion = bomblette.GetComponent< ProjectileExplosion>();
            explosion.blastRadius = self.radius * ConfigManager.ExplodeAgain_radiusCoefficient.Value;

            float damage = RoR2.Util.OnHitProcDamage(self.baseDamage, body.damage, ConfigManager.ExplodeAgain_damageCoefficient.Value);

            for (int n = 0; n < maxbombs; n++)
            {
                if (!RoR2.Util.CheckRoll(ConfigManager.ExplodeAgain_chance.Value * self.procCoefficient, body.master)) continue;

                float speedOverride = UnityEngine.Random.Range(0.5f, 1f) * self.radius * 3;

                float angle = (float)n * MathF.PI * 2f / (float)maxbombs;
                vector3.x += Mathf.Sin(angle);
                vector3.z += Mathf.Cos(angle);

                FireProjectileInfo fireProjectileInfo = default;
                fireProjectileInfo.projectilePrefab = bomblette;
                fireProjectileInfo.position = vector2 + new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                fireProjectileInfo.rotation = RoR2.Util.QuaternionSafeLookRotation(vector3);
                fireProjectileInfo.procChainMask = self.procChainMask;
                fireProjectileInfo.owner = self.attacker;
                fireProjectileInfo.damage = damage;
                fireProjectileInfo.crit = self.crit;
                fireProjectileInfo.force = self.baseForce;
                fireProjectileInfo.damageColorIndex = DamageColorIndex.Item;
                fireProjectileInfo.speedOverride = speedOverride;
                fireProjectileInfo.useSpeedOverride = true;
                FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;


                ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
            }
        }
    }
}
