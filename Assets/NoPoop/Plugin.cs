using System.Linq;
using BepInEx;
using HarmonyLib;
using NPC;
using Utilities;

namespace NoPoop
{
    [BepInPlugin("nopoop.sessional.dev", "NoPoop", "1.1")]
    public class NoPoopPlugin : BaseUnityPlugin
    {
        void Awake()
        {
            Harmony harmony = new Harmony("nopoop.sessional.dev");

            var poopCodePoint = AccessTools.Method(typeof(NpcDropping), "Dropping");
            var poopHook = AccessTools.Method(typeof(NoPoopPlugin), nameof(NoPoopPlugin.Dropping));
            harmony.Patch(poopCodePoint, new HarmonyMethod(poopHook));
        }
        
        public static void Dropping(NpcDropping.NpcDroppingData dropping)
        {
            if (dropping.SResource != null && dropping.SResource.Resources != null)
            {
                dropping.SResource.Resources = dropping.SResource.Resources.ToList().Where(x => x.ItemID != ItemID.ID.Manure).ToArray();
            }
        }
    }
}
