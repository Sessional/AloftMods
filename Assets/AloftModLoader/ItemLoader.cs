using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using HarmonyLib;
using Scriptable_Objects;
using UnityEngine;
using Utilities;
using Logger = UnityEngine.Logger;

namespace AloftModLoader
{
    public class ItemLoader
    {
        private readonly Harmony _harmony;
        private readonly List<ScriptableInventoryItem> _items;
        private readonly ManualLogSource _logger;

        public ItemLoader(ManualLogSource logger, Harmony harmony, List<Object> assets)
        {
            this._logger = logger;
            this._harmony = harmony;
            this._items = assets
                .FilterAndCast<AloftModFramework.Items.Item>()
                .Select(x =>
                {
                    var item = ScriptableObject.CreateInstance<ScriptableInventoryItem>();
                    
                    logger.LogDebug("Loaded " + x.itemName + " at item id " + x.id);
                    
                    item.ID = (ItemID.ID) x.id;
                    
                    item.DisplayName = x.itemName;
                    item.DisplayDescription = x.description;
                    item.DisplaySprite = x.sprite;
                    
                    item.Category = x.category.GetCategory();
                    item.Type = x.type.GetItemType();
                    item.Weight = x.weight.GetWeight();
                    item.EquipType = x.equipType.GetEquippable();
                    item.AudioPickupID = x.pickupAudioTag.GetAudioTag();
                    item.ItemTags = x.itemTags.Select(tag => tag.GetTag()).ToArray();

                    return item;
                })
                .ToList();
        }

        public void Patch()
        {
            _harmony.Patch(
                AccessTools.Method(typeof(ScriptableInventoryManager), nameof(ScriptableInventoryManager.GetItem), new[] { typeof(ItemID.ID) }),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(ItemLoader), nameof(ItemLoader.GetItem)))
                );
        }

        private static ScriptableInventoryItem GetItem(ScriptableInventoryItem __result, ItemID.ID id)
        {
            if (__result == null)
            {
                var result = Plugin.ItemLoader._items.FirstOrDefault(x => x.ID == id);
                Plugin.ItemLoader._logger.LogDebug("Rewrote item id for " + (int) id + " to be " + result);
                return result;
            }

            return __result;
        }
    }
}
