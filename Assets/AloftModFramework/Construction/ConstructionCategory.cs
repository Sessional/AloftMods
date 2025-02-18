using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AloftModFramework.Construction
{
    [CreateAssetMenu(fileName = "ConstructionCategory", menuName = "Aloft Mod Framework/Construction Category")]
    public class ConstructionCategory : ScriptableObject
    {
        public int id;
        public string categoryName;
        public Sprite displayIcon;
        public Sprite secondaryIcon;

        public bool useParentCategory;
        public ConstructionCategoryReference parentCategory;
    }
}
