using System.Collections;
using System.Collections.Generic;
using AloftModFramework.Items;
using Scriptable_Objects.Combat;
using UnityEngine;

namespace AloftModFramework.Equipment
{
    [CreateAssetMenu(fileName = "Equippable", menuName = "Aloft Mod Framework/Equippable")]
    public class Equippable : ScriptableObject
    {
        public int id;

        public ItemReference item;
        public GameObject heldItemPrefab;

        public Player.Hands.Hands.HandBehaviour handBehaviour;
        public SCombatValues combatValues;
        public SBowValues bowValues;

        public EquippableTemplateReference equippableTemplate;
    }
}
