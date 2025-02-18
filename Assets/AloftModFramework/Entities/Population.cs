using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects;
using UnityEngine;

namespace AloftModFramework.Entities
{
    
    [CreateAssetMenu(fileName = "Population", menuName = "Aloft Mod Framework/Population")]
    public class Population : ScriptableObject
    {
        public int id;
        public GameObject prefab;

        public BehaviorTypeReference behaviorType;
        public MultistepBehaviorReference multistepBehavior;

        public ScriptablePopulationData.SpawnDistance loadDistance = ScriptablePopulationData.SpawnDistance.Medium;
        public DataTagReference[] dataTags;

        public bool learnedFromSketching;
        public Construction.ConstructionBlueprint learnedBlueprint;
    }
}
