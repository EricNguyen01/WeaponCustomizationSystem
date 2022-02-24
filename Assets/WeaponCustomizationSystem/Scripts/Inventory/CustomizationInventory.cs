using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponCustomizationSystem
{
    /*
     * Add or remove weapons/attachments scriptable objects to the corresponding lists of this class to customize them
     * The system will pull data from the assigned scriptable objects and use them in the customization functions in other classes
     */
    public class CustomizationInventory : MonoBehaviour
    {
        public static CustomizationInventory CustomizationInventoryInstance;
        [field: SerializeField] public List<WeaponItem> weaponList { get; private set; } = new List<WeaponItem>();
        [field: SerializeField] public List<AttachmentItem> attachmentList { get; private set; } = new List<AttachmentItem>();

        private void Awake()
        {
            if(CustomizationInventoryInstance == null)
            {
                CustomizationInventoryInstance = this;
                if (weaponList == null || weaponList.Count == 0) Debug.LogWarning("Weapon list of Customization Inventory Is Null Or Empty!");
                if (attachmentList == null || attachmentList.Count == 0) Debug.LogWarning("Attachment list of Customization Inventory Is Null Or Empty!");
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
    }
}
