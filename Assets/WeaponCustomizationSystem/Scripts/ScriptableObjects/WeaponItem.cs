using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponCustomizationSystem
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "CustomizationAssets/Weapons/New Weapon")]
    public class WeaponItem : Item
    {
        public ItemType itemType { get; private set; } = ItemType.Weapon;

        public enum WeaponType { AssaultRifle, SMG, LMG, SniperRifle, Handgun}
        [field: SerializeField] public WeaponType weaponType { get; private set; } = WeaponType.AssaultRifle;

    }
}
