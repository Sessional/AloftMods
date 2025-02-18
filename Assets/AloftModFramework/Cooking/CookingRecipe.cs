using System.Collections;
using System.Collections.Generic;
using AloftModFramework.Items;
using Scriptable_Objects.Cooking;
using UnityEngine;

namespace AloftModFramework.Cooking
{
    [CreateAssetMenu(fileName = "CookingRecipe", menuName = "Aloft Mod Framework/Cooking Recipe")]
    public class CookingRecipe : ScriptableObject
    {
        public bool isPerfectRecipe;
        public ItemReference[] inputs;
        public ItemReference output;
        public CategoryReference category;
        public bool unlockableByRecipePage;

        public SCookRecipeBastard.RecipeVariationsStruct[] variations;
    }
}
