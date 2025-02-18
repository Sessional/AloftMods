using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace AloftModFramework.Items
{
    [Serializable]
    public class ItemTypeReference
    {
        public ItemType itemType;
        public ItemID.ItemType vanillaType;
        public int id = -1;
        
        public int GetItemTypeAsInt()
        {
            if (itemType != null) return itemType.id;
            if (id != -1) return id;
            return (int)vanillaType;
        }

        public ItemID.ItemType GetItemType()
        {
            return (ItemID.ItemType) GetItemTypeAsInt();
        }
    }
}
