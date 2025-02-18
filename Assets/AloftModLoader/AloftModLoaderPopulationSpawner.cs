using System.Collections;
using System.Collections.Generic;
using Balance;
using Creator.Creator_IO;
using UnityEngine;
using Utilities;

namespace AloftModLoader
{
    public class AloftModLoaderPopulationSpawner : SPopBalancing.PopChance
    {
        public SCreatorFileAbstract.CreatorTagBiomeID biome;
        public PopulationID.ID spawner;
    }
}
