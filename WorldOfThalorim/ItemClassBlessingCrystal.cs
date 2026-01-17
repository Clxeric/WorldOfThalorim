using HarmonyLib;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Text;
using Vintagestory;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.Server;

namespace WorldOfThalorim
{
    public class ItemClassBlessingCrystal : Item
    {
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (byEntity == null)
            {
                return;
            }
            

            handling = EnumHandHandling.Handled;
            byEntity.AnimManager.StartAnimation("interactfirmgrip");
        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (byEntity == null) return false; 

            if (secondsUsed < 1.5f) //досрочное завершение
            {
                return true;
            }

            return false;
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            byEntity.AnimManager.StopAnimation("interactfirmgrip");

            if (byEntity == null) return;

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

                        var modsystem = byEntity.Api.ModLoader.GetModSystem<WorldOfThalorimModSystem>();

                        try
                        {

                            if (modsystem != null)
                            {
                                modsystem.SendTrySyncGiftedMagic(byEntity, 1, serverPlayer);
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.WriteLine($"[WorldOfThalorim] Error server send: {e.Message}");
                        }
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

                        var modsystem = byEntity.Api.ModLoader.GetModSystem<WorldOfThalorimModSystem>();
                        try
                        {

                            if (modsystem != null)
                            {
                                modsystem.SendTrySyncGiftedMagic(byEntity, 1, serverPlayer);
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.WriteLine($"[WorldOfThalorim] Error server send: {e.Message}");
                        }
                    }
                }
            }
            if (byEntity.World.Side == EnumAppSide.Client)
            {
                Debug.WriteLine($"[WorldOfThalorim] Клиент OnHeldInteractStop {giftedMagic}");
            }
            if (byEntity.World.Side == EnumAppSide.Server)
            {
                Debug.WriteLine($"[WorldOfThalorim] Сервер OnHeldInteractStop {giftedMagic}");
            }
        }
        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            dsc.AppendLine(Lang.Get("worldofthalorim:BlessingCrystalDesc"));
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        }
    }
}
