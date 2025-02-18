using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AloftModFramework.Cooking;
using BepInEx.Logging;
using HarmonyLib;
using Inventory;
using Level_Manager;
using Scriptable_Objects.Consumable;
using Scriptable_Objects.Cooking;
using UI;
using UnityEngine;
using Utilities;

namespace AloftModLoader
{
    public class CookingLoader
    {
        private readonly Harmony _harmony;
        private readonly ManualLogSource _logger;

        private List<SCookMeal> Meals;
        private List<SCookRecipe> CookingRecipes;

        public CookingLoader(ManualLogSource logger, Harmony harmony, List<Object> allAssets)
        {
            this._harmony = harmony;
            this._logger = logger;

            Meals = allAssets
                .FilterAndCast<Meal>()
                .Select(x =>
                    {
                        var meal = ScriptableObject.CreateInstance<SCookMeal>();

                        meal.ItemID = x.item.GetItemId();
                        meal.StatDuration = x.statDuration;
                        meal.StatHpMaxAmount = x.statHpMaxAmount;
                        meal.TriggerConditions = x.triggerConditions.Select(condition => condition.GetConditionTrigger()).ToArray();
                        meal.Edible = x.edible;
                        meal.CookPreventBurning = x.preventBurning;
                        meal.IngredientType = x.ingredientType.GetIngredientType();
                        return meal;
                    }
                ).ToList();

            CookingRecipes = allAssets
                .FilterAndCast<CookingRecipe>()
                .Select(x =>
                {
                    SCookRecipe recipe;
                    if (x.isPerfectRecipe)
                    {
                        recipe = ScriptableObject.CreateInstance<SCookRecipePerfect>();
                        ((SCookRecipePerfect)recipe).Inputs = x.inputs.Select(input => input.GetItemId()).ToArray();
                    }
                    else
                    {
                        recipe = ScriptableObject.CreateInstance<SCookRecipeBastard>();
                        ((SCookRecipeBastard)recipe).RecipeVariations = x.variations;
                    }

                    recipe.Output = x.output.GetItemId();
                    recipe.IsPerfectRecipe = x.isPerfectRecipe;
                    recipe.Category = x.category.GetCategory();
                    recipe.UnlockableByRecipePage = x.unlockableByRecipePage;
                    
                    return recipe;
                })
                .ToList();
        }


        public void Patch()
        {
            this._harmony.Patch(
                AccessTools.Method(typeof(SCookingManager), nameof(SCookingManager.GetMeal)),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(CookingLoader), nameof(CookingLoader.GetMeal)))
            );
            
            this._harmony.Patch(
                AccessTools.Method(typeof(SCookingManager), nameof(SCookingManager.GetMealRecipe)),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(CookingLoader), nameof(CookingLoader.GetMealRecipe)))
            );
            
            this._harmony.Patch(
                AccessTools.Method(typeof(SCookingManager), nameof(SCookingManager.GetMealFromRecipe)),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(CookingLoader), nameof(CookingLoader.GetMealFromRecipe)))
            );
            
            this._harmony.Patch(
                AccessTools.Method(typeof(CanvasCooking), nameof(CanvasCooking.GetInventoryItemStacks)),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(CookingLoader), nameof(CookingLoader.GetInventoryItemStacks)))
            );
            
            this._harmony.Patch(
                AccessTools.Method(typeof(CanvasCooking), nameof(CanvasCooking.GetRecipeItemStacks)),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(CookingLoader), nameof(CookingLoader.GetRecipeItemStacks)))
            );
            
            
            this._harmony.Patch(
                AccessTools.Method(typeof(SConsumableRecipePage), nameof(SConsumableRecipePage.GetRandomHiddenMeal)),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(CookingLoader), nameof(CookingLoader.GetRandomHiddenMeal)))
            );
        }

        public static SCookMeal GetMeal(SCookMeal __result, ItemID.ID mealID)
        {
            // Meals are really the `ingredients` or `products` of cooking.
            if (__result == null)
            {
                return Plugin.CookingLoader.Meals.FirstOrDefault(x => x.ItemID == mealID);
            }
            return __result;
        }
        
        public static SCookRecipe GetMealRecipe(SCookRecipe __result, ItemID.ID mealID)
        {
            if (__result == null)
            {
                var modRecipe = Plugin.CookingLoader.CookingRecipes.FirstOrDefault(x => x.Output == mealID);

                if (modRecipe is SCookRecipeBastard)
                {
                    // in the case where both recipes are `bastard` return vanilla.
                    return __result;
                }
                
                // in the case where our recipe is not a bastard (is perfect), we want to erase the bastard result with ours.
                return modRecipe;
            }

            return __result;
        } 
        
        public static SCookRecipe GetMealFromRecipe(SCookRecipe __result, ItemID.ID[] itemIDs)
        {
            if (__result == null || __result is SCookRecipeBastard)
            {
                var modRecipe = Plugin.CookingLoader.CookingRecipes.FirstOrDefault(x => x.IsCompatible(itemIDs));

                if (modRecipe is SCookRecipeBastard)
                {
                    // in the case where both recipes are `bastard` return vanilla.
                    return __result;
                }
                // in the case where our recipe is not a bastard (is perfect), we want to erase the bastard result with ours.
                return modRecipe;
            }

            return __result;
        }
        
        public static ItemStackV2[] GetInventoryItemStacks(ItemStackV2[] __result)
       {
           List<ItemStackV2> itemStackV2List = new List<ItemStackV2>();

           foreach (var meal in Plugin.CookingLoader.Meals)
           {
               if (meal.IngredientType != SCookRecipe.IngredientType.None && Crafting.Inventory.Inventory.HasItem(meal.ItemID, 1, true))
               {
                   ItemStackV2 itemStackV2 = new ItemStackV2();
                   itemStackV2.SetItemStackV2(meal.ItemID, Crafting.Inventory.Inventory.GetQty(meal.ItemID, true), null);
                   if (itemStackV2.Quantity > Crafting.Inventory.Inventory.GetQty(itemStackV2.ItemID))
                       itemStackV2.AddParameter(ItemParameterV2.ParameterIDEnum.UseStorage, 1);
                   itemStackV2List.Add(itemStackV2);
               }
           }

           return __result.AddRangeToArray(itemStackV2List.ToArray());
       }

       public static ItemStackV2[] GetRecipeItemStacks(ItemStackV2[] __result)
       {
           List<ItemStackV2> itemStackV2List = new List<ItemStackV2>();

           foreach (var recipe in Plugin.CookingLoader.CookingRecipes)
           {
               if (Level.CraftingManager.UnlockRecipe.MealRecipeIsDiscovered(recipe.Output))
               {
                   ItemStackV2 itemStackV2 = new ItemStackV2();
                   itemStackV2.SetItemStackV2(recipe.Output, recipe.HasAllIngredient() ? 1 : 0, null);
                   itemStackV2List.Add(itemStackV2);
               }
           }

           return __result.AddRangeToArray(itemStackV2List.ToArray());
       }

       public static ItemID.ID GetRandomHiddenMeal(ItemID.ID __result)
       {
           // TODO: we should be able to unlock these BEFORE having to unlock all the originals....
           if (__result == ItemID.ID.Empty)
           {
               List<ItemID.ID> idList = new List<ItemID.ID>();
               foreach (var recipe in Plugin.CookingLoader.CookingRecipes)
               {
                   if (recipe.UnlockableByRecipePage && !Level.CraftingManager.UnlockRecipe.MealRecipeIsDiscovered(recipe.Output, true))
                   {
                       idList.Add(recipe.Output);
                   }
               }

               return idList.Count == 0 ? __result : idList[UnityEngine.Random.Range(0, idList.Count)];
           }

           return __result;
       }
    }
}
