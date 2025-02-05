using System.Collections;
using System.Collections.Generic;
using Crafting.MultiStep_Construction;
using Scriptable_Objects;
using Terrain.Platforms.Population.Population_Soul;
using UnityEngine;

namespace AloftModFramework
{
    [CreateAssetMenu(fileName = "Building", menuName = "AloftModFramework/Building")]
    public class AloftModFrameworkBuildingData : ScriptableObject
    {
        public int PopulationId;
        public GameObject InstancePrefab;
        
        // TODO: replace with a different construct to permit custom behavior types
        public PopulationSoul.BehaviourTypeEnum BehaviourType;
        public MultiStepConstructionManager.MultiStepBehaviour MultiStepBehaviour;

        public ScriptablePopulationData.SpawnDistance LoadDistance = ScriptablePopulationData.SpawnDistance.Medium;
        public ScriptablePopulationData.PopDataTagID[] PopDataTags;

        public bool CanLearnViaSketchbook;
    }
}
