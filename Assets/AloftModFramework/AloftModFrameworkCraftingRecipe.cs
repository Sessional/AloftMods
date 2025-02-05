using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects;
using UnityEngine;
using Utilities;

namespace AloftModFramework
{
    [CreateAssetMenu(fileName = "Recipe", menuName = "AloftModFramework/Crafting Recipe")]
    public class AloftModFrameworkCraftingRecipe : ScriptableObject
    {
        public int[] InputItems;
        public ItemID.ID OutputVanillaItem;
        public int OutputItemId;
        public AloftModFrameworkItem OutputModItem;
        public int Quantity;

        public bool AttachToExistingStation;
        public SRecipeManager.CraftingStation Station;
    }
}
