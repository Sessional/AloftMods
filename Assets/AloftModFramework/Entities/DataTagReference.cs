using System;
using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects;
using UnityEngine;

namespace AloftModFramework.Entities
{
    [Serializable]
    public class DataTagReference
    {
        public  ScriptablePopulationData.PopDataTagID vanillaTag;
        
        public ScriptablePopulationData.PopDataTagID GetTag()
        {
            return vanillaTag;
        }
    }
}
