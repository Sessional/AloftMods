using System.Collections;
using System.Collections.Generic;
using AloftModFramework.Entities;
using UnityEngine;
using Utilities;

namespace AloftModFramework.Spawners
{
    [CreateAssetMenu(fileName = "PopulationSpawner", menuName = "Aloft Mod Framework/Population Spawner")]
    public class PopulationSpawner : ScriptableObject
    {
        public PopulationReference[] spawnedPopulations;
        public PopulationReference spawner;
        public MinMaxInt spawnAmount;
        public MinMax spreading;
        [Range(0.0f, 1f)]
        public float density;

        public BiomeReference biome;
    }
}
