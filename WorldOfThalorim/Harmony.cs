using HarmonyLib;
using System;
using System.Diagnostics;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;



namespace WorldOfThalorim
{
    [HarmonyPatch(typeof(EntityBehaviorHealth))]
    [HarmonyPatch("ApplyHealing")]
    public class HarmonySkeletonHealing
    {
        [HarmonyPrefix]
        public static bool Prefix_ApplyHealing(EntityBehaviorHealth __instance, DamageSource damageSource, ref float damage)
        {
            Entity entity = __instance.entity;
            if (entity is EntityPlayer player)
            {
                EntityTagArray tagArray = player.Tags;
                ICoreAPI api = player.Api;
                ushort skeletonTagId = api.TagRegistry.EntityTagToTagId("skeleton");

                if (skeletonTagId == 0)
                {
                    Debug.WriteLine("[WorldOfThalorim] Null PlayerTag");
                }

                EntityTagArray SkeletonArray = new EntityTagArray(skeletonTagId);//Тупая штука, отсутствие просто проверки тега а не ебаной сверки

                if (tagArray.ContainsAll(SkeletonArray))
                {
                    ItemStack lastitemstack = null;

                    IPlayer iplay = player.Player;
                    ItemSlot activeSlot = iplay.InventoryManager.ActiveHotbarSlot;
                    ItemStack itemstack = activeSlot.Itemstack;

                    if (itemstack == null && lastitemstack != null)
                    {
                        itemstack = lastitemstack;
                    }
                    if (itemstack == null) { return false; }
                    lastitemstack = itemstack;

                    CollectibleObject collectible = itemstack.Collectible;

                    string itemName = collectible.GetHeldItemName(activeSlot.Itemstack);
                    if (itemName.ToLower().Contains("potion") || itemName.ToLower().Contains("potionflask"))
                    {
                        return true;
                    }

                    damage = 0f;

                    return true;
                }

            }
            return false;
        }
    }

    [HarmonyPatch("HydrateOrDiedrate.Hot_Weather.EntityBehaviorBodyTemperatureHot", "UpdateCoolingFactor")] //Я убил на этот код столько времени что ну... я хз, при этом он не то чтоб хорош, или сложный
    public static class HarmonyCoolingAttribute
    {
        [HarmonyPostfix]
        public static void Postfix_UpdateCoolingFactor(object __instance)
        {
            Type type = __instance.GetType();
            FieldInfo entityProp = typeof(EntityBehavior)
                .GetField("entity", BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo coolingProp = type.GetProperty("Cooling");

            if (coolingProp != null && entityProp != null)
            {
                Entity entity = entityProp.GetValue(__instance) as Entity;
                EntityAgent entityAgent = entity as EntityAgent;
                float currentValue = (float)coolingProp.GetValue(__instance);
                float bonus = entityAgent.Stats.GetBlended("coolingBonus");
                float newValue = currentValue + bonus;
                coolingProp.SetValue(__instance, newValue);
            }
        }
    }
}

