using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using Level_Manager;
using Scriptable_Objects;
using UnityEngine;
using Utilities;

namespace AloftModLoader
{
    public class CraftingRecipeLoader
    {
        private readonly ManualLogSource _logger;
        private readonly Harmony _harmony;
        private readonly List<AloftModLoaderCraftingRecipe> _recipes;
        private readonly List<ScriptableCraftRecipeGroup> _stations;

        private IList _patchedGroups = new List<SRecipeManager.CraftingStation>();

        public CraftingRecipeLoader(ManualLogSource logger, Harmony harmony, List<Object> assets)
        {
            this._logger = logger;
            this._harmony = harmony;

            this._stations = assets
                .FilterAndCast<AloftModFramework.Crafting.CraftingStation>()
                .Select(x =>
                {
                    var recipeGroup = ScriptableObject.CreateInstance<ScriptableCraftRecipeGroup>();

                    recipeGroup.DisplayName = x.stationName;
                    recipeGroup.StationType = (SRecipeManager.CraftingStation) x.stationId;
                    
                    return recipeGroup;
                })
                .ToList();
            
            this._recipes = assets
                .FilterAndCast<AloftModFramework.Crafting.CraftingRecipe>()
                .Select(x =>
                {
                    var recipe = ScriptableObject.CreateInstance<AloftModLoaderCraftingRecipe>();
                    
                    logger.LogDebug("Loaded recipe to craft " + x.outputItem.GetItemId());

                    recipe.Input = x.inputItems.Select(input => input.GetItemId()).ToArray();
                    recipe.Output = new ScriptableCrafting.CraftingCostClass(x.outputItem.GetItemId(), x.quantity);

                    
                    
                    if (x.attachToStation)
                    {
                        recipe.craftingStation = x.craftingStation.GetStation();
                        var stationToAttachTo = this._stations
                            .FirstOrDefault(station => station.StationType == x.craftingStation.GetStation());
                        if (stationToAttachTo != null)
                        {
                            logger.LogDebug("Attaching recipe for item " + x.outputItem.GetItemId() + " to station " + recipe.craftingStation);
                            stationToAttachTo.Recipes = stationToAttachTo.Recipes.AddToArray(recipe);
                        }
                    }

                    return recipe;
                })
                .ToList();
        }

        public void Patch()
        {
            _harmony.Patch(
                AccessTools.Method(typeof(SRecipeManager), nameof(SRecipeManager.GetRecipes)),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(CraftingRecipeLoader), nameof(CraftingRecipeLoader.GetRecipes)))
            );
        }

        public static ScriptableCraftRecipeGroup GetRecipes(ScriptableCraftRecipeGroup __result, SRecipeManager.CraftingStation stationType)
        {
            if (stationType == SRecipeManager.CraftingStation.All && !Plugin.CraftingRecipeLoader._patchedGroups.Contains(stationType))
            {
                // This all flow is invoked when trying to find the recipes to throw into the page
                // it should contain every recipe that exists in the game so that the visualizers
                // for stations can render all the options.
                Plugin.CraftingRecipeLoader._logger.LogDebug("Attaching " + Plugin.CraftingRecipeLoader._recipes.Count + " new recipes to the ALL station.");
                Plugin.CraftingRecipeLoader._patchedGroups.Add(stationType);
                __result.Recipes = __result.Recipes.AddRangeToArray(Plugin.CraftingRecipeLoader._recipes.Cast<ScriptableCraftRecipe>().ToArray());
            }
            
            // TODO: find a way to erase this auto-unlock of recipes here (using the unlock manager?)
            Level.CraftingManager.UnlockRecipe.UnlockRecipes(Plugin.CraftingRecipeLoader._recipes.Select(recipe => recipe.Output.ItemID).ToArray());

            if (__result == null)
            {
                // Not finding a recipe group here for a station means that the station
                // that is being asked for must be a new station (not vanilla)
                var station = Plugin.CraftingRecipeLoader._stations
                    .FirstOrDefault(x => x.StationType == stationType);
                Plugin.CraftingRecipeLoader._logger.LogDebug("Returning recipes for station " + stationType);
                return station;
            }
            
            if (!Plugin.CraftingRecipeLoader._patchedGroups.Contains(stationType))
            {
                var stationRecipes = Plugin.CraftingRecipeLoader._recipes
                    .Where(x => x.craftingStation == stationType)
                    .Cast<ScriptableCraftRecipe>()
                    .ToArray();
                Plugin.CraftingRecipeLoader._logger.LogDebug("Attaching " + stationRecipes.Length + " new recipes to station " + stationType);
                // finding a result originally implies that the station already exists (vanilla)
                // but its possible that a recipe was attached to an existing station, and this
                // takes care of tacking those results into the existing group.
                Plugin.CraftingRecipeLoader._patchedGroups.Add(stationType);
                __result.Recipes = __result.Recipes.AddRangeToArray(stationRecipes);
            }

            return __result;
        }
    }
}
