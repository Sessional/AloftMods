using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects;
using UnityEngine;

namespace AloftModFramework
{
    [CreateAssetMenu(fileName = "Building Category", menuName = "AloftModFramework/Building Category")]
    public class AloftModFrameworkBuildingCategory : ScriptableObject
    {
        public int BuildingCategoryId;
        public string Name;
        public Sprite DisplayIcon;
        public Sprite SecondaryIcon;
        public bool UseAParentCategory = false;
        public ScriptableBuildingTab.BuildingCategory ParentCategory;
    }
}
