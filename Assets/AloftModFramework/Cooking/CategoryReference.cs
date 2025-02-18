using System;
using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects.Cooking;
using UnityEngine;

namespace AloftModFramework.Cooking
{
    [Serializable]
    public class CategoryReference
    {
        public SCookRecipe.CookingCategory vanillaCategory;
        public int id;

        public int GetCategoryAsInt()
        {
            if (vanillaCategory != SCookRecipe.CookingCategory.None) return (int) vanillaCategory;
            return id;
        }

        public SCookRecipe.CookingCategory GetCategory()
        {
            return (SCookRecipe.CookingCategory)GetCategoryAsInt();
        }
    }
}
