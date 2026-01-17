using HarmonyLib;
using ProtoBuf;
using System.Diagnostics;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.Server;

namespace WorldOfThalorim
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PacketServerToClientSyncGiftedMagic//Серверный пакет для клиента, синхронизация допусков к магии
    {
        public long EntityId;
        public int giftedMagic;
    }
    public class WorldOfThalorimModSystem : ModSystem
    {
        private Harmony harmony;

        ICoreServerAPI sapi;
        IServerNetworkChannel serverChannel;

        ICoreClientAPI capi;
        IClientNetworkChannel clientChannel;

        public override void Start(ICoreAPI api)
        {
            api.RegisterItemClass(Mod.Info.ModID + ".ItemClassBlessingCrysta", typeof(ItemClassBlessingCrystal));

            var harmony = new Harmony(Mod.Info.ModID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;

            serverChannel = api.Network.RegisterChannel("worldofthalorim")
                .RegisterMessageType(typeof(PacketServerToClientSyncGiftedMagic));
        }
        public override void StartClientSide(ICoreClientAPI api)
        {
            capi = api;

            clientChannel = api.Network.RegisterChannel("worldofthalorim")
                .RegisterMessageType(typeof(PacketServerToClientSyncGiftedMagic))
                .SetMessageHandler<PacketServerToClientSyncGiftedMagic>(OnClientReceiveSyncGiftedMagic);
        }
        public void SendTrySyncGiftedMagic(EntityAgent entity, int currentGiftedMagic,IServerPlayer serverPlayer)
        {

            if (serverChannel == null)
            {
                //sapi.Logger.Error("[WorldOfThalorim] Ошибка сети(типа важно)");
                return;
            }

            try
            {
                long entityId = entity.EntityId;
                if (entityId == 0)
                {
                    //Debug.WriteLine("[WorldOfThalorim] Ошибка сети ник говны");
                    return;
                }

                var packet = new PacketServerToClientSyncGiftedMagic()
                {
                    EntityId = entityId,
                    giftedMagic = currentGiftedMagic
                };
                serverChannel.SendPacket(packet, serverPlayer);
            }
            catch (System.Exception e)
            {
                Debug.WriteLine($"[WorldOfThalorim] Ошибка сети: {e.Message}");
            }
        }

        private void OnClientReceiveSyncGiftedMagic(PacketServerToClientSyncGiftedMagic packet)
        {
            Entity entity = capi.World.GetEntityById(packet.EntityId);

            entity.WatchedAttributes.SetInt("giftedMagic", packet.giftedMagic);

            Debug.WriteLine($"[WorldOfThalorim] получен парраметр {packet.giftedMagic}");
        }
        public override void Dispose()
        {
            harmony?.UnpatchAll(Mod.Info.ModID);
        }
    }
}
