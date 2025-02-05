using Audio;
using Player.Player_Equip;
using Scriptable_Objects;
using UnityEngine;
using Utilities;

namespace AloftModFramework
{
    [CreateAssetMenu(fileName = "Item", menuName = "AloftModFramework/Item")]
    public class AloftModFrameworkItem : ScriptableObject
    {
        public int ItemId;
        public string Name;
        public string Description;
        public Sprite DisplaySprite;

        public ItemID.ItemCatergory Category;
        public ItemID.ItemType Type;
        public ItemID.ItemWeight Weight;
        public PlayerEquip.Equipable EquipType;
        public Audio_Pickup.PickUpAudioTagID AudioPickupID;
        public ScriptableInventoryItem.ItemTagID[] ItemTags;
    }
}
