using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects.Cooking;
using UnityEngine;
using Utilities;

namespace AloftModFramework
{
    [CreateAssetMenu(fileName = "Cooking Recipe", menuName = "AloftModFramework/Cookable Recipe")]
    public class AloftModFrameworkCookingRecipe : ScriptableObject
    {
        public ItemID.ID OutputVanillaItem;
        public int OutputItemId;
        public AloftModFrameworkItem OutputModItem;

        public bool IsPerfectRecipe;
        public SCookRecipe.CookingCategory Category;
        public bool UnlockableByRecipePage;

        public SCookRecipeBastard.RecipeVariationsStruct[] Variations;
        public int[] InputItemIds;

        public int GetItemIdAsInt()
        {
            if (OutputVanillaItem != ItemID.ID.Empty) return (int)OutputVanillaItem;
            if (OutputModItem != null) return OutputModItem.ItemId;
            else return OutputItemId;
        }

        public ItemID.ID GetItemId()
        {
            return (ItemID.ID)GetItemIdAsInt();
        }
    }
}
