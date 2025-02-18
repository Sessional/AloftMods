using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AloftModFramework.Localization;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace AloftModLoader
{
    public class LocalizationLoader
    {
        private readonly ManualLogSource _logger;
        private readonly Harmony _harmony;
        
        private readonly List<LocalizationResource> _localizationAssets;

        private readonly Dictionary<string, string> _localizationValues = new Dictionary<string, string>();
        
        public LocalizationLoader(ManualLogSource logger, Harmony harmony, List<Object> allAssets)
        {
            this._logger = logger;
            this._harmony = harmony;

            this._localizationAssets = allAssets
                .FilterAndCast<LocalizationResource>()
                .ToList();
            
            logger.LogDebug("Found " + _localizationAssets.Count + " total localization assets.");
        }

        public void Patch()
        {
            this._harmony.Patch(
                AccessTools.Method(typeof(Utilities.Localization), nameof(Utilities.Localization.SetLanguage)),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(LocalizationLoader), nameof(LocalizationLoader.SetLanguage)))
            );
            this._harmony.Patch(
                AccessTools.Method(typeof(Utilities.Localization), nameof(Utilities.Localization.GetLocalizedValue)),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(LocalizationLoader), nameof(LocalizationLoader.GetLocalizedValue)))
            );
        }
        
        public static void SetLanguage(int index)
        {
            Plugin.LocalizationLoader._localizationValues.Clear();
            var localizationName = Utilities.Localization.GetLanguageName(index);
            
            var relevantLocalizations = Plugin.LocalizationLoader._localizationAssets
                .Select(asset => {
                    Plugin.LocalizationLoader._logger.LogDebug("Total localizations in this asset: " + asset.localizations.Length);
                    Plugin.LocalizationLoader._logger.LogDebug("Languages in this asset: " + string.Join(",", asset.localizations.Select(x => x.language).ToList()));
                    return asset.localizations.FirstOrDefault(localization => localization.language == localizationName);
                })
                .Where(x => x != null)
                .ToList();
            Plugin.LocalizationLoader._logger.LogDebug("Found " + relevantLocalizations.Count +
                                                       " localization resources.");

            foreach (var localization in relevantLocalizations)
            {
                foreach (var entry in localization.localizations.text.Split("\n"))
                {
                    if (string.IsNullOrEmpty(entry)) continue;

                    var splitEntry = entry.Split("\t");
                    if (splitEntry.Length != 2) Plugin.LocalizationLoader._logger.LogWarning("Localization line is invalid. " + entry);
                    else Plugin.LocalizationLoader._localizationValues.Add(splitEntry[0], splitEntry[1]);
                }
            }
        }
        
        public static string GetLocalizedValue(string __result, string key)
        {
            if (string.IsNullOrEmpty(__result))
            {
                if (Plugin.LocalizationLoader._localizationValues.TryGetValue(key, out var value))
                {
                    return value;
                }
            }
            return __result;
        }
    }
}
