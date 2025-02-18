using System.Collections;
using System.Collections.Generic;
using AloftModFramework.Crafting;
using Level_Manager;
using Scriptable_Objects;
using Terrain.Platforms.Population.Construction;
using UI;
using UI.Inventory.V2;
using UI.Permanent;
using UnityEngine;

namespace NewWorkbench
{
    public class ConstructionNewWorkbenchStation : ConstructionAbstract
    {
        public CraftingStation craftingStation;
        public int numberCraftingSlots;
    
        public void Click_Interact()
        {
            if (!UI_WarningMessage.IslandIsCompatible(base.Platform, true, false))
            {
                return;
            }

            Level.CraftingManager.WorkbenchManager.OpenStation(numberCraftingSlots, (SRecipeManager.CraftingStation)craftingStation.stationId);
            Level.CanvasManager.OpenTab(CanvasManager.CanvasTab.PlayerMenu);
            Level.CanvasManager.CanvasPlayerMenu.Tabs.OpenTab(SPlayerMenuTab.TabIDEnum.Crafting);
        }
    }
}
