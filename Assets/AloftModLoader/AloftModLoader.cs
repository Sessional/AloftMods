using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Unity.Mono;
using UnityEngine;
using AloftModFramework;
using Balance;
using Creator.Creator_IO;
using HarmonyLib;
using Inventory;
using Level_Manager;
using Scriptable_Objects;
using Scriptable_Objects.Consumable;
using Scriptable_Objects.Cooking;
using Terrain.Platforms.Population;
using Terrain.Platforms.Population.BalanceSpawner;
using Terrain.Platforms.Population.Construction.Machinery;
using Terrain.Platforms.Types;
using UI;
using UI.Building;
using Utilities;

namespace AloftModLoader {
    public static class LinqExtensions
    {
       public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
       {
           //source.ThrowIfNull("source");
           //action.ThrowIfNull("action");
           foreach (T element in source)
           {
               action(element);
           }
           return source;
       }
    }

    static class ItemPatches
    {
       public static ScriptableInventoryItem RewriteItemIdResult(ScriptableInventoryItem __result, ItemID.ID id)
       {
           if (__result == null)
           {
               return AloftModLoader.Items.FirstOrDefault(x => x.ID == id);
           }

           return __result;
       }
    }

    static class RecipePatches
    {
       private static IList PatchedGroups = new List<SRecipeManager.CraftingStation>();
       public static ScriptableCraftRecipeGroup RewriteRecipeResult(ScriptableCraftRecipeGroup __result, SRecipeManager.CraftingStation stationType)
       {
           if (stationType == SRecipeManager.CraftingStation.All)
           {
               // this flow looks very similar to the station type filter below... But... it's `ALL` so it doesn't do a filter
               // it really just adds everything.
               PatchedGroups.Add(stationType);
               __result.Recipes = __result.Recipes.AddRangeToArray(AloftModLoader.Recipes.ToArray());
           }

           // TODO: inappropriate to unlock recipes here, but until a new way is plugged in/discovered/wired up, this will do.
           Level.CraftingManager.UnlockRecipe.UnlockRecipes(AloftModLoader.Recipes.Select(recipe => recipe.Output.ItemID).ToArray());

           if (__result == null)
           {
               // This assumes that any new stations have a pre-built recipe group that is wired up for us when constructing recipes
               // this may or may not preclude people from adding new recipes into another mods workbench...
               __result = AloftModLoader.RecipeGroups.Where(x => x.StationType == stationType).FirstOrDefault();
           }
           else if (__result != null && !PatchedGroups.Contains(stationType))
           {
               PatchedGroups.Add(stationType);
               __result.Recipes = __result.Recipes.AddRangeToArray(AloftModLoader.Recipes.Where(x => x.CraftingStation == stationType).ToArray());
           }

           return __result;
       }
    }

    static class LocalizationPatches
    {
       public static Dictionary<string, string> LocalizationValues = new Dictionary<string, string>();
       public static void SetLanguage(int index)
       {
           LocalizationValues.Clear();
           var localizationName = Localization.GetLanguageName(index);
           var selectedLocalization = AloftModLoader.Localizations.FirstOrDefault(x => x.Language.Equals(localizationName));
           // try to match the language picked
           // when none is matched, try to grab whatever the first one loaded
           // if that doesn't work then our best effort attempt is over
           var localizationResource = selectedLocalization == null ? AloftModLoader.Localizations.FirstOrDefault() : null;

           if (localizationResource != null)
           {
               foreach (var entry in localizationResource.LocalizationFile.text.Split('\n'))
               {
                   if (string.IsNullOrEmpty(entry)) continue;

                   var splitEntry = entry.Split('\t');
                   if (splitEntry.Length == 2)
                   {
                       LocalizationValues.Add(splitEntry[0], splitEntry[1]);
                   }
                   else
                   {
                       Console.WriteLine("Error with entry: " + entry);
                   }
               }
           }
       }

       public static string GetLocalizedValue(string __result, string key)
       {
           if (string.IsNullOrEmpty(__result))
           {
               if (LocalizationValues.TryGetValue(key, out var value))
               {
                   return value;
               }
           }
           return __result;
       }
    }

    static class BuildingHooks
    {

       public static bool PopulationDataAdded = false;
       public static void InitListExtension()
       {
           if (!PopulationDataAdded)
           {
               PopulationDataAdded = true;
               Level.TerrainManager.PopulationManager.PopulationDataManager.AllPopulationData = Level.TerrainManager.PopulationManager.PopulationDataManager.AllPopulationData.AddRangeToArray(AloftModLoader.Buildings.ToArray());
           }
       }

       private static bool Learned = false;
       public static ScriptableCrafting GetCrafting(ScriptableCrafting __result, PopulationID.ID id)
       {

           if (!Learned)
           {
               Learned = true;
               // TOOD: should have a different way of setting up learning blueprints for building?
               Console.WriteLine("Learning blueprints!");
               AloftModLoader.BuildingBlueprints.ForEach(blueprint => {
                   Console.WriteLine("Learning blueprint " + blueprint.name);
                   Level.CraftingManager.UnlockRecipe.UnlockNewRecipeBuilding(blueprint.PopData.PopulationID);
               });
           }

           InitListExtension(); // TOOD: why is this not getting called somewhere else..?

           var loadedBuildng = AloftModLoader.BuildingBlueprints.FirstOrDefault(x => x.ID == id);
           if (loadedBuildng != null)
           {
               return loadedBuildng;
           }
           return __result;
       }

       public static ScriptablePopulationData GetPopulationData(ScriptablePopulationData __result, PopulationID.ID populationID)
       {
           // stomp, looks like the other one alwasy returns something stupid? WaterL
           if (AloftModLoader.Buildings != null && AloftModLoader.Buildings.Count > 0)
           {
               // TODO: it's more than just buildings, it's really any population...
               var loadedPop = AloftModLoader.Buildings.FirstOrDefault(x => x.PopulationID == populationID);
               if (loadedPop != null)
               {
                   return loadedPop;
               }
           }

           return __result;
       }

       public static ScriptablePopulationData GetPopulationData2(ScriptablePopulationData __result, PopulationID.ID popID)
       {
           return GetPopulationData(__result, popID);
       }

       public static GameObject GetPrefabGameObject(GameObject __result, ScriptablePopulationData __instance)
       {
           if (__result == null)
           {
               if (__instance is AloftModFrameworkPopulationData)
               {
                   return ((AloftModFrameworkPopulationData)__instance).Prefab;
               }
           }
           return __result;
       }

       public static bool CustomTabsAdded = false;
       public static void AddMoreTabs(UI_BuildingMenu __instance)
       {
           if (!CustomTabsAdded)
           {
               CustomTabsAdded = true;

               for (int i = 0; i < AloftModLoader.BuildingTabs.Count; i++)
               {
                   var tab = AloftModLoader.BuildingTabs[i];

                   if (tab.HasParentCategory)
                   {
                       var parentCategory = __instance.ScriptableTabs.FirstOrDefault(x => x.Category == tab.ParentCategory);
                       if (parentCategory != null)
                       {
                           parentCategory.SubTabs = parentCategory.SubTabs.AddToArray(tab);
                       }
                   } else
                   {
                       __instance.ScriptableTabs = __instance.ScriptableTabs.AddToArray(tab);
                   }
               }
           }
       }
    }

    static class WorldGenHooks
    {
       public static List<SResourceBalancing.PopBalanceList> AlreadyAdjustedPopBalancedLists = new List<SResourceBalancing.PopBalanceList>();
       public static SResourceBalancing GetResourceBalancing(SResourceBalancing __result, PopulationID.ID popSpawnerID, PopBalanceSpawner __instance)
       {
           if (__result != null)
           {
               var soulRef = __instance.SoulRef;
               if (soulRef != null)
               {
                   var spawnerType = __result.SpawnerID;
                   var platformData = soulRef.PlatformData;
                   var eligibleSpawners = AloftModLoader.ResourceSpawns.Where(x => x.SpawnerId == __result.SpawnerID).ToList();
                   if (eligibleSpawners.Count == 0) return __result;
                   foreach (SResourceBalancing.PopBalanceList popListPerBiome in __result.PopListPerBiome)
                   {
                       if (AlreadyAdjustedPopBalancedLists.Contains(popListPerBiome)) continue;
                       AlreadyAdjustedPopBalancedLists.Add(popListPerBiome);
                       var biome = popListPerBiome.BiomeID;
                       // TODO: match biome
                       //var popDataForBiome = eligibleSpawners.Where(x => x.Biome == resourceBalanceList.BiomeID).ToList();

                       for (int i = 0; i < popListPerBiome.PopBalanceData.PopBalancings.Length; i++)
                       {
                           // TODO: match requirements
                           if (eligibleSpawners.Count >= 0)
                           {
                               var spopBalancing = popListPerBiome.PopBalanceData;
                               spopBalancing.PopBalancings[i].Pops = spopBalancing.PopBalancings[i].Pops.AddRangeToArray(eligibleSpawners.ToArray());
                               //spopBalancing.PopBalancings[i].Pops = eligibleSpawners.ToArray();
                               // TODO: weight correctly rather than doing even weightings.
                               var evenChanceToSpawn = 1.0f / spopBalancing.PopBalancings[i].Pops.Length;
                               for (int popChanceIndex = 0; popChanceIndex < spopBalancing.PopBalancings[i].Pops.Length; popChanceIndex++)
                               {
                                   spopBalancing.PopBalancings[i].Pops[popChanceIndex].ChanceToSpawn = new Vector2(evenChanceToSpawn * popChanceIndex, evenChanceToSpawn * (popChanceIndex + 1));
                               }
                               Console.WriteLine(spopBalancing.PopBalancings[i]);
                           }
                       }
                   }
               }

           }
           return __result;
       }

       public static List<SPopBalancing.PopulationGroupDataKit> GetPopulationGroupDataKits(List<SPopBalancing.PopulationGroupDataKit> __result, Vector3 popLocalPosition, float seed, bool islandIsHealthy, bool islandIsCleansed, SPopBalancing __instance)
       {
           if (__instance.name.StartsWith("CropBalancing_"))
           {
               //Console.WriteLine(__instance.PopBalancings.Length + ": " + string.Join(",", __instance.PopBalancings.SelectMany(x => x.Pops.SelectMany(y => y.PopIDs.Select(z => (int) z))).ToList()));
               Console.WriteLine(__instance.name + ": " + __result.Count + " : " + string.Join(",", __result.Select(x => (int)x.PopID).ToList()));
           }
           return __result;
       }
    }

    public static class Cheats {
       public static bool ProcessCommand(ref bool __result, string input)
       {
           var parts = input.Split(' ');

           if (parts.Length > 0)
           {
               Console.WriteLine(input);
               switch (parts[0])
               {
                   case "showpop":
                       var island = Level.TerrainManager.PlatformManager.HomeIsland;
                       Console.WriteLine("Is island null? " + (island == null).ToString());
                       var pop = island.PopulationSouls.Values.FirstOrDefault(x => x.PopulationID == (PopulationID.ID) 400003);
                       Console.WriteLine("Is bee pop null? " + (pop == null).ToString());
                       Console.WriteLine("Pop location is " + pop.LocalPosition);
                       __result = true;
                       return false;
                   case "learn":
                       switch (parts[1])
                       {
                           case "building":
                               Console.WriteLine("Learning building!");
                               if (int.TryParse(parts[2], out var buildingId))
                               {
                                   Console.WriteLine("Learned building!");
                                   Level.CraftingManager.UnlockRecipe.UnlockNewRecipeBuilding((PopulationID.ID)buildingId);
                                   __result = true;
                                   return false;
                               }
                               break;
                           case "recipe":
                               if (int.TryParse(parts[2], out var recipeId))
                               {
                                   Level.CraftingManager.UnlockRecipe.LearnItemRecipe((ItemID.ID)recipeId);
                                   __result = true;
                                   return false;
                               }
                               break;
                       }
                       break;
                   case "spawn":
                       switch (parts[1])
                       {
                           case "pop":
                               if (int.TryParse(parts[2], out var popId))
                               {
                                   var population = AloftModLoader.Buildings.FirstOrDefault(x => x.PopulationID == (PopulationID.ID)popId);
                                   if (population != null)
                                   {
                                       PlatformAbstract cachedPlatform = Level.PlayerManager.AffectedByPlatform.CachedPlatform;
                                       var location = cachedPlatform.AffectedObjectParent.InverseTransformPoint(Level.PlayerManager.Anatomy.Pivot.position);
                                       var transform = Level.PlayerManager.Anatomy.Pivot;
                                       Console.WriteLine("Location is " + location.x + "," + location.y +"," + location.z);
                                       Console.WriteLine("Ghost prefab is" + population.ScriptableCrafting.CustomGhostPrefab.name);
                                       var spot = new Vector3(6010, 724, 26658);
                                       var spawnedObj = GameObject.Instantiate(population.ScriptableCrafting.CustomGhostPrefab, spot, Quaternion.identity);
                                       __result = true;
                                       return false;
                                   }
                               }
                               break;
                       }
                       break;
               }
           }
           return true;
       }

       public static void TweakCommandsOnStart(CanvasConsole __instance)
       {

       }
    }

    public static class CookingHooks
    {
       public static SCookMeal GetMeal(SCookMeal __result, ItemID.ID mealID)
       {
           if (__result == null)
           {
               return AloftModLoader.Meals.FirstOrDefault(x => x.ItemID == mealID);
           }

           return __result;
       }

       public static SCookRecipe GetMealRecipe(SCookRecipe __result, ItemID.ID mealID)
       {
           if (__result == null)
           {
               var modRecipe = AloftModLoader.CookingRecipes.FirstOrDefault(x => x.Output == mealID);

               if (modRecipe is SCookRecipeBastard)
               {
                   // in the case where both recipes are `bastard` return vanilla.
                   return __result;
               }
               else
               {
                   // in the case where our recipe is not a bastard (is perfect), we want to erase the bastard result with ours.
                   return modRecipe;
               }
           }

           return __result;
       }

       public static SCookRecipe GetMealFromRecipe(SCookRecipe __result, ItemID.ID[] itemIDs)
       {
           if (__result == null || __result is SCookRecipeBastard)
           {
               var modRecipe = AloftModLoader.CookingRecipes.FirstOrDefault(x => x.IsCompatible(itemIDs));

               /*
                * {
                   Console.WriteLine(x.name);
                   if (x is SCookRecipePerfect) return (x as SCookRecipePerfect).IsCompatible(itemIDs);
                   else if (x is SCookRecipeBastard) return (x as SCookRecipeBastard).IsCompatible(itemIDs);
                   return false;
               }*/

               if (modRecipe is SCookRecipeBastard)
               {
                   // in the case where both recipes are `bastard` return vanilla.
                   return __result;
               } else
               {
                   // in the case where our recipe is not a bastard (is perfect), we want to erase the bastard result with ours.
                   return modRecipe;
               }
           }

           return __result;
       }

       public static ItemStackV2[] GetInventoryItemStacks(ItemStackV2[] __result)
       {
           List<ItemStackV2> itemStackV2List = new List<ItemStackV2>();

           foreach (var meal in AloftModLoader.Meals)
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

           foreach (var recipe in AloftModLoader.CookingRecipes)
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
               foreach (var recipe in AloftModLoader.CookingRecipes)
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

    public class AloftModFrameworkPopulationData : ScriptablePopulationData
    {
       public GameObject Prefab;
    }

    public class AloftModFrameworkInternalCraftingRecipe : ScriptableCraftRecipe
    {
       public SRecipeManager.CraftingStation CraftingStation;
    }

    public class AloftModFrameworkInternalBuildingTab : ScriptableBuildingTab
    {
       public bool HasParentCategory = false;
       public BuildingCategory ParentCategory;
    }

    public class AloftModFrameworkPopChance : SPopBalancing.PopChance
    {
       public SCreatorFileAbstract.CreatorTagBiomeID Biome;
       public PopulationID.ID SpawnerId;
    }

    [BepInPlugin("aloftmodloader.sessional.dev", "Aloft Mod Loader", "1.0")]
    public class AloftModLoader : BaseUnityPlugin
    {
       const string GUID = "aloftmodloader.sessional.dev";
       const string NAME = "Aloft Mod Loader";
       const string VERSION = "0.1.1";

       public static Shader shaderEveryoneNeeds;

       static List<AssetBundle> bundles;
       static List<UnityEngine.Object> allAssets;
       public static List<ScriptableInventoryItem> Items;
       public static List<AloftModFrameworkInternalCraftingRecipe> Recipes;
       public static List<AloftModFrameworkInternalBuildingTab> BuildingTabs;
       public static List<ScriptableCraftRecipeGroup> RecipeGroups;
       public static List<ScriptableCrafting> BuildingBlueprints;
       public static List<AloftModFrameworkPopulationData> Buildings;
       public static List<AloftModFrameworkLocalization> Localizations;
       public static List<SCookMeal> Meals;
       public static List<SCookRecipe> CookingRecipes;
       public static List<AloftModFrameworkPopChance> ResourceSpawns;
       public static List<GameObject> GameObjects;

       public static Material hackyShader;

       public static BepInEx.Logging.ManualLogSource LoggerRef;

       public void Start()
       {
           Logger.LogInfo("Running!");
           string bundleDirectory = Path.Combine(Application.streamingAssetsPath, "amf");
           Logger.LogInfo("Bundle directory: " + bundleDirectory);
           var bundleNames = Directory.GetFiles(bundleDirectory);
           Logger.LogInfo("Bundle names: " + string.Join(",", bundleNames));

           shaderEveryoneNeeds = Shader.Find("Amplify/V2/DefaultPBR_Interactive");
           var whiteFallback = Resources.Load<Texture2D>("white");

           bundles = bundleNames
               .Where(x => !x.EndsWith(".manifest")) // TODO: order imports by this with dependencies rather than lucking out...
               .Distinct()
               .Select(x => {
                   Logger.LogInfo(string.Format("Loading asset bundle {0}", x));
                   return AssetBundle.LoadFromFile(x);
               }).ToList();
           allAssets = bundles.SelectMany(x => x.LoadAllAssets()).ForEach(x =>
           {
               DontDestroyOnLoad(x);
               x.hideFlags = HideFlags.HideAndDontSave;
           }).ToList();

           Items = allAssets
               .Where(x => x is AloftModFrameworkItem)
               .Cast<AloftModFrameworkItem>()
               .Select(x => {
                   var item = ScriptableObject.CreateInstance(typeof(ScriptableInventoryItem)) as ScriptableInventoryItem;
                   item.DisplayName = x.Name;
                   item.DisplayDescription = x.Description;
                   item.Category = x.Category;
                   item.DisplaySprite = x.DisplaySprite;
                   item.ID = (ItemID.ID)x.ItemId;
                   item.Type = x.Type;
                   item.Weight = x.Weight;
                   item.EquipType = x.EquipType;
                   item.AudioPickupID = x.AudioPickupID;
                   item.ItemTags = x.ItemTags;
                   item.hideFlags = HideFlags.HideAndDontSave;
                   return item;

               })
               .ToList();


           var aloftFrameworkRecipeGroups = allAssets
               .Where(x => x is AloftModFrameworkCraftingRecipeGroup)
               .Cast<AloftModFrameworkCraftingRecipeGroup>();

           RecipeGroups = aloftFrameworkRecipeGroups
               .Select(x =>
               {
                   var group = ScriptableObject.CreateInstance(typeof(ScriptableCraftRecipeGroup)) as ScriptableCraftRecipeGroup;

                   group.StationType = (SRecipeManager.CraftingStation) x.StationId;

                   return group;
               })
               .ToList();

           Recipes = allAssets
               .Where(x => x is AloftModFrameworkCraftingRecipe)
               .Cast<AloftModFrameworkCraftingRecipe>()
               .Select(x =>
               {
                   var recipe = ScriptableObject.CreateInstance(typeof(AloftModFrameworkInternalCraftingRecipe)) as AloftModFrameworkInternalCraftingRecipe;
                   Logger.LogInfo("Generating recipe: " + x.name);
                   recipe.Input = x.InputItems.Select(input => (ItemID.ID)input).ToArray();
                   recipe.hideFlags = HideFlags.HideAndDontSave;

                   ItemID.ID outputItemId;
                   if (x.OutputVanillaItem != ItemID.ID.Empty)
                   {
                       outputItemId = x.OutputVanillaItem;
                   }
                   else if (x.OutputModItem != null)
                   {
                       outputItemId = (ItemID.ID)x.OutputModItem.ItemId;
                   }
                   else
                   {
                       outputItemId = (ItemID.ID)x.OutputItemId;
                   }
                   recipe.Output = new ScriptableCrafting.CraftingCostClass(outputItemId, x.Quantity);
                   
                   if (x.AttachToExistingStation)
                   {
                       recipe.CraftingStation = x.Station;
                   }

                   var correspondingGroup = aloftFrameworkRecipeGroups.Where(group => group.Recipes.Contains(x)).FirstOrDefault();
                   if (correspondingGroup != null)
                   {
                       var recipeGroup = RecipeGroups.Where(group => group.StationType == (SRecipeManager.CraftingStation)correspondingGroup.StationId).FirstOrDefault();
                       recipeGroup.Recipes = recipeGroup.Recipes.AddItem(recipe).ToArray();
                       recipe.CraftingStation = (SRecipeManager.CraftingStation)correspondingGroup.StationId;
                   }

                   return recipe;
               })
               .ToList();


           BuildingTabs = allAssets
               .Where(x => x is AloftModFrameworkBuildingCategory)
               .Cast<AloftModFrameworkBuildingCategory>()
               .Select(x =>
               {
                   var buildingTab = ScriptableObject.CreateInstance<AloftModFrameworkInternalBuildingTab>();
                   buildingTab.Category = (ScriptableBuildingTab.BuildingCategory) x.BuildingCategoryId;
                   buildingTab.DisplayName = x.Name;
                   buildingTab.DisplayIcon = x.DisplayIcon;
                   buildingTab.SecondaryIcon = x.SecondaryIcon;
                   buildingTab.SubTabs = new ScriptableBuildingTab[0];

                   buildingTab.HasParentCategory = x.UseAParentCategory;
                   buildingTab.ParentCategory = x.ParentCategory;

                   return buildingTab;
               })
               .ToList();

           var buildingBlueprints = allAssets
               .Where(x => x is AloftModFrameworkBuildingBlueprint)
               .Cast<AloftModFrameworkBuildingBlueprint>()
               .ToList();

           //a building has no blueprint if and only if
           //there is no match of a building blueprint to our building data
           var BuildingsWithoutBlueprints = allAssets
               .Where(x => x is AloftModFrameworkBuildingData)
               .Cast<AloftModFrameworkBuildingData>()
               .Where(x => buildingBlueprints.FirstOrDefault(y => y.BuildingData.PopulationId == x.PopulationId) == null)
               .Select(x =>
               {
                   var populationData = ScriptableObject.CreateInstance(typeof(AloftModFrameworkPopulationData)) as AloftModFrameworkPopulationData;

                   populationData.PopulationID = (PopulationID.ID)x.PopulationId;
                   populationData.Prefab = x.InstancePrefab;
                   populationData.BehaviourType = x.BehaviourType;
                   populationData.MultiStepBehaviour = x.MultiStepBehaviour;
                   populationData.LoadDistance = x.LoadDistance;
                   populationData.PopDataTags = x.PopDataTags ?? new ScriptablePopulationData.PopDataTagID[0];
                   populationData.PrefabPaths = new string[0];

                   if (x.CanLearnViaSketchbook)
                   {
                       Logger.LogWarning(string.Format("A building {0} was marked with CanLearnViaSketchbook but it has no corresponding Blueprint to make it buildable.", x.name));
                   }

                   return populationData;
               })
               .ToList();

           ResourceSpawns = allAssets
               .Where(x => x is AloftModFrameworkSpawnedResource)
               .Cast<AloftModFrameworkSpawnedResource>()
               .Select(spawnedResource => {
                   var resourceBalancing = new AloftModFrameworkPopChance();

                   resourceBalancing.DebugTitle = spawnedResource.Name;
                   resourceBalancing.SpawnerId = spawnedResource.SpawnerId;
                   resourceBalancing.Biome = spawnedResource.Biome;
                   resourceBalancing.PopIDs = spawnedResource.PopulationIds.Select(x => (PopulationID.ID)x).ToArray();
                   resourceBalancing.SpawnAmount = new MinMaxInt(spawnedResource.SpawnAmountMin, spawnedResource.SpawnAmountMax);
                   resourceBalancing.Spreading = new MinMax(spawnedResource.SpreadingMin, spawnedResource.SpreadingMax);
                   resourceBalancing.Density = spawnedResource.Density;
                   resourceBalancing.ChanceToSpawn = Vector2.zero;

                   return resourceBalancing;
               })
               .ToList();

           var buildingsAndTheirBlueprints = allAssets
               .Where(x => x is AloftModFrameworkBuildingData)
               .Cast<AloftModFrameworkBuildingData>()
               .Where(x => buildingBlueprints.FirstOrDefault(y => y.BuildingData.PopulationId == x.PopulationId) != null)
               .Select(building =>
               {
                   Logger.LogInfo("Building building data:" + building.name);

                   var populationData = ScriptableObject.CreateInstance(typeof(AloftModFrameworkPopulationData)) as AloftModFrameworkPopulationData;
                   populationData.hideFlags = HideFlags.HideAndDontSave;
                   populationData.PopulationID = (PopulationID.ID)building.PopulationId;
                   populationData.Prefab = building.InstancePrefab;
                   populationData.BehaviourType = building.BehaviourType;
                   populationData.MultiStepBehaviour = building.MultiStepBehaviour;
                   populationData.LoadDistance = building.LoadDistance;
                   populationData.PopDataTags = building.PopDataTags ?? new ScriptablePopulationData.PopDataTagID[0];
                   populationData.PrefabPaths = new string[0];

                   var buildingBlueprint = buildingBlueprints.First(blueprint => blueprint.BuildingData == building);
                   Logger.LogInfo("Building corresponding blueprint data:" + buildingBlueprint.name);
                   var blueprintData = ScriptableObject.CreateInstance(typeof(ScriptableCrafting)) as ScriptableCrafting;
                   blueprintData.hideFlags = HideFlags.HideAndDontSave;
                   blueprintData.ID = populationData.PopulationID;
                   blueprintData.PopData = populationData;
                   blueprintData.DisplayName = buildingBlueprint.DisplayName;
                   blueprintData.DisplayDescription = buildingBlueprint.DisplayDescription;
                   blueprintData.DisplaySprite = buildingBlueprint.DisplaySprite;
                   blueprintData.HideInBuildMenu = buildingBlueprint.HideInBuildMenu;
                   blueprintData.Category = buildingBlueprint.GetCategory();
                   blueprintData.Variants = buildingBlueprint.Variants.Select(variant => (PopulationID.ID)variant.PopulationId).ToArray();
                   blueprintData.IsVariantOf = buildingBlueprint.IsVariantOf == null ? PopulationID.ID.Empty : (PopulationID.ID)buildingBlueprint.IsVariantOf.PopulationId;
                   blueprintData.DefaultScale = buildingBlueprint.DefaultScale;
                   blueprintData.CraftingCost = buildingBlueprint.CraftingCost;
                   blueprintData.HammerCost = buildingBlueprint.HammerCost;
                   blueprintData.PopToUnlockAsWell = buildingBlueprint.PopToUnlockAsWell;
                   blueprintData.AudioType = buildingBlueprint.AudioType;

                   populationData.ScriptableCrafting = blueprintData;
                   populationData.SketchbookCraftingRef = blueprintData;


                   return new
                   {
                       Blueprint = blueprintData,
                       Building = populationData,
                   };
               })
               .ToList();

           // TODO: is there blueprints that might not have been mapped to a building? If so, that's probably something we should say...
           BuildingBlueprints = buildingsAndTheirBlueprints.Select(x => x.Blueprint).ToList();

           var workbench = Resources.Load("Platform Builder/Constructions/Machines/Pre_Construction_Workbench") as GameObject;
           var workbenchMeshRenderer = workbench.GetComponentsInChildren<MeshRenderer>().First();
           var workbenchMaterial = workbenchMeshRenderer.material;
           Buildings = buildingsAndTheirBlueprints.Select(x => x.Building)
               .Union(BuildingsWithoutBlueprints)
               .ForEach(x =>
               {
                   x.Prefab.GetComponentsInChildren<MeshRenderer>().ForEach(mesh =>
                   {
                       var mainTex = mesh.material.GetTexture("_MainTex");
                       var normalTex = mesh.material.GetTexture("_BumpMap");
                       var detailMask = mesh.material.GetTexture("_DetailMask");

                       var mat = new Material(workbenchMaterial);
                       mat.name = "AloftModFramework_DefaultPBR_Interactive_Material";
                       mat.hideFlags = HideFlags.HideAndDontSave;
                       mat.SetTexture("_TextureAlbedo", mainTex);
                       mat.SetTexture("_TextureNormals", normalTex);
                       mat.SetTexture("_TextureMask", detailMask);
                       mat.SetVector("_ColorSelect", new Vector4(1.6f, 1.3f, 0.5f, 1));
                       mat.SetColor("_Color", new Color(1f, 1f, 1f, 1f));
                       mesh.material = mat;
                       for (int i = 0; i < mesh.materials.Length; i++)
                       {
                           // woulda been nice to go from shader, but IDK what's going on there... Lets you dodge the bullet on loading an object...
                           // var mats = new Material(Shader.Find("Amplify/V2/DefaultPBR_Interactive"));
                           var mat2 = new Material(workbenchMaterial);
                           mat2.name = "AloftModLoaderMaterial";
                           mat2.hideFlags = HideFlags.HideAndDontSave;
                           mat2.SetTexture("_TextureAlbedo", mainTex);
                           mat2.SetTexture("_TextureNormals", normalTex);
                           mat2.SetTexture("_TextureMask", detailMask);
                           mat2.SetVector("_ColorSelect", new Vector4(1.6f, 1.3f, 0.5f, 1));
                           mat2.SetColor("_Color", new Color(1f, 1f, 1f, 1f));
                           mesh.materials[i] = mat2;
                       }
                   });
               })
               .ToList();

           Localizations = allAssets
               .Where(x => x is AloftModFrameworkLocalization)
               .Cast<AloftModFrameworkLocalization>()
               .ToList();

           Meals = allAssets
               .Where(x => x is AloftModFrameworkCookingMeal)
               .Cast<AloftModFrameworkCookingMeal>()
               .Select(x =>
               {
                   var meal = ScriptableObject.CreateInstance<SCookMeal>();

                   Logger.LogInfo("Item id was: " + x.GetItemId());
                   meal.ItemID = x.GetItemId();
                   meal.StatDuration = x.StatDuration;
                   meal.StatHpMaxAmount = x.StatHpMaxAmount;
                   meal.TriggerConditions = x.TriggerConditions;
                   meal.Edible = x.Edible;
                   meal.CookPreventBurning = x.CookPreventBurning;
                   meal.IngredientType = x.IngredientType;

                   return meal;
               })
               .ToList();

           CookingRecipes = allAssets
               .Where(x => x is AloftModFrameworkCookingRecipe)
               .Cast<AloftModFrameworkCookingRecipe>()
               .Select(x =>
               {
                   SCookRecipe recipe;
                   if (x.IsPerfectRecipe)
                   {
                       recipe = ScriptableObject.CreateInstance<SCookRecipePerfect>();
                       Logger.LogInfo("Recipe ingredients were: " + string.Join(",", x.InputItemIds.Cast<ItemID.ID>().ToArray()));
                       (recipe as SCookRecipePerfect).Inputs = x.InputItemIds.Cast<ItemID.ID>().ToArray();
                   } else
                   {
                       recipe = ScriptableObject.CreateInstance<SCookRecipeBastard>();
                       (recipe as SCookRecipeBastard).RecipeVariations = x.Variations;
                   }
                   Logger.LogInfo("Item id was: " + x.GetItemId());
                   recipe.Output = x.GetItemId();
                   // TODO: looks like "is perfect" maybe ties to SCookRecipePerfect vs SCookRecipeBastard
                   //  probably relates to "exactly this ingredient" or "any of these ingredients"
                   //  i.e. blueberry vs fruit
                   recipe.IsPerfectRecipe = x.IsPerfectRecipe;
                   recipe.Category = x.Category;
                   recipe.UnlockableByRecipePage = x.UnlockableByRecipePage;

                   return recipe;
               })
               .ToList();

           LoggerRef = Logger;

           Harmony harmony = new Harmony(GUID);
           /// Hooks for adding new items
           var alternativeItemLoadPoint = AccessTools.Method(typeof(ScriptableInventoryManager), nameof(ScriptableInventoryManager.GetItem), new[] { typeof(ItemID.ID) });
           var customItemLoadHook = AccessTools.Method(typeof(ItemPatches), nameof(ItemPatches.RewriteItemIdResult));
           harmony.Patch(alternativeItemLoadPoint, null, new HarmonyMethod(customItemLoadHook));

           /// Hooks for adding new crafting recipes
           var recipeLoadPoint = AccessTools.Method(typeof(SRecipeManager), nameof(SRecipeManager.GetRecipes));
           var recipeLoadHook = AccessTools.Method(typeof(RecipePatches), nameof(RecipePatches.RewriteRecipeResult));
           harmony.Patch(recipeLoadPoint, null, new HarmonyMethod(recipeLoadHook));


           /// Hooks related to extending localization
           var localizationPoint = AccessTools.Method(typeof(Localization), nameof(Localization.SetLanguage));
           var localizationHook = AccessTools.Method(typeof(LocalizationPatches), nameof(LocalizationPatches.SetLanguage));
           harmony.Patch(localizationPoint, null, new HarmonyMethod(localizationHook));
           var localizationPoint2 = AccessTools.Method(typeof(Localization), nameof(Localization.GetLocalizedValue));
           var localizationHook2 = AccessTools.Method(typeof(LocalizationPatches), nameof(LocalizationPatches.GetLocalizedValue));
           harmony.Patch(localizationPoint2, null, new HarmonyMethod(localizationHook2));



           /// Hooks related to building new structures
           var craftingPoint = AccessTools.Method(typeof(ScriptableCraftingManager), nameof(ScriptableCraftingManager._GetCrafting));
           var craftingHook = AccessTools.Method(typeof(BuildingHooks), nameof(BuildingHooks.GetCrafting));
           harmony.Patch(craftingPoint, null, new HarmonyMethod(craftingHook));
           var craftingPoint2 = AccessTools.Method(typeof(PopulationManager), nameof(PopulationManager.GetPopulationData));
           var craftingHook2 = AccessTools.Method(typeof(BuildingHooks), nameof(BuildingHooks.GetPopulationData));
           harmony.Patch(craftingPoint2, null, new HarmonyMethod(craftingHook2));
           var craftingPoint3 = AccessTools.Method(typeof(ScriptablePopulationDataManager), nameof(ScriptablePopulationDataManager.GetPopulation));
           var craftingHook3 = AccessTools.Method(typeof(BuildingHooks), nameof(BuildingHooks.GetPopulationData2));
           harmony.Patch(craftingPoint3, null, new HarmonyMethod(craftingHook3));
           var craftingPoint4 = AccessTools.Method(typeof(ScriptablePopulationDataManager), nameof(ScriptablePopulationDataManager.InitList));
           var craftingHook4 = AccessTools.Method(typeof(BuildingHooks), nameof(BuildingHooks.InitListExtension));
           harmony.Patch(craftingPoint4, null, new HarmonyMethod(craftingHook4));
           var prefabPoint = AccessTools.Method(typeof(ScriptablePopulationData), nameof(ScriptablePopulationData.GetPrefabGameObject));
           var prefabHook = AccessTools.Method(typeof(BuildingHooks), nameof(BuildingHooks.GetPrefabGameObject));
           harmony.Patch(prefabPoint, null, new HarmonyMethod(prefabHook));
           var buildingTabsHookPoint = AccessTools.Method(typeof(UI_BuildingMenu), nameof(UI_BuildingMenu.Tabs_Initialize));
           var buildingTabsHook = AccessTools.Method(typeof(BuildingHooks), nameof(BuildingHooks.AddMoreTabs));
           harmony.Patch(buildingTabsHookPoint, new HarmonyMethod(buildingTabsHook));


           /// World object spawning hooks
           var popBalancingPoint = AccessTools.Method(typeof(SPopBalancing), nameof(SPopBalancing.GetPopulationDataKits));
           var popBalancingHook = AccessTools.Method(typeof(WorldGenHooks), nameof(WorldGenHooks.GetPopulationGroupDataKits));
           harmony.Patch(popBalancingPoint, null, new HarmonyMethod(popBalancingHook));
           var balancingManagerResources = AccessTools.Method(typeof(SBalancingManager), nameof(SBalancingManager.GetResourceBalancing));
           var balancingManagerResourcesHook = AccessTools.Method(typeof(WorldGenHooks), nameof(WorldGenHooks.GetResourceBalancing));
           harmony.Patch(balancingManagerResources, null, new HarmonyMethod(balancingManagerResourcesHook));


           // Cooking hooks
           var cookingMealPoint = AccessTools.Method(typeof(SCookingManager), nameof(SCookingManager.GetMeal));
           var cookingMealPointHook = AccessTools.Method(typeof(CookingHooks), nameof(CookingHooks.GetMeal));
           harmony.Patch(cookingMealPoint, null, new HarmonyMethod(cookingMealPointHook));
           var cookingMealRecipePoint = AccessTools.Method(typeof(SCookingManager), nameof(SCookingManager.GetMealRecipe));
           var cookingMealRecipeHook = AccessTools.Method(typeof(CookingHooks), nameof(CookingHooks.GetMealRecipe));
           harmony.Patch(cookingMealRecipePoint, null, new HarmonyMethod(cookingMealRecipeHook));
           var cookingMealFromRecipePoint = AccessTools.Method(typeof(SCookingManager), nameof(SCookingManager.GetMealFromRecipe));
           var cookingMealFromRecipeHook = AccessTools.Method(typeof(CookingHooks), nameof(CookingHooks.GetMealFromRecipe));
           harmony.Patch(cookingMealFromRecipePoint, null, new HarmonyMethod(cookingMealFromRecipeHook));

           // Crafting screen hooks
           var cookingInventoryItemStacksPoint = AccessTools.Method(typeof(CanvasCooking), nameof(CanvasCooking.GetInventoryItemStacks));
           var cookingInventoryItemStacksHook = AccessTools.Method(typeof(CookingHooks), nameof(CookingHooks.GetInventoryItemStacks));
           harmony.Patch(cookingInventoryItemStacksPoint, null, new HarmonyMethod(cookingInventoryItemStacksHook));
           var cookingInventoryRecipeItemStacksPoint = AccessTools.Method(typeof(CanvasCooking), nameof(CanvasCooking.GetRecipeItemStacks));
           var cookingInventoryRecipeItemStacksHook = AccessTools.Method(typeof(CookingHooks), nameof(CookingHooks.GetRecipeItemStacks));
           harmony.Patch(cookingInventoryRecipeItemStacksPoint, null, new HarmonyMethod(cookingInventoryRecipeItemStacksHook));

           // Crafting recipe scroll hooks
           var randomHiddenMealPoint = AccessTools.Method(typeof(SConsumableRecipePage), nameof(SConsumableRecipePage.GetRandomHiddenMeal));
           var randomHiddenMealHook = AccessTools.Method(typeof(CookingHooks), nameof(CookingHooks.GetRandomHiddenMeal));
           harmony.Patch(randomHiddenMealPoint, null, new HarmonyMethod(randomHiddenMealHook));
       }
    }
}