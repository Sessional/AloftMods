using System;
using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects;
using UnityEngine;

namespace AloftModFramework.Crafting
{
    [Serializable]
    public class CraftingStationReference
    {
        public CraftingStation station;
        public SRecipeManager.CraftingStation vanillaStation;
        public int id = -1;

        public int GetStationAsInt()
        {
            if (station != null) return station.stationId;
            if (id != -1) return id;
            return (int)vanillaStation;
        }

        public SRecipeManager.CraftingStation GetStation()
        {
            return (SRecipeManager.CraftingStation)GetStationAsInt();
        }
    }
}
