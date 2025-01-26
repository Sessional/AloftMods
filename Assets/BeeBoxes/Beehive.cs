using Terrain.Platforms.Population.Nature;
using Utilities;
using Scriptable_Objects;
using Interactions.Interactibles.Interaction_LIsteners;

public class Beehive : PopNatureBeeHive
{
    private float LastHarvest = 0;
    private bool IsHarvestable => TimeRef.WorldTime - LastHarvest >= HarvestCooldownMinutes * 60f;

    public InteractibleMeta InteractibleScript;

    public new void Event_TryHarvest()
    {
        if (IsHarvestable)
        {
            ScriptableCrafting.CraftingCostClass[] onHarvestItems = OnHarvestItems;
            foreach (ScriptableCrafting.CraftingCostClass craftingCostClass in onHarvestItems)
            {
                Crafting.Inventory.Inventory.Pickup(craftingCostClass.ItemID, craftingCostClass.Qty);
            }
            LastHarvest = TimeRef.WorldTime;
            RefreshCustomValues();
        }
    }

    protected override void RefreshCustomValues()
    {
        if (FullOfHoneyObj != null)
        {
            FullOfHoneyObj.SetActive(IsHarvestable);            
        }

        if (InteractibleScript != null)
        {
            InteractibleScript.ToggleInteractible(IsHarvestable);            
        }
    }
}
