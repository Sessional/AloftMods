using System;
using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects.Cooking;
using UnityEngine;

namespace AloftModFramework
{
    [Serializable]
    public class IngredientTypeReference
    {
        public SCookRecipe.IngredientType vanillaType;
        public int id;

        public int GetIngredientTypeAsInt()
        {
            if (vanillaType != SCookRecipe.IngredientType.None) return (int)vanillaType;
            return id;
        }

        public SCookRecipe.IngredientType GetIngredientType()
        {
            return (SCookRecipe.IngredientType)GetIngredientTypeAsInt();
        }
    }
}
