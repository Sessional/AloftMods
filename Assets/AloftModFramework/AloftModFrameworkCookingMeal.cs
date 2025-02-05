using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects;
using Scriptable_Objects.Cooking;
using UnityEngine;
using Utilities;

namespace AloftModFramework
{
    [CreateAssetMenu(fileName = "Meal", menuName = "AloftModFramework/Cookable Meal")]
    public class AloftModFrameworkCookingMeal : ScriptableObject
    {
        public ItemID.ID VanillaItem;
        public int ItemId;
        public AloftModFrameworkItem ModItem;

        public int StatDuration;
        public int StatHpMaxAmount;
        public SCondition.ConditionTrigger[] TriggerConditions;
        public bool Edible;
        public bool CookPreventBurning;
        public SCookRecipe.IngredientType IngredientType;

        public int GetItemIdAsInt()
        {
            if (VanillaItem != ItemID.ID.Empty) return (int)VanillaItem;
            if (ModItem != null) return ModItem.ItemId;
            else return ItemId;
        }

        public ItemID.ID GetItemId()
        {
            return (ItemID.ID)GetItemIdAsInt();
        }
    }
}
