using System;
using System.Collections;
using System.Collections.Generic;
using Inventory;
using Terrain.Platforms.Population.Construction.Storage;
using UnityEngine;
using Utilities;

namespace ManureBox
{
    public class ManureBoxStorage : StorageVisual_Abstract
    {
        public GameObject emptyObj;
        public GameObject partialFillObj;
        public GameObject fullObj;

        public override void DisplayContainer(ItemContainerV2 container)
        {
            base.DisplayContainer(container);
            
            var isEmpty = container.GetQuantity(ItemID.ID.Manure) == 0;
            var isFull = !container.CanAdd(ItemID.ID.Manure, 1);
            var isPartialFill = !isEmpty && !isFull;
            this.emptyObj.SetActive(isEmpty);
            this.fullObj.SetActive(isFull);
            this.partialFillObj.SetActive(isPartialFill);
        }
    }
}
