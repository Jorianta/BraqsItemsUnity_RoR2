using RoR2;
using R2API;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using static BraqsItems.Util.Helpers;
using BraqsItems.Misc;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace BraqsItems
{   
    public static class TimedCooldownReduction
    {
        public static ItemDef itemDef;

        public static BuffDef buffDef;

        public static float baseTimer = 10f;
        public static float timerReduction = .15f;

        internal static void Init()
        {
            Log.Info("Initializing Simon Says");

            if (!ConfigManager.BiggerExplosions_isEnabled.Value) return;

            //ITEM//
            itemDef = GetItemDef("TimedCooldownReduction");

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));


            Hooks();

            Log.Info("Simon Says Initialized");
        }

        private static void Hooks()
        {
            // On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
        }

        // public class BraqsItems_CooldownReductionTimer : CharacterBody.ItemBehavior
        // {

        //     public float activationTimer = baseTimer;
            
        //     public float reductionInterval = 1f;
        //     public float reductionTimer = reductionInterval;

        //     private void Start()
        //     {
        //         Log.Debug("CooldownReductionTimer:Start()");

        //         if (body) body.onInventoryChanged += Body_onInventoryChanged;
        //     }

        //     private void Update()
        //     {
        //         activationTimer -= Time.deltaTime;

        //         if(activationTimer > 0) return;
                

        //     }

        //     private void ReduceCooldown()
        //     {
                
        //     }

        //     private void Body_onInventoryChanged()
        //     {
        //         if (!body) return;
        //         if (stack < 1) 
        //         {
        //             countdownTimer = baseTimer;
        //             return;
        //         }

        //         countdownTimer = Mathf.Min(baseTimer * Mathf.Pow( 1 - timerReduction, stack-1), countdownTimer);
        //         trigger
        //     }

        //     private void OnDestroy()
        //     {
        //         Log.Debug("CooldownReductionTimer:OnDestroy()");
        //         if (body) body.onInventoryChanged -= Body_onInventoryChanged;
               
        //     }
        // }
    }
}
