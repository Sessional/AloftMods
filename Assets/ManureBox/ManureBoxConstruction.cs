using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Network;
using Terrain.Platforms.Population.Construction;
using Terrain.Platforms.Population.Construction.Storage;
using UnityEngine;
using Utilities;

namespace ManureBox
{
    public class ManureBoxConstruction : ConstructionStorage
    {
        private float _checkTimer = 0;
        
        public void Update()
        {
            this._checkTimer -= TimeRef.DT;

            if (this._checkTimer >= 0) return;

            if (this.Platform is null) return;
            if (this.Platform.GlobalData is null) return;

            this._checkTimer = 5f;
            
            var manureSouls = this.Platform.GlobalData.GetPopulationSoulWithID(PopulationID.ID.DroppingManure);
            foreach (var soul in manureSouls)
            {
                if (this.Container.CanAdd(ItemID.ID.Manure, 1)) this.Container.AddAndGetRemaining(ItemID.ID.Manure, 1);
                
                if (soul.Gameobject is not null) GameObject.Destroy(soul.Gameobject);
                if (soul.PopulationObject is not null) GameObject.Destroy(soul.PopulationObject.gameObject);
                soul.RemoveSoul();
            }
            this.StorageVisualRef.DisplayContainer(this.Container);
        }
    }
}
