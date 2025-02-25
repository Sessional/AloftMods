using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AloftModFramework.Entities;
using BepInEx.Logging;
using HarmonyLib;
using Level_Manager;
using Scriptable_Objects;
using Terrain.Platforms.Population;
using UnityEngine;
using Utilities;
using Logger = BepInEx.Logging.Logger;
using Object = UnityEngine.Object;

namespace AloftModLoader
{
    public class EntityLoader
    {
        private readonly ManualLogSource _logger;
        private readonly Harmony _harmony;
        private readonly List<AloftModLoaderPopulationData> _populations;
        private readonly Material _interactableSelectableMaterial;

        public EntityLoader(ManualLogSource logger, Harmony harmony, List<Object> assets)
        {
            this._logger = logger;
            this._harmony = harmony;

            // interactible selectable material
            _logger.LogDebug("Loading material from standard workbench.");
            var workbench = Resources.Load("Platform Builder/Constructions/Machines/Pre_Construction_Workbench") as GameObject;
            var workbenchMeshRenderer = workbench.GetComponentsInChildren<MeshRenderer>().First();
            _interactableSelectableMaterial = workbenchMeshRenderer.material;
            
            _logger.LogDebug("Loading discovered populations.");
            this._populations = assets
                .FilterAndCast<Population>()
                .Select(x =>
                {
                    logger.LogDebug("Loading population data " + x.name + " at id " + x.id);
                    var populationData = ScriptableObject.CreateInstance<AloftModLoaderPopulationData>();

                    populationData.PopulationID = (PopulationID.ID)x.id;
                    
                    populationData.prefab = x.prefab;
                    populationData.prefab.GetComponentsInChildren<MeshRenderer>()
                        .ForEach(renderer =>
                        {
                            _logger.LogDebug("Replacing material with " + _interactableSelectableMaterial.name);
                            
                            var mainTex = renderer.material.GetTexture("_MainTex");
                            var normalTex = renderer.material.GetTexture("_BumpMap");
                            var detailMask = renderer.material.GetTexture("_DetailMask");
                            var color = renderer.material.GetColor("_Color");

                            var newMaterial = new Material(_interactableSelectableMaterial);
                            newMaterial.name = "AloftModLoader_InteractableSelectable_Material";
            
                            _logger.LogDebug("Adding textures to material: " + mainTex + " " + normalTex + " " + detailMask);
                            newMaterial.SetTexture("_TextureAlbedo", mainTex);
                            newMaterial.SetTexture("_TextureNormals", normalTex);
                            newMaterial.SetTexture("_TextureMask", detailMask);
                            newMaterial.SetVector("_ColorSelect", new Vector4(1.6f, 1.3f, 0.5f, 1));
                            newMaterial.SetColor("_Color", color);

                            renderer.material = newMaterial;
                        });
                    
                    populationData.BehaviourType = x.behaviorType.GetBehaviorType();
                    populationData.MultiStepBehaviour = x.multistepBehavior.GetMultistepBehavior();
                    populationData.LoadDistance = x.loadDistance;
                    populationData.PopDataTags = x.dataTags.Select(tag => tag.GetTag()).ToArray();
                    populationData.PrefabPaths = new string[0];

                    // Stash for later referencing when we construct blueprints.
                    populationData.canBeLearnedFromSketching = x.learnedFromSketching;
                    populationData.blueprintLearnedFromSketching = x.learnedBlueprint;

                    return populationData;
                })
                .ToList();
        }

        public ScriptablePopulationData GetDataForId(PopulationID.ID id)
        {
            // TODO: how can we find population data for vanilla populations?
            //  maintain a list?
            return this._populations.First(x => x.PopulationID == id);
        } 

        public void Patch()
        {
            this._harmony.Patch(
                AccessTools.Method(typeof(PopulationManager), nameof(PopulationManager.GetPopulationData)),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(EntityLoader), nameof(EntityLoader.GetPopulationDataFromPopulationManager)))
            );
            
            this._harmony.Patch(
                AccessTools.Method(typeof(PopulationManager), nameof(PopulationManager.GetPopulationData)),
                null,
                null,
                new HarmonyMethod(AccessTools.Method(typeof(EntityLoader), nameof(EntityLoader.MuteException)))
            );


            this._harmony.Patch(
                AccessTools.Method(typeof(ScriptablePopulationDataManager),
                    nameof(ScriptablePopulationDataManager.GetPopulation)),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(EntityLoader), nameof(EntityLoader.GetPopulationDataFromPopulationDataManager)))
            );
            
            this._harmony.Patch(
                AccessTools.Method(typeof(PopulationManager), nameof(PopulationManager.Initialize)),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(EntityLoader), nameof(EntityLoader.Initialize)))
            );

            this._harmony.Patch(
                AccessTools.Method(typeof(ScriptablePopulationData),
                    nameof(ScriptablePopulationData.GetPrefabGameObject)),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(EntityLoader), nameof(EntityLoader.GetPrefabGameObject)))
            );
        }

        public static IEnumerable<CodeInstruction> MuteException(IEnumerable<CodeInstruction> instructions,
            ILGenerator il)
        {
            var writeLineMethod = new Action<object>(Console.WriteLine);
            var writeLineMethodInfo = writeLineMethod.GetMethodInfo();
            
            var newInstructions = new List<CodeInstruction>(instructions);
            var nopIndex = newInstructions.FindIndex(x =>
                x.opcode == OpCodes.Call && ((MethodInfo)x.operand) == writeLineMethodInfo);
            
            if (nopIndex != -1)
            {
                var originalOp = newInstructions[nopIndex];
                newInstructions[nopIndex] = new CodeInstruction(OpCodes.Ret);
                newInstructions[nopIndex].labels = originalOp.labels;
                newInstructions[nopIndex].blocks = originalOp.blocks;
            }
            
            if (Plugin.configLogDebugILPatches.Value) Plugin.EntityLoader._logger.LogDebug("New code: " + string.Join("\n", newInstructions));
            return newInstructions;
        }
        
        public static ScriptablePopulationData GetPopulationDataFromPopulationManager(ScriptablePopulationData __result, PopulationManager __instance,
            PopulationID.ID populationID)
        {
            if (__result == null)
            {
                var entity = Plugin.EntityLoader._populations.FirstOrDefault(x => x.PopulationID == populationID);
                if (entity != null)
                {
                    Plugin.EntityLoader._logger.LogDebug("Found population data for " + entity.PopulationID);
                    return entity;
                }
            
                // This was the default before we patched the ret null instead of logging.
                Plugin.EntityLoader._logger.LogError("Unable to find an entity with entity id: " + populationID);
                return __instance.PopulationDataManager.AllPopulationData[0];
            }

            return __result;
        }
        
        public static ScriptablePopulationData GetPopulationDataFromPopulationDataManager(ScriptablePopulationData __result, PopulationID.ID popID)
        {
            if (__result == null)
            {
                var entity = Plugin.EntityLoader._populations.FirstOrDefault(x => x.PopulationID == popID);
                if (entity != null)
                {
                    Plugin.EntityLoader._logger.LogDebug("Found population data for " + entity.PopulationID);
                    return entity;
                }
            }
            return __result;
        }

        public static void Initialize(PopulationManager __instance)
        {
            if (Plugin.EntityLoader._populations.Count > 0 && Level.TerrainManager.PopulationManager.PopulationDataManager.AllPopulationData.Contains(
                    Plugin.EntityLoader._populations.First()))
            {
                Plugin.EntityLoader._logger.LogWarning("Extending population data twice.");
            }
            Plugin.EntityLoader._logger.LogDebug("Extending population data.");
            Level.TerrainManager.PopulationManager.PopulationDataManager.AllPopulationData =
                Level.TerrainManager.PopulationManager.PopulationDataManager.AllPopulationData.AddRangeToArray(
                    Plugin.EntityLoader._populations
                        .Cast<ScriptablePopulationData>()
                        .ToArray()
                );
        }

        public static GameObject GetPrefabGameObject(GameObject __result, ScriptablePopulationData __instance)
        {
            if (__result == null)
            {
                if (__instance is AloftModLoaderPopulationData)
                {
                    var prefab = ((AloftModLoaderPopulationData)__instance).prefab;
                    Plugin.EntityLoader._logger.LogDebug("Loading a custom prefab for population " + __instance.PopulationID + " as " + prefab.name);
                    return prefab;
                }
                else
                {
                    Plugin.EntityLoader._logger.LogDebug("Unable to find a prefab for population " + __instance.PopulationID);
                }
            }
            return __result;
        }
    }
}
