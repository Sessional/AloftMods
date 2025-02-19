using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects.Combat;
using UnityEngine;

namespace AloftModFramework.Equipment
{
    public class EquippableTemplate : ScriptableObject
    {
        public ScriptableCombatAnimationOverride overrideAnimations;
        public SCombatAnimationThirdPerson thirdPersonAnimations;
    }
}
