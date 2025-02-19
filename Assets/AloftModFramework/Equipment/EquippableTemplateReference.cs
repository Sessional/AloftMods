using System;
using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects.Combat;
using UnityEngine;

namespace AloftModFramework.Equipment
{
    public enum VanillaEquippableTemplate
    {
        None,
        Pickaxe
    }
    
    [Serializable]
    public class EquippableTemplateReference
    {
        public EquippableTemplate template;
        public VanillaEquippableTemplate vanillaTemplate;
    }
}
