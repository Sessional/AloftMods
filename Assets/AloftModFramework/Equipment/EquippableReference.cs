using System;
using System.Collections;
using System.Collections.Generic;
using Player.Player_Equip;
using UnityEngine;

namespace AloftModFramework.Equipment
{
    [Serializable]
    public class EquippableReference
    {
        public Equippable equippable;
        public PlayerEquip.Equipable vanillaEquippable;
        public int id;
        
        public int GetEquippableAsInt()
        {
            if (equippable != null) return equippable.id;
            if (vanillaEquippable != PlayerEquip.Equipable.None) return (int)vanillaEquippable;
            return id;
        }

        public PlayerEquip.Equipable GetEquippable()
        {
            return (PlayerEquip.Equipable) GetEquippableAsInt();
        }
    }
}
