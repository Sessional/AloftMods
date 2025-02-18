using System.Collections;
using System.Collections.Generic;
using AloftModFramework.Construction;
using Scriptable_Objects;
using UnityEngine;

namespace AloftModLoader
{
    public class AloftModLoaderPopulationData : ScriptablePopulationData
    {
        public GameObject prefab;

        // Thee two fields are stashed as metadata in this custom PopData class
        // because entities (population) is loaded and configured before the
        // corresponding ScriptableCrafting is configured by the loader.
        // The loader visits these fields to set the proper fields during the
        // ScriptableCrafting creation process. See ConstructionLoader.
        public bool canBeLearnedFromSketching;
        public ConstructionBlueprint blueprintLearnedFromSketching;
    }
}
