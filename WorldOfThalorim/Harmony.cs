using HarmonyLib;
using System;
using System.Diagnostics;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;



namespace WorldOfThalorim
{
    [HarmonyPatch(typeof(EntityBehaviorHealth), "ApplyHealing",
     new Type[] { typeof(DamageSource), typeof(float) })]
    public class HarmonySkeletonHealing
    {
        [HarmonyPatch(typeof(EntityBehaviorHealth), "ApplyHealing")]
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
}

