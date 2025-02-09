using System.Linq;
using BepInEx;
using BepInEx.Unity.Mono;
using HarmonyLib;
using NPC;
using Utilities;

public static class PoopPatch
{
    public static void Dropping(NpcDropping.NpcDroppingData dropping)
    {
        if (dropping.SResource != null && dropping.SResource.Resources != null)
        {
            dropping.SResource.Resources = dropping.SResource.Resources.ToList().Where(x => x.ItemID != ItemID.ID.Manure).ToArray();
        }
    }
}

[BepInPlugin("nopoop.sessional.dev", "NoPoop", "1.0.0")]
public class NoPoopPlugin : BaseUnityPlugin
{
    void Awake()
    {
        Harmony harmony = new Harmony("nopoop.sessional.dev");

        var poopCodePoint = AccessTools.Method(typeof(NpcDropping), "Dropping");
        var poopHook = AccessTools.Method(typeof(PoopPatch), nameof(PoopPatch.Dropping));
        harmony.Patch(poopCodePoint, new HarmonyMethod(poopHook));
    }
}
