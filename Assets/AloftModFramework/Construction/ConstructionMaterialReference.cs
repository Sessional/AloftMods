using System;
using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects;
using UnityEngine;

namespace AloftModFramework.Construction
{
    [Serializable]
    public class ConstructionMaterialReference
    {
        public ConstructionMaterial material;
        public ScriptableCrafting.ConstructionMaterial vanillaMaterial;
        public int id = -1;

        public int GetConstructionMaterialAsInt()
        {
            if (material != null) return material.id;
            if (id != -1) return id;
            return (int) vanillaMaterial;
        }
        
        public ScriptableCrafting.ConstructionMaterial GetConstructionMaterial()
        {
            return (ScriptableCrafting.ConstructionMaterial) GetConstructionMaterialAsInt();
        }
    }
}
