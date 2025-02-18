using System;
using System.Collections;
using System.Collections.Generic;
using Creator.Creator_IO;
using UnityEngine;

namespace AloftModFramework
{
    [Serializable]
    public class BiomeReference
    {
        public SCreatorFileAbstract.CreatorTagBiomeID vanillaBiome;

        public int GetBiomeAsInt()
        {
            return (int)vanillaBiome;
        }

        public SCreatorFileAbstract.CreatorTagBiomeID GetBiome()
        {
            return (SCreatorFileAbstract.CreatorTagBiomeID)GetBiomeAsInt();
        }
    }
}
