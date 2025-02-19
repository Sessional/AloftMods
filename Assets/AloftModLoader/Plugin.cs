using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Level_Manager;
using Scriptable_Objects.Combat;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AloftModLoader
{
    [BepInPlugin("AloftModLoader", "AloftModLoader", "1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static List<AssetBundle> AllBundles;
        public static List<Object> AllAssets;

        public static LocalizationLoader LocalizationLoader;
        public static ItemLoader ItemLoader;
        public static CraftingRecipeLoader CraftingRecipeLoader;
        public static ConstructionLoader ConstructionLoader;
        public static EntityLoader EntityLoader;
        public static CookingLoader CookingLoader;
        public static SpawnerLoader SpawnerLoader;
        public static EquipmentLoader EquipmentLoader;

        public static VanillaGameAssetLocator AssetLocator;
        
        public Plugin()
        {
            Logger.LogDebug("Info location: " + Info.Location);
            var modDirectory = Path.GetDirectoryName(Info.Location);
            string pluginDirectory;
            // Developing locally does not insert Author-Name paths into the staging
            // directory, but when installed via R2ModMan they do show up. If running
            // without `plugins` at the end, go up one extra level, otherwise stay put.
            if (modDirectory.EndsWith("plugins"))
            {
                pluginDirectory = Path.Combine(Path.GetDirectoryName(Info.Location));                
            }
            else
            {
                pluginDirectory = Path.Combine(Path.GetDirectoryName(Info.Location), "..");
            }
            var bundleNames = Directory.GetFiles(pluginDirectory, "*.amf.assetbundle", SearchOption.AllDirectories);
            Logger.LogDebug("Found bundles: " + string.Join(",", bundleNames));

            AllBundles = bundleNames.Select(x =>
            {
                Logger.LogDebug("Loading bundle " + x);
                return AssetBundle.LoadFromFile(x);
            }).ToList();

            AllAssets = AllBundles
                .SelectMany(x => x.LoadAllAssets())
                .ForEach(x =>
                    {
                        Logger.LogDebug("Loaded asset " + x.name);
                        x.hideFlags = HideFlags.HideAndDontSave;
                        DontDestroyOnLoad(x);
                    }
                ).ToList();

            //var assetsBundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, "sharedassets1.assets"));
            var resources = ScriptableObject.FindObjectsOfType<ScriptableEquipable>();
            Logger.LogError(resources.Length);

            
            Harmony harmony = new Harmony("AloftModLoader");

            AssetLocator = new VanillaGameAssetLocator(Logger, harmony);
            AssetLocator.Patch();
            
            Logger.LogDebug("Loading localizations");
            LocalizationLoader = new LocalizationLoader(Logger, harmony, AllAssets);
            LocalizationLoader.Patch();
            
            Logger.LogDebug("Loading items.");
            ItemLoader = new ItemLoader(Logger, harmony, AllAssets);
            ItemLoader.Patch();

            Logger.LogDebug("Loading entities.");
            EntityLoader = new EntityLoader(Logger, harmony, AllAssets);
            EntityLoader.Patch();

            Logger.LogDebug("Loading crafting recipes.");
            CraftingRecipeLoader = new CraftingRecipeLoader(Logger, harmony, AllAssets);
            CraftingRecipeLoader.Patch();

            Logger.LogDebug("Loading constructions.");
            ConstructionLoader = new ConstructionLoader(Logger, harmony, AllAssets);
            ConstructionLoader.Patch();
            
            Logger.LogDebug("Loading cooking assets.");
            CookingLoader = new CookingLoader(Logger, harmony, AllAssets);
            CookingLoader.Patch();

            Logger.LogDebug("Loading spawners.");
            SpawnerLoader = new SpawnerLoader(Logger, harmony, AllAssets);
            SpawnerLoader.Patch();

            EquipmentLoader = new EquipmentLoader(Logger, harmony, AllAssets);
            EquipmentLoader.Patch();

        }

        public void Awake()
        {
            Level.LoadSceneCallBack += LoadSceneCallBack;
            Logger.LogWarning("Awake!");
            //var equipables = Level.ConstantManager.ConstantManagers.InventoryManager.Equipables;
            //Logger.LogDebug("During start found " + equipables.Length + " scriptable equipables.");
        }

        private void LoadSceneCallBack()
        {
            var equipables = Level.ConstantManager.ConstantManagers.InventoryManager.Equipables;
            Logger.LogDebug("During load scene callback found " + equipables.Length + " scriptable equipables.");
        }

        public void Start()
        {
            Logger.LogWarning("Start!");
            var equipables = ScriptableObject.FindObjectsOfType<ScriptableEquipable>();
            Logger.LogDebug("Found " + equipables.Length + " equipables.");
        }
    }
}
