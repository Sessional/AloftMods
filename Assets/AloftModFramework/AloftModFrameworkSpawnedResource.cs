using Creator.Creator_IO;
using UnityEngine;
using Utilities;

namespace AloftModFramework
{
    [CreateAssetMenu(fileName = "Spawnable Resource", menuName = "AloftModFramework/Spawnable Resource")]
    public class AloftModFrameworkSpawnedResource : ScriptableObject
    {
        public string Name;
        public int[] PopulationIds;
        public int SpawnAmountMin;
        public int SpawnAmountMax;
        public float SpreadingMin;
        public float SpreadingMax;
        public float Density;
        //todo: chance as "weight"

        public SCreatorFileAbstract.CreatorTagBiomeID Biome;
        public PopulationID.ID SpawnerId;
    }
}
