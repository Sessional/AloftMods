using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace AloftModFramework.Items
{
    [Serializable]
    public class ItemCategoryReference
    {
        public ItemCategory itemCategory;
        public ItemID.ItemCatergory vanillaCategory;
        public int id;
        
        public int GetCategoryAsInt()
        {
            if (itemCategory != null) return itemCategory.id;
            if (vanillaCategory != ItemID.ItemCatergory.None) return (int)vanillaCategory;
            return id;
        }

        public ItemID.ItemCatergory GetCategory()
        {
            return (ItemID.ItemCatergory) GetCategoryAsInt();
        }
    }
}
