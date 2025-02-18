using System;
using Utilities;

namespace AloftModFramework.Items
{
    [Serializable]
    public class ItemReferenceWithQuantity
    {
        public Item item;
        public int id;
        public ItemID.ID vanillaItem;

        public int quantity;

        public int GetItemIdAsInt()
        {
            if (item != null) return item.id;
            if (vanillaItem != ItemID.ID.Empty) return (int)vanillaItem;
            return id;
        }

        public ItemID.ID GetItemId()
        {
            return (ItemID.ID) GetItemIdAsInt();
        }
    }
}
