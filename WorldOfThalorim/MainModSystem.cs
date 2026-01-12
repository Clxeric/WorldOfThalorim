using HarmonyLib;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace WorldOfThalorim
{
    public class WorldOfThalorimModSystem : ModSystem
    {
        private Harmony harmony;

        public override void Start(ICoreAPI api)
        {
            api.RegisterItemClass(Mod.Info.ModID + ".ItemClassBlessingCrysta", typeof(ItemClassBlessingCrystal));

            var harmony = new Harmony(Mod.Info.ModID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public override void Dispose()
        {
            harmony?.UnpatchAll(Mod.Info.ModID);
        }
    }
}
