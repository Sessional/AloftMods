using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Level_Manager;
using Terrain.Platforms.Population.Construction;
using UnityEngine;

namespace Teleporters
{
    public class ConstructionTeleporter : ConstructionAbstract
    {
        public GameObject exitPoint;

        public GameObject emitter;
        public GameObject arrivalEmitter;
        
        public void DoTeleport()
        {
            var furthestTeleporter = this.Platform.GlobalData.GetPopulationSoulWithID(this.SoulRef.PopulationID)
                .Where(x => x.Gameobject != this.gameObject)
                .OrderByDescending(x =>
                {
                    var destinationExit = x.Gameobject.GetComponent<ConstructionTeleporter>().exitPoint;
                    return Vector3.Distance(destinationExit.transform.position, this.exitPoint.transform.position);
                })
                .FirstOrDefault();

            if (furthestTeleporter is not null)
            {
                emitter.GetComponent<ParticleSystem>().Emit(100);
                var destinationExit = furthestTeleporter.Gameobject.GetComponent<ConstructionTeleporter>().exitPoint;
                Level.PlayerManager.Movement.TeleportToPosition(destinationExit.transform.position);
                furthestTeleporter.Gameobject.GetComponent<ConstructionTeleporter>().arrivalEmitter
                    .GetComponent<ParticleSystem>().Emit(100);
            }
        }
    }
}
