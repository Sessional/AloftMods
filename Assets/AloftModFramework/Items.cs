using UnityEngine;
using Audio;
using Crafting.MultiStep_Construction;
using Creator.Creator_IO;
using Player.Player_Equip;
using Scriptable_Objects;
using Scriptable_Objects.Cooking;
using Terrain.Platforms.Population.Population_Soul;
using Utilities;
using static Scriptable_Objects.ScriptableInventoryItem;

namespace AloftModFramework
{

    // TODO: ideally item references would look something like this, but Unity doesn't appear to serialize these
    //  when loading from an asset bundle... It's there in the file but comes out null when you load the asset.. :(
    //  wonder if there's any tweaks we can do to make it actually load?
    //[Serializable]
    //public class AloftModFrameworkItemReference
    //{
    //    public int ItemId;
    //    public ItemID.ID VanillaItem;
    //    public AloftModFrameworkItem ModItem;

    //    public AloftModFrameworkItemReference() { }
    //    public AloftModFrameworkItemReference(int itemId, ItemID.ID vanillaItem, AloftModFrameworkItem modItem)
    //    {
    //        ItemId = itemId;
    //        VanillaItem = vanillaItem;
    //        ModItem = modItem;
    //    }

    //    public int GetItemIdAsInt()
    //    {
    //        if (VanillaItem != ItemID.ID.Empty) return (int)VanillaItem;
    //        if (ModItem != null) return ModItem.ItemId;
    //        else return ItemId;
    //    }

    //    public ItemID.ID GetItemId()
    //    {
    //        return (ItemID.ID)GetItemIdAsInt();
    //    }
    //}
}
