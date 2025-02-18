using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AloftModFramework.Construction;
using BepInEx.Logging;
using Crafting.Construction.EZSnap;
using HarmonyLib;
using Level_Manager;
using Scriptable_Objects;
using UI.Building;
using UnityEngine;
using Utilities;
using Logger = UnityEngine.Logger;

namespace AloftModLoader
{
    public class ConstructionLoader
    {
        private readonly ManualLogSource _logger;
        private readonly Harmony _harmony;
        private readonly List<ScriptableBuildingTab> _constructionCategories;
        private readonly List<ScriptableCrafting> _constructionBlueprints;

        private bool _learnedConstructionBlueprints = false;
        private bool _customTabsAdded = false;

        public ConstructionLoader(ManualLogSource logger, Harmony harmony, List<Object> assets)
        {
            this._logger = logger;
            this._harmony = harmony;

            var constructionCategories = assets
                .FilterAndCast<AloftModFramework.Construction.ConstructionCategory>()
                .ToList();

            // The act of adding a `parent` tab turns this in to a subtab.
            // The design of Alofts construction window makes this a terminal node
            // because there is no sub tab of a sub tab.
            // Constructing these first and setting their subtabs to an empty array
            // then makes sense because they will be unable to use more subtabs.
            var tabsWithParents = constructionCategories
                .Where(x => x.useParentCategory)
                .Select(x =>
                {
                    var buildingTab = ScriptableObject.CreateInstance<AloftModLoaderBuildingTab>();

                    buildingTab.Category = (ScriptableBuildingTab.BuildingCategory) x.id;
                    buildingTab.DisplayName = x.categoryName;
                    buildingTab.DisplayIcon = x.displayIcon;
                    buildingTab.SecondaryIcon = x.secondaryIcon;
                    
                    buildingTab.SubTabs = new ScriptableBuildingTab[0];

                    buildingTab.parentCategory = x.parentCategory.GetCategory();
                    buildingTab.hasParentCategory = x.useParentCategory;
                    
                    return buildingTab;
                })
                .ToList();

            var tabsWithoutParents = constructionCategories
                .Where(x => !x.useParentCategory)
                .Select(x =>
                {
                    var buildingTab = ScriptableObject.CreateInstance<ScriptableBuildingTab>();

                    buildingTab.Category = (ScriptableBuildingTab.BuildingCategory) x.id;
                    buildingTab.DisplayName = x.categoryName;
                    buildingTab.DisplayIcon = x.displayIcon;
                    buildingTab.SecondaryIcon = x.secondaryIcon;
                    
                    buildingTab.SubTabs = tabsWithParents
                        .Where(subTab => subTab.hasParentCategory)
                        .Where(subTab => subTab.parentCategory == (ScriptableBuildingTab.BuildingCategory) x.id)
                        .ToArray();

                    return buildingTab;
                }).ToList();

            this._constructionCategories = tabsWithoutParents.Union(tabsWithParents).ToList();

            this._constructionBlueprints = assets
                .FilterAndCast<ConstructionBlueprint>()
                .Select(x =>
                {
                    var blueprint = ScriptableObject.CreateInstance<ScriptableCrafting>();

                    blueprint.ID = x.populationData.GetPopulationId();

                    var popData = Plugin.EntityLoader.GetDataForId(x.populationData.GetPopulationId());
                    blueprint.PopData = popData;

                    // Because population data has a reference to the `ScriptableCrafting` object
                    // (the blueprint) that is abstracted away from the framework, the attaching
                    // happens here. The primary reason being that neither the PopulationData
                    // or the ScriptableCrafting actually exist in the concept of the mod loader.
                    // But, if a blueprint exists for the framework PopData and it is set to
                    // learn it, at some point during loading the data this should visit that
                    // reference and set it (a bit later than normal, but still plenty early).
                    var modLoaderPopData = popData as AloftModLoaderPopulationData;
                    if (modLoaderPopData != null)
                    {
                        if (modLoaderPopData.canBeLearnedFromSketching &&
                            modLoaderPopData.blueprintLearnedFromSketching == x)
                        {
                            popData.SketchbookCraftingRef = blueprint;
                        }
                    }
                    popData.ScriptableCrafting = blueprint;
                    
                    blueprint.DisplayName = x.displayName;
                    blueprint.DisplayDescription = x.description;
                    blueprint.DisplaySprite = x.sprite;

                    blueprint.HideInBuildMenu = x.hideInBuildMenu;
                    blueprint.Category = x.category.GetCategory();
                    _logger.LogDebug("Category was: " + x.category.GetCategory());
                    blueprint.Variants = x.variants.Select(variant => variant.GetPopulationId()).ToArray();
                    blueprint.IsVariantOf = x.isVariantOf.GetPopulationId();

                    blueprint.DefaultScale = x.defaultScale;
                    blueprint.CraftingCost = x.craftingCost.Select(cost =>
                        new ScriptableCrafting.CraftingCostClass(cost.GetItemId(), cost.quantity)).ToArray();
                    blueprint.HammerCost = x.hammerCost.Select(cost =>
                        new ScriptableCrafting.CraftingCostClass(cost.GetItemId(), cost.quantity)).ToArray();
                    blueprint.PopToUnlockAsWell =
                        x.unlockedPopulations.Select(unlocks => unlocks.GetPopulationId()).ToArray();
                    blueprint.AudioType = x.audioType.GetConstructionMaterial();
                    
                    return blueprint;
                })
                .ToList();
        }

        public void Patch()
        {
            
            // Has to run prefix because during this method it dynamically appends visualizers to the
            //  UI for the tabs, if the method isn't done already (i.e. if it is postfix), the first
            //  time the UI is rendered no custom tabs appear.
            _harmony.Patch(
                AccessTools.Method(typeof(UI_BuildingMenu), nameof(UI_BuildingMenu.Tabs_Initialize)),
                new HarmonyMethod(AccessTools.Method(typeof(ConstructionLoader), nameof(ConstructionLoader.Tabs_Initialize)))
            );

            _harmony.Patch(
                AccessTools.Method(typeof(ScriptableCraftingManager), nameof(ScriptableCraftingManager._GetCrafting)),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(ConstructionLoader),
                    nameof(ConstructionLoader._GetCrafting)))
            );
            
            _harmony.Patch(
                AccessTools.Method(typeof(SEzSnapManager), nameof(SEzSnapManager.GetSnapData)),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(ConstructionLoader),
                    nameof(ConstructionLoader.GetSnapData)))
            );
        }

        public static void Tabs_Initialize(UI_BuildingMenu __instance)
        {
            if (!Plugin.ConstructionLoader._customTabsAdded)
            {
                Plugin.ConstructionLoader._customTabsAdded = true;
                
                // During load, SubTabs are appended to custom tabs
                //  however that doesn't solve the problem of sticking new subtabs
                //  into existing tabs, which is what this is for.
                foreach (var category in Plugin.ConstructionLoader._constructionCategories)
                {
                    var categoryAsChildTab = category as AloftModLoaderBuildingTab;
                    if (categoryAsChildTab != null && categoryAsChildTab.hasParentCategory)
                    {
                        var parentCategory =
                            __instance.ScriptableTabs.FirstOrDefault(x =>
                                x.Category == categoryAsChildTab.parentCategory);
                        if (parentCategory != null)
                        {
                            parentCategory.SubTabs = parentCategory.SubTabs.AddToArray(category);
                        }
                    }
                    else
                    {
                        __instance.ScriptableTabs =
                            __instance.ScriptableTabs.AddToArray(category);                        
                    }
                }
                
            }
        }

        public static ScriptableCrafting _GetCrafting(ScriptableCrafting __result, PopulationID.ID id)
        {

            if (!Plugin.ConstructionLoader._learnedConstructionBlueprints)
            {
                Plugin.ConstructionLoader._learnedConstructionBlueprints = true;
                // TOOD: should have a different way of setting up learning blueprints for building?
                Plugin.ConstructionLoader._logger.LogDebug("Force learning blueprints!");
                Plugin.ConstructionLoader._constructionBlueprints.ForEach(blueprint =>
                {
                    Plugin.ConstructionLoader._logger.LogDebug("Learning blueprint " + blueprint.DisplayName);
                    Level.CraftingManager.UnlockRecipe.UnlockNewRecipeBuilding(blueprint.PopData.PopulationID);
                });
            }
            
            var blueprint = Plugin.ConstructionLoader._constructionBlueprints.FirstOrDefault(x => x.ID == id);
            if (blueprint != null)
            {
                return blueprint;
            }
            return __result;
        }

        public static SEzSnapData GetSnapData(SEzSnapData __result, PopulationID.ID populationID)
        {
            if (__result == null)
            {
                var blueprint = Plugin.ConstructionLoader._constructionBlueprints.FirstOrDefault(x => x.PopData.PopulationID == populationID);
                if (blueprint != null)
                {
                    var aloftModPopData = blueprint.PopData as AloftModLoaderPopulationData;
                    if (aloftModPopData != null)
                    {
                        var snapData = aloftModPopData.prefab.GetComponent<EzSnapIO>();
                        if (snapData != null) return snapData.EzSnapData;
                    }
                }
            }

            return __result;
        }
    }
}
