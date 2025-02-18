using System.Collections;
using System.Collections.Generic;
using AloftModFramework.Items;
using UnityEngine;

namespace AloftModFramework.Cooking
{
    
    [CreateAssetMenu(fileName = "Meal", menuName = "Aloft Mod Framework/Cooking Meal")]
    public class Meal : ScriptableObject
    {
        public ItemReference item;
        public int statDuration;
        public int statHpMaxAmount;
        public ConditionTriggerReference[] triggerConditions;
        public bool edible;
        public bool preventBurning;
        public IngredientTypeReference ingredientType;
    }
}
