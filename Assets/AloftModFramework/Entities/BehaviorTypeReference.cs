using System;
using System.Collections;
using System.Collections.Generic;
using Terrain.Platforms.Population.Population_Soul;
using UnityEngine;

namespace AloftModFramework.Entities
{
    [Serializable]
    public class BehaviorTypeReference
    {
        public PopulationSoul.BehaviourTypeEnum vanillaBehaviorType;
        public PopulationSoul.BehaviourTypeEnum GetBehaviorType()
        {
            return vanillaBehaviorType;
        }
    }
}
