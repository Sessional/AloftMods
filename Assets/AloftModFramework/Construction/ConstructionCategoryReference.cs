using System;
using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects;
using UnityEngine;

namespace AloftModFramework.Construction
{
    [Serializable]
    public class ConstructionCategoryReference
    {
        public ConstructionCategory category;
        public ScriptableBuildingTab.BuildingCategory vanillaCategory;
        public int id = -1;

        public int GetCategoryAsInt()
        {
            if (category != null) return category.id;
            if (id != -1) return id;
            return (int)vanillaCategory;
        }
        
        public ScriptableBuildingTab.BuildingCategory GetCategory()
        {
            return (ScriptableBuildingTab.BuildingCategory)GetCategoryAsInt();
        }
    }
}
