using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AloftModFramework.Crafting
{
    
    [CreateAssetMenu(fileName = "CraftingRecipe", menuName = "Aloft Mod Framework/Crafting Recipe")]
    public class CraftingRecipe : ScriptableObject
    {
        public Items.ItemReference[] inputItems;
        public Items.ItemReference outputItem;
        public int quantity;

        public bool attachToStation = false;
        public CraftingStationReference craftingStation;
    }
}
