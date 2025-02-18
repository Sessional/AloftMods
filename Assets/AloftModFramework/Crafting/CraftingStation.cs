using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AloftModFramework.Crafting
{
    [CreateAssetMenu(fileName = "CraftingStation", menuName = "Aloft Mod Framework/Crafting Station")]
    public class CraftingStation : ScriptableObject
    {
        public int stationId;
        public string stationName;
    }
}
