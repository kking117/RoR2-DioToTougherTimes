﻿using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using R2API;
using R2API.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace ConsumedBuff.ItemChanges
{
    public class PowerElixir
    {
        public static void Enable()
        {
            UpdateText();
            if (MainPlugin.Elixir_Regen.Value != 0f)
            {
                RecalculateStatsAPI.GetStatCoefficients += CalculateStatsHook;
            }
            if (MainPlugin.Elixir_Buff.Value > 0f)
            {
                On.RoR2.CharacterMasterNotificationQueue.PushItemTransformNotification += OnItemAdded;
            }
        }
        public static void UpdateText()
        {
            string pickup = string.Format("");
            string desc = string.Format("");
            if(MainPlugin.Elixir_Regen.Value != 0f)
            {
                pickup = string.Format("An empty container from an Elixir you consumed. Boosts health regen.");
                desc = string.Format("Increases <style=cIsHealing>base health regeneration</style> by <style=cIsHealing>+{0} hp/s</style> <style=cStack>(+{0} per stack)</style>.", MainPlugin.Elixir_Regen.Value);
            }
            else
            {
                pickup = string.Format("An empty container from an Elixir you consumed. Does nothing.");
                desc = string.Format("A spent item with no remaining power.");
            }
            LanguageAPI.Add("ITEM_HEALINGPOTIONCONSUMED_PICKUP", pickup);
            LanguageAPI.Add("ITEM_HEALINGPOTIONCONSUMED_DESC", desc);
        }
        private static void OnItemAdded(On.RoR2.CharacterMasterNotificationQueue.orig_PushItemTransformNotification orig, CharacterMaster self, ItemIndex oldItem, ItemIndex newItem, CharacterMasterNotificationQueue.TransformationType transformationType)
        {
            orig(self, oldItem, newItem, transformationType);
            if (NetworkServer.active)
            {
                if (transformationType == CharacterMasterNotificationQueue.TransformationType.Default)
                {
                    if (self.GetBody())
                    {
                        if (oldItem == DLC1Content.Items.HealingPotion.itemIndex)
                        {
                            if (newItem == DLC1Content.Items.HealingPotionConsumed.itemIndex)
                            {
                                MainPlugin.AddTimeToBuff(self.GetBody(), RoR2Content.Buffs.CrocoRegen, 2.5f, true);
                            }
                        }
                    }
                }
            }
        }
        private static void CalculateStatsHook(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.inventory)
            {
                float levelBonus = sender.level - 1f;
                int itemCount = sender.inventory.GetItemCount(DLC1Content.Items.HealingPotionConsumed);
                if(itemCount > 0)
                {
                    levelBonus = MainPlugin.Elixir_Regen.Value * 0.2f * levelBonus;
                    args.baseRegenAdd += itemCount * (MainPlugin.Elixir_Regen.Value + levelBonus);
                }
            }
        }
    }
}