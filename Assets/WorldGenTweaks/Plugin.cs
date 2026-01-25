using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Crafting.Construction;
using HarmonyLib;
using Level_Manager;
using Terrain.Platforms;
using Terrain.Platforms.Population.Construction.Machinery.Table_Map;
using Terrain.World;
using UnityEngine;
using UnityEngine.Pool;
using Utilities;

namespace WorldGenTweaks
{
    [BepInPlugin("worldgentweaks", "worldgentweaks", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> configLogDebugILPatches;
        public static ConfigEntry<bool> stompVanillaFirstRingClusterCount;
        public static ConfigEntry<int> minClustersPerSector;
        public static ConfigEntry<int> maxClustersPerSector;
        public static ConfigEntry<int> minIslandsPerCluster;
        public static ConfigEntry<int> maxIslandsPerCluster;
        public static ConfigEntry<bool> claimEveryIsland;
        public static ManualLogSource _logger;
        public static Harmony _Harmony;
        
        public Plugin()
        {
            Plugin._logger = this.Logger;
            configLogDebugILPatches = Config.Bind("Logging", "LogILPatchInstructions", false,
                "Log (at debug) the IL Patch instructions after the patch has been applied.");
            
            stompVanillaFirstRingClusterCount = Config.Bind("Clusters", "DisableSpawnSectorClusterCap", true,
                "The minimum number of clusters to generate per sector");
            minClustersPerSector = Config.Bind("Clusters", "MinPerSector", 20,
                "The minimum number of clusters to generate per sector");
            maxClustersPerSector = Config.Bind("Clusters", "MaxPerSector", 30,
                "The maximum number of clusters to generate per sector");
            
            minIslandsPerCluster = Config.Bind("Islands", "MinPerCluster", 20,
                "The minimum number of islands to generate per cluster");
            maxIslandsPerCluster = Config.Bind("Islands", "MaxPerCluster", 30,
                "The maximum number of islands to generate per cluster");

            claimEveryIsland = Config.Bind("Islands", "ClaimEveryIsland", false,
                "Allow claiming every island. No more `too big` and `fresco` and `titan` errors.");
            
            _Harmony = new Harmony("worldgentweaks");
            if (stompVanillaFirstRingClusterCount.Value)
            {
                _Harmony.Patch(
                    AccessTools.Method(typeof(WorldGenerator), "GenerateSectors"),
                    null,
                    null,
                    new HarmonyMethod(AccessTools.Method(typeof(Plugin), nameof(Plugin.StompSpawnSectorClusterCap)))
                );
            }

            if (claimEveryIsland.Value)
            {
                _Harmony.Patch(
                    AccessTools.Method(typeof(Ghost_CanConstructPlatformHomeIsland), "CheckIsland"),
                    null,
                    new HarmonyMethod(AccessTools.Method(typeof(Plugin), nameof(Plugin.CheckIsland)))
                );

                _Harmony.Patch(
                    AccessTools.Method(typeof(PlatformAdditionalData), "get_IsTooHeavyToMove"),
                    null,
                    new HarmonyMethod(AccessTools.Method(typeof(Plugin), nameof(Plugin.TooHeavy)))
                );

                _Harmony.Patch(
                    AccessTools.Method(typeof(Ghost_CanConstructPlatformTooHeavy), nameof(Ghost_CanConstructPlatformTooHeavy.CanConstruct)),
                    null,
                    new HarmonyMethod(AccessTools.Method(typeof(Plugin), nameof(Plugin.PlatformToHeavyConstruct)))
                );
            }
        }
        
        public void Awake()
        {
            Level.LoadSceneCallBack += LoadSceneCallBack;
            Logger.LogWarning("Awake!");
        }

        private void LoadSceneCallBack()
        {
            Logger.LogDebug("Load callback from world generator!");
            var ringData = Level.ConstantManager.ConstantManagers.WorldManager.RingGenerator.RingDatas;
            for (int i = 0; i < ringData.Length; i++)
            {
                Level.ConstantManager.ConstantManagers.WorldManager.RingGenerator.RingDatas[i]
                    .ClusterPerSectors = new MinMaxInt(Plugin.minClustersPerSector.Value, Plugin.maxClustersPerSector.Value);
            }
            
            var clusterData = Level.ConstantManager.ConstantManagers.WorldManager.ClusterGenerator.ClusterPerBiomes;
            for (int i = 0; i < clusterData.Length; i++)
            {
                Level.ConstantManager.ConstantManagers.WorldManager.ClusterGenerator.ClusterPerBiomes[i]
                    .IslandCountPerCluster = new MinMaxInt(Plugin.minIslandsPerCluster.Value, Plugin.maxIslandsPerCluster.Value);
            }
        }
        
        public static IEnumerable<CodeInstruction> StompSpawnSectorClusterCap(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            FieldInfo clusterAmountField = AccessTools.Field(typeof(WorldGenerator.GenerationSector), nameof(WorldGenerator.GenerationSector.ClusterAmount));

            var code = new List<CodeInstruction>(instructions);

            int noOpIndex = -1;

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].StoresField(clusterAmountField) && code[i-1].opcode == OpCodes.Ldc_I4_4)
                {
                    noOpIndex = i;
                }
            }
            
            if (noOpIndex != -1)
            {
                code[noOpIndex] = new CodeInstruction(OpCodes.Pop);
            }
            
            if (Plugin.configLogDebugILPatches.Value) Plugin._logger.LogDebug("New code: " + string.Join("\n", code));

            return code;
        }

        public static bool CheckIsland(bool __result, PlatformGlobalData globalData)
        {
            Plugin._logger.LogDebug("Allowing claiming of island.");
            return true;
        }

        public static bool TooHeavy(bool __result)
        {
            Plugin._logger.LogDebug("Skipping too heavy check.");
            return false;
        }
        
        public static bool PlatformToHeavyConstruct(bool __result)
        {
            Plugin._logger.LogDebug("Skipping paltform can construct check.");
            return true;
        }
        
        public static IEnumerable<CodeInstruction> HomeKiteInteract(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            FieldInfo clusterAmountField = AccessTools.Field(typeof(WorldGenerator.GenerationSector), nameof(WorldGenerator.GenerationSector.ClusterAmount));

            var code = new List<CodeInstruction>(instructions);

            int noOpIndex = -1;

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].StoresField(clusterAmountField) && code[i-1].LoadsConstant(4))
                {
                    noOpIndex = i;
                }
            }
            
            if (noOpIndex != -1)
            {
                code[noOpIndex] = new CodeInstruction(OpCodes.Pop);
            }
            
            if (Plugin.configLogDebugILPatches.Value) Plugin._logger.LogDebug("New code: " + string.Join("\n", code));

            return code;
        }
    }
}
