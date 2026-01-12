using HarmonyLib;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
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
                    if (damageSource.DamageOverTimeType == 9999)
                    {
                        return true;
                    }

                    damage = 0f;

                    return true;
                }

            }
            return true;
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

    [HarmonyPatch("Alchemy.TempEffect", "ApplyHealth")]
    public static class HarmonyTempEffect
    {
        [HarmonyPrefix]
        public static bool Prefix_ApplyHealth(object __instance, EntityPlayer entity) //Этот код является дуркой, но подругому я не умею... выглядит ужастно, он заменяет уже сущестующих код но в 3 раза больше и всё это для того чтоб добавить одну строчку кода
        {
            Type type = __instance.GetType();

            FieldInfo contextField = type.GetField("Context",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            object contextObj = contextField.GetValue(__instance);
            Type contextType = contextObj.GetType();
            PropertyInfo healthProp = contextType.GetProperty("Health",
                BindingFlags.Public | BindingFlags.Instance);

            float health = (float)healthProp.GetValue(contextObj);

            if (Math.Abs(health) <= float.Epsilon)
            {
                //return true; мб я всё сломал...
            }
            float wearableHealEffect = 0f; //Это тут было до меня

            PropertyInfo ignoreArmourProp = contextType.GetProperty("IgnoreArmour",
                BindingFlags.Public | BindingFlags.Instance);

            bool ignoreArmour = (bool)ignoreArmourProp.GetValue(contextObj);

            if (ignoreArmour)
            {
                ITreeAttribute statsTree = entity.WatchedAttributes
                   .GetTreeAttribute("stats")
                    ?.GetTreeAttribute("healingeffectivness");

                if (statsTree != null)
                    wearableHealEffect = statsTree.GetFloat("wearablemod");

                if (Math.Abs(wearableHealEffect) > float.Epsilon)
                    entity.Stats.Set("healingeffectivness", "wearablemod", 0f, false);
            }

            var damageSource = new DamageSource
            {
                Source = EnumDamageSource.Internal,
                Type = health > 0 ? EnumDamageType.Heal : EnumDamageType.Poison,

                DamageOverTimeType = 9999 //попытка
            };

            entity.ReceiveDamage(damageSource, Math.Abs(health));

            if (Math.Abs(wearableHealEffect) > float.Epsilon)
                entity.Stats.Set("healingeffectivness", "wearablemod", wearableHealEffect, false);

            return false;
        }
    }

    [HarmonyPatch("rustboundmagic.src.common.item.resource.ItemRustyDustRM", "OnHeldInteractStop")]
    public static class HarmonyItemRustyDustRM
    {
        [HarmonyPrefix]
        public static bool Prefix_OnHeldInteractStop(object __instance, float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (byEntity == null)
                return false;

            if (!(byEntity is EntityPlayer entityPlayer))
                return false;

                int giftedMagic = entityPlayer.WatchedAttributes.GetInt("giftedMagic", 0);
            if (giftedMagic == 1)
            {
                if (entityPlayer.World.Side == EnumAppSide.Server)
                {
                    IServerPlayer serverPlayer = entityPlayer.World.PlayerByUid(entityPlayer.PlayerUID) as IServerPlayer;
                    serverPlayer.SendMessage(GlobalConstants.GeneralChatGroup, Lang.Get("worldofthalorim:RustyDustTrue", Array.Empty<object>()), EnumChatType.Notification, null);
                }

                return true;
            }

            if (entityPlayer.World.Side == EnumAppSide.Server)
            {
                IServerPlayer serverPlayer = entityPlayer.World.PlayerByUid(entityPlayer.PlayerUID) as IServerPlayer;
                serverPlayer.SendMessage(GlobalConstants.GeneralChatGroup, Lang.Get("worldofthalorim:RustyDustFalse", Array.Empty<object>()), EnumChatType.Notification, null);
            }

            return false;
        }
    }
}

