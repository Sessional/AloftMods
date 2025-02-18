using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace AloftModFramework.Items
{
    [Serializable]
    public class ItemWeightReference
    {
        public ItemWeight itemWeight;
        public ItemID.ItemWeight vanillaWeight;
        public int id = -1;
        public int GetWeightAsInt()
        {
            if (itemWeight != null) return itemWeight.id;
            if (id != -1) return id;
            return (int)vanillaWeight;
        }

        public ItemID.ItemWeight  GetWeight()
        {
            return (ItemID.ItemWeight ) GetWeightAsInt();
        }
    }
}
