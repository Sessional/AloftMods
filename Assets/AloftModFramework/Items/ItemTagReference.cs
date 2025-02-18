using System;
using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects;
using UnityEngine;

namespace AloftModFramework.Items
{
    [Serializable]
    public class ItemTagReference
    {
        public ItemTag itemTag;
        public ScriptableInventoryItem.ItemTagID vanillaTag;
        public int id = -1;
        
        public int GetTagAsInt()
        {
            if (itemTag != null) return itemTag.id;
            if (id != -1) return id;
            return (int)vanillaTag;
        }

        public ScriptableInventoryItem.ItemTagID GetTag()
        {
            return (ScriptableInventoryItem.ItemTagID) GetTagAsInt();
        }
    }
}
