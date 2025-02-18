using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AloftModFramework.Spawners;
using Balance;
using BepInEx.Logging;
using HarmonyLib;
using Terrain.Platforms.Population.BalanceSpawner;
using UnityEngine;
using Utilities;
using Logger = BepInEx.Logging.Logger;
using Object = UnityEngine.Object;

namespace AloftModLoader
{
    public class SpawnerLoader
    {
        private readonly ManualLogSource _logger;
        private readonly Harmony _harmony;

        private List<AloftModLoaderPopulationSpawner> populationChances;

        private List<SResourceBalancing.PopBalanceList> AlreadyAdjustedPopBalancedLists =
            new List<SResourceBalancing.PopBalanceList>();

        public SpawnerLoader(ManualLogSource logger, Harmony harmony, List<Object> assets)
        {
            this._logger = logger;
            this._harmony = harmony;

            populationChances = assets
                .FilterAndCast<PopulationSpawner>()
                .Select(x =>
                {
                    var spawner = new AloftModLoaderPopulationSpawner();

                    spawner.PopIDs = x.spawnedPopulations.Select(popRef => popRef.GetPopulationId()).ToArray();
                    spawner.Spreading = x.spreading;
                    spawner.biome = x.biome.GetBiome();
                    spawner.spawner = x.spawner.GetPopulationId();
                    spawner.Density = x.density;
                    spawner.DebugTitle = x.name;
                    spawner.SpawnAmount = x.spawnAmount;
                    spawner.ChanceToSpawn = Vector2.zero;
                    
                    return spawner;
                })
                .ToList();

        }

        public void Patch()
        {
            this._harmony.Patch(
                AccessTools.Method(typeof(SBalancingManager), nameof(SBalancingManager.GetResourceBalancing)),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(SpawnerLoader), nameof(SpawnerLoader.GetResourceBalancing)))
            );

            // Left in here for debugging... The resource spawning code is temperamental..
            this._harmony.Patch(
                AccessTools.Method(typeof(SPopBalancing), nameof(SPopBalancing.GetPopulationDataKits)),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(SpawnerLoader), nameof(SpawnerLoader.GetPopulationGroupDataKits)))
            );
        }
        
        public static List<SPopBalancing.PopulationGroupDataKit> GetPopulationGroupDataKits(List<SPopBalancing.PopulationGroupDataKit> __result, Vector3 popLocalPosition, float seed, bool islandIsHealthy, bool islandIsCleansed, SPopBalancing __instance)
        {
            if (__instance.name.StartsWith("CropBalancing_"))
            {
                Plugin.SpawnerLoader._logger.LogDebug(__instance.name + ": " + __result.Count + " : " + string.Join(",", __result.Select(x => (int)x.PopID).ToList()));
            }
            return __result;
        }

        public static SResourceBalancing GetResourceBalancing(SResourceBalancing __result, PopulationID.ID popSpawnerID,
            PopBalanceSpawner __instance)
        {
            // The intent here is to bandaid spawners with our new spawned resources
            //  this leaves out adding new spawners for now
            
            // The flow through here is roughly like this:
            // 1. SResourceBalancing __result = "For a random crop spawner"
            // 3. PopBalanceList PopListPerBiome = "For this biome"
            // 2. SPopBalancing PopBalanceData = "..."
            // 3. PopListPerRequirements PopBalancings = "When not corrupted"
            // 4. Pops = "option A if chances are met"
            // 5. PopIds = "One of: wheat variant 1, wheat variant 2"
            if (__result != null)
            {
                var spawnerBeingRequested = __result.SpawnerID;
                var spawnersForCurrentSpawner = Plugin.SpawnerLoader.populationChances
                    .Where(x => x.spawner == spawnerBeingRequested)
                    .ToList();
                if (spawnersForCurrentSpawner.Count == 0)
                {
                    Plugin.SpawnerLoader._logger.LogDebug("Did not find any relevant spawners for spawner " + spawnerBeingRequested);
                    return __result;
                }

                for (int popListPerBiomeIdx = 0;
                     popListPerBiomeIdx < __result.PopListPerBiome.Length;
                     popListPerBiomeIdx++)
                {
                    var popListPerBiome = __result.PopListPerBiome[popListPerBiomeIdx];
                    
                    if (Plugin.SpawnerLoader.AlreadyAdjustedPopBalancedLists.Contains(popListPerBiome)) continue;
                    Plugin.SpawnerLoader.AlreadyAdjustedPopBalancedLists.Add(popListPerBiome);

                    var spawnersForCurrentBiome =
                        spawnersForCurrentSpawner
                            .Where(x => x.biome == popListPerBiome.BiomeID)
                            .ToList();

                    if (spawnersForCurrentBiome.Count == 0)
                    {
                        Plugin.SpawnerLoader._logger.LogDebug("Did not find any relevant spawners for biome " + popListPerBiome.BiomeID);
                        continue;
                    }
                    
                    
                    for (int balancePerTagIdx = 0;
                         balancePerTagIdx < popListPerBiome.PopBalanceData.PopBalancings.Length;
                         balancePerTagIdx++)
                    {
                        Plugin.SpawnerLoader._logger.LogDebug("Attaching new spawning populations to " + spawnerBeingRequested + ": " + string.Join(",", spawnersForCurrentBiome.Cast<SPopBalancing.PopChance>().ToList()));
                        // this is a struct level, weird things happen here?
                        // sometimes things come back returned by value instead of reference?
                        // https://stackoverflow.com/a/25171957 ?
                        // a very common source of breaking when trying to rewrite parts of this...
                        popListPerBiome.PopBalanceData.PopBalancings[balancePerTagIdx].Pops = popListPerBiome.PopBalanceData.PopBalancings[balancePerTagIdx].Pops.AddRangeToArray(spawnersForCurrentBiome.Cast<SPopBalancing.PopChance>().ToArray());
                        // when debugging it can help to just replace all pops
                        //popListPerBiome.PopBalanceData.PopBalancings[balancePerTagIdx].Pops =
                        //    spawnersForCurrentBiome.Cast<SPopBalancing.PopChance>().ToArray();
                        
                        var evenChanceToSpawn = 1.0f / popListPerBiome.PopBalanceData.PopBalancings[balancePerTagIdx].Pops.Length;
                        for (int popIdx = 0;
                             popIdx < popListPerBiome.PopBalanceData.PopBalancings[balancePerTagIdx].Pops.Length;
                             popIdx++)
                        {
                            var chance = new Vector2(evenChanceToSpawn * popIdx, evenChanceToSpawn * (popIdx + 1));
                            Plugin.SpawnerLoader._logger.LogDebug("Chance set to : " + chance);
                            popListPerBiome.PopBalanceData.PopBalancings[balancePerTagIdx].Pops[popIdx].ChanceToSpawn = chance;
                        }
                    }
                }
            }
            return __result;
        }
    }
}
