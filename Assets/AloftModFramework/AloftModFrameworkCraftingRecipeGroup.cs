using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AloftModFramework
{
    [CreateAssetMenu(fileName = "RecipeGroup", menuName = "AloftModFramework/Crafting Recipe Group")]
    public class AloftModFrameworkCraftingRecipeGroup : ScriptableObject
    {
        public int StationId;
        public string StationName;
        public AloftModFrameworkCraftingRecipe[] Recipes;
    }
}
