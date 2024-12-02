using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using static BraqsItems.Misc.Stats;

namespace BraqsItems.Misc
{
    internal class CharacterEvents
    {
        public static void Init()
        {
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;

            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
        }

        private static void CharacterBody_onBodyStartGlobal(CharacterBody body)
        {
            if (body.masterObject && !body.masterObject.GetComponent<BraqsItems_CharacterEventComponent>())
            {
                body.masterObject.AddComponent<BraqsItems_CharacterEventComponent>();
            }
        }

        private static void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig(self, damageReport);

            if (damageReport.victimMaster && damageReport.victimMaster.TryGetComponent(out BraqsItems_CharacterEventComponent eventComponent))
            {
                eventComponent.OnDeath();
            }
            else
            {
                Log.Debug("BraqsItems: a character had no event component attached.");
            }
        }


        public class BraqsItems_CharacterEventComponent : MonoBehaviour
        {
            public CharacterMaster master;
            public HealthComponent healthComponent;
            public CharacterBody body;

            public delegate void CharacterDeathHandler(CharacterBody body);

            public event CharacterDeathHandler OnCharacterDeath;

            public void Start()
            {

                if (gameObject.TryGetComponent(out CharacterMaster component))
                {
                    master = component;
                    body = master.GetBody();
                    healthComponent = master.GetComponent<HealthComponent>();
                }
 
            }

            public void OnDeath()
            {
                OnCharacterDeath?.Invoke(body);
                Log.Debug("OnCharacterDeath");
            }
        }
    }
}
