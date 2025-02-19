using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using Level_Manager;
using Scriptable_Objects.Combat;
using Object = UnityEngine.Object;

namespace AloftModLoader
{
    public class VanillaGameAssetLocator
    {
        private readonly ManualLogSource _logger;
        private readonly Harmony _harmony;

        public VanillaGameAssetLocator(ManualLogSource logger, Harmony harmony)
        {

            this._logger = logger;
            this._harmony = harmony;
        }

        public void Patch()
        {
            /*this._harmony.Patch(
                AccessTools.Method(typeof(ConstantManager), nameof(ConstantManager.Initialize)),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(VanillaGameAssetLocator), nameof(VanillaGameAssetLocator.Start)))
            );*/
        }

        public static void Start()
        {
            Plugin.AssetLocator._logger.LogDebug("Running late start!");
            
            var resources = Object.FindObjectsOfType<ScriptableEquipable>();
            Plugin.AssetLocator._logger.LogDebug("During start found " + resources.Length + " scriptable equipables.");

            var equipables = Level.ConstantManager.ConstantManagers.InventoryManager.Equipables;
            Plugin.AssetLocator._logger.LogDebug("During start found " + equipables.Length + " scriptable equipables.");
        }
    }
}
