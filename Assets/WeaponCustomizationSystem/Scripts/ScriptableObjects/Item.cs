using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WeaponCustomizationSystem
{
    [System.Serializable]
    public class Item : ScriptableObject
    {
        [field: SerializeField] public string itemName { get; private set; }
        [field: SerializeField] public Sprite itemIcon { get; private set; }
        [field: SerializeField] public GameObject itemPrefab { get; private set; }
        public enum ItemType { Weapon, Attachment}

        public virtual void Awake()
        {

        }
    }
}
