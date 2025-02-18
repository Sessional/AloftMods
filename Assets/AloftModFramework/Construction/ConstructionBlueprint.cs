using System.Collections;
using System.Collections.Generic;
using AloftModFramework.Entities;
using AloftModFramework.Items;
using Scriptable_Objects;
using UnityEngine;

namespace AloftModFramework.Construction
{
    [CreateAssetMenu(fileName = "Construction Blueprint", menuName = "Aloft Mod Framework/Construction Blueprint")]
    public class ConstructionBlueprint : ScriptableObject
    {
        public string displayName;
        public string description;
        public Sprite sprite;

        public PopulationReference populationData;
        
        public bool hideInBuildMenu;

        public ConstructionCategoryReference category;

        public float defaultScale = 1f;

        public ItemReferenceWithQuantity[] craftingCost;
        public ItemReferenceWithQuantity[] hammerCost;

        public PopulationReference isVariantOf;
        public PopulationReference[] variants;
        
        public PopulationReference[] unlockedPopulations;

        public ConstructionMaterialReference audioType;
    }
}
