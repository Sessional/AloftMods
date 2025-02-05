using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects;
using UnityEngine;
using Utilities;

namespace AloftModFramework
{
    [CreateAssetMenu(fileName = "Building Blueprint", menuName = "AloftModFramework/Building Blueprint")]
    public class AloftModFrameworkBuildingBlueprint : ScriptableObject
    {
        public bool HideInBuildMenu;
        public string DisplayName;
        public string DisplayDescription;
        public Sprite DisplaySprite;

        public ScriptableBuildingTab.BuildingCategory Category;
        public AloftModFrameworkBuildingCategory ModCategory;
        public int CategoryId;
        
        // This is really the "LOOK" of the construction thing.
        public AloftModFrameworkBuildingData BuildingData;

        // TOOD: these should become smarter to allow binding to EXISTING data -- (we deleted the PopulationID.ID enum reference because it only works for vanilla recipes then
        public AloftModFrameworkBuildingData IsVariantOf;
        public AloftModFrameworkBuildingData[] Variants;

        public float DefaultScale = 1f;

        // TODO: we should make it so these are smart enough to allow new resources
        public ScriptableCrafting.CraftingCostClass[] CraftingCost;
        public ScriptableCrafting.CraftingCostClass[] HammerCost;


        // TODO: we should make it so these are smart enough to allow new "pops"
        public PopulationID.ID[] PopToUnlockAsWell;


        public ScriptableCrafting.ConstructionMaterial AudioType;

        public ScriptableBuildingTab.BuildingCategory GetCategory()
        {
            if (ModCategory != null)
            {
                return (ScriptableBuildingTab.BuildingCategory) ModCategory.BuildingCategoryId;
            }
            
            if (CategoryId != 0)
            {
                return (ScriptableBuildingTab.BuildingCategory)CategoryId;
            }

            return Category;
        }
    }
}
