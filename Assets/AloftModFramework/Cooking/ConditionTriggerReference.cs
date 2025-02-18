using System;
using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects;
using UnityEngine;

namespace AloftModFramework.Cooking
{
    [Serializable]
    public class ConditionTriggerReference
    {
        public SCondition.ConditionTrigger vanillaConditionTrigger;
        public int id;

        public int GetConditionTriggerAsInt()
        {
            if (vanillaConditionTrigger != SCondition.ConditionTrigger.None) return (int)vanillaConditionTrigger;
            return id;
        }

        public SCondition.ConditionTrigger GetConditionTrigger()
        {
            return (SCondition.ConditionTrigger)GetConditionTriggerAsInt();
        }
    }
}
