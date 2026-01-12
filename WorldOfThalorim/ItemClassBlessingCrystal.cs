using HarmonyLib;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace WorldOfThalorim
{
    public class ItemClassBlessingCrystal : Item
    {
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (byEntity == null || slot.Empty) return;

            handling = EnumHandHandling.PreventDefault;
            byEntity.AnimManager.StartAnimation("interactfirmgrip");
        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (byEntity == null || slot.Empty) return false; 

            if (secondsUsed < 1.5f) //досрочное завершение
            {
                return true;
            }

            return false;
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            byEntity.AnimManager.StopAnimation("interactfirmgrip");

            if (byEntity == null || slot.Empty || secondsUsed < 1.5f) return;

            int giftedMagic = byEntity.WatchedAttributes.GetInt("giftedMagic", 0);
            if (giftedMagic == 0)
            {
                byEntity.WatchedAttributes.SetInt("giftedMagic", 1);
                byEntity.World.PlaySoundAt(new AssetLocation("sounds/block/glass"), byEntity);

                if (byEntity is EntityPlayer player)
                {
                    if (byEntity.World.Side == EnumAppSide.Server)
                    {
                        IServerPlayer serverPlayer = byEntity.World.PlayerByUid(player.PlayerUID) as IServerPlayer;
                        serverPlayer.SendMessage(GlobalConstants.GeneralChatGroup, Lang.Get("worldofthalorim:BlessingCrystalTrue"), EnumChatType.Notification, null);
                    }
                }

                slot.TakeOut(1);
                slot.MarkDirty();
            }
            else
            {
                if (byEntity is EntityPlayer player)
                {
                    if (byEntity.World.Side == EnumAppSide.Server)
                    {
                        IServerPlayer serverPlayer = byEntity.World.PlayerByUid(player.PlayerUID) as IServerPlayer;
                        serverPlayer.SendMessage(GlobalConstants.GeneralChatGroup, Lang.Get("worldofthalorim:BlessingCrystalFalse"), EnumChatType.Notification, null);
                    }
                }
            }
        }
        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            dsc.AppendLine(Lang.Get("worldofthalorim:BlessingCrystalDesc"));
        }
    }
}
