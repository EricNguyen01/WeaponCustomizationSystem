using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponCustomizationSystem
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "CustomizationAssets/Attachments/New Attachment")]
    public class AttachmentItem : Item
    {
        public ItemType itemType { get; private set; } = ItemType.Attachment;
        public enum AttachmentType { Empty, Sight, Barrel, Underbarrel, SideBarrel, Grip, Mag, Stock}
        [field: SerializeField] public AttachmentType attachmentType { get; private set; } = AttachmentType.Sight;
    }
}
