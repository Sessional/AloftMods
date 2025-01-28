using System.Collections;
using System.Collections.Generic;
using Level_Manager;
using Scriptable_Objects;
using Terrain.Platforms.Population.Construction;
using UI;
using UI.Inventory.V2;
using UI.Permanent;
using UnityEngine;

public class ConstructionCustomCraftingStation : ConstructionAbstract
{
    public int craftingStation;
    
    public void Click_Interact()
    {
        if (!UI_WarningMessage.IslandIsCompatible(base.Platform, true, false))
        {
            return;
        }

        Level.CraftingManager.WorkbenchManager.OpenStation(5, (SRecipeManager.CraftingStation)craftingStation);
        Level.CanvasManager.OpenTab(CanvasManager.CanvasTab.PlayerMenu);
        Level.CanvasManager.CanvasPlayerMenu.Tabs.OpenTab(SPlayerMenuTab.TabIDEnum.Crafting);
    }
}
