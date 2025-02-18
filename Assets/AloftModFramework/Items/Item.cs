using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AloftModFramework.Items
{
    [CreateAssetMenu(fileName = "Item", menuName = "Aloft Mod Framework/Item")]
    public class Item : ScriptableObject
    {
        public int id;
        public string itemName;
        public string description;
        public Sprite sprite;
        
        public ItemCategoryReference category;
        public ItemTypeReference type;
        public ItemWeightReference weight;
        public EquippableReference equipType;
        public PickupAudioTagReference pickupAudioTag;

        public ItemTagReference[] itemTags;
    }
}
