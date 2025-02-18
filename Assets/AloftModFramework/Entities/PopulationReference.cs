using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace AloftModFramework.Entities
{
    [Serializable]
    public class PopulationReference
    {
        public Population population;
        public PopulationID.ID vanillaPopulation;
        public int id;

        public int GetPopulationIdAsInt()
        {
            if (population != null) return population.id;
            if (vanillaPopulation != PopulationID.ID.Empty) return (int) vanillaPopulation;
            return id;
        }

        public PopulationID.ID GetPopulationId()
        {
            return (PopulationID.ID)GetPopulationIdAsInt();
        }
    }
}
