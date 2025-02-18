using System;
using System.Collections;
using System.Collections.Generic;
using Crafting.MultiStep_Construction;
using UnityEngine;

namespace AloftModFramework.Entities
{
    [Serializable]
    public class MultistepBehaviorReference
    {
        public MultiStepConstructionManager.MultiStepBehaviour vanillaBehavior;
        
        public MultiStepConstructionManager.MultiStepBehaviour GetMultistepBehavior()
        {
            return vanillaBehavior;
        }
    }
}
