using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponCustomizationSystem
{
    public class AttachmentSlot : MonoBehaviour
    {
        [Header("Attachment Slot Data")]
        [SerializeField] private AttachmentItem.AttachmentType attachmentType = AttachmentItem.AttachmentType.Sight;//default = sight
        [SerializeField] private List<AttachmentItem> incompatibleAttachmentsList = new List<AttachmentItem>();

        [Header("Attachment Slot UI Data")]
        [SerializeField] private AttachmentSelectionUI attachmentSelectionUIPrefab;
        [SerializeField] 
        [Tooltip("The position in the transform in which the attachment UI button will be spawned. If null, this obj's transform position will be used instead!")] 
        private Transform attachmentUIButtonSpawn;

        public List<AttachmentItem> attachmentsForThisSlot { get; private set; } = new List<AttachmentItem>();

        private List<GameObject> attachmentItemObjects = new List<GameObject>();
        public AttachmentItem currentlyEquippedAttachment { get; private set; }
        private GameObject currentlyEquippedAttachmentObj;

        private void Start()
        {
            //These functions below must be call in this exact order or else there will be BUGS!!!
            GetAttachmentsForThisSlot();
            GenerateAndDisableAcceptedAttachmentsOnStartup();
            GenerateAttachmentSelectionUIObjectFromPrefab();
        }

        public void EquipAttachment(AttachmentItem attachment)
        {
            if(attachment != currentlyEquippedAttachment) currentlyEquippedAttachment = attachment;

            if (attachment.itemPrefab == null) return;
            if (attachmentItemObjects == null || attachmentItemObjects.Count == 0) return;

            for (int i = 0; i < attachmentsForThisSlot.Count; i++)
            {
                if(attachment == attachmentsForThisSlot[i])
                {
                    if(attachmentItemObjects[i] != null) attachmentItemObjects[i].SetActive(true);
                    currentlyEquippedAttachmentObj = attachmentItemObjects[i];
                }
            }
        }

        public void UnEquipAttachment()
        {
            //in case an empty attachment is unequipped (which means that previously, the player has not equipped anything)
            if (currentlyEquippedAttachmentObj == null) return;

            currentlyEquippedAttachmentObj.SetActive(false);
        }

        private void GetAttachmentsForThisSlot()
        {
            if (CustomizationInventory.CustomizationInventoryInstance.attachmentList == null || CustomizationInventory.CustomizationInventoryInstance.attachmentList.Count == 0)
            {
                Debug.LogError("Customization Inventory Attachment list is not set!");
                return;
            }

            List<AttachmentItem> attachments = CustomizationInventory.CustomizationInventoryInstance.attachmentList;

            //refines and generates the appropriate attachment list that will be spawned under this attachment slot

            for (int i = 0; i < attachments.Count; i++)
            {
                //get the attachment of type empty from the Inventory so that the player can "unequip" attachments by "equipping" the empty one
                //if currently equipped attachment of type Attachment Item is null on startup-> this Empty will be it
                if (attachments[i].attachmentType == AttachmentItem.AttachmentType.Empty)
                {
                    if (currentlyEquippedAttachment == null) currentlyEquippedAttachment = attachments[i];
                    attachmentsForThisSlot.Add(attachments[i]);
                    continue;
                }

                //if attachment type not match -> remove from list
                if (attachments[i].attachmentType != attachmentType)
                {
                    continue;
                }

                if (incompatibleAttachmentsList != null && incompatibleAttachmentsList.Count > 0)
                {
                    bool compatible = true;
                    //if this attachment slot requires that some attachments are incompatible with it, check for incompatibility
                    for (int j = 0; j < incompatibleAttachmentsList.Count; j++)
                    {
                        if (attachments[i] == incompatibleAttachmentsList[j])
                        {
                            compatible = false;
                            break;
                        }
                    }

                    if (compatible) attachmentsForThisSlot.Add(attachments[i]);
                }
                else
                {
                    attachmentsForThisSlot.Add(attachments[i]);
                }
            }
        }

        //This function generates the attachment game objects from the attachment items (scriptable objects) list on startup
        //Then it disables the attachment game objects
        private void GenerateAndDisableAcceptedAttachmentsOnStartup()
        {
            //spawn attachments and disable them on start
            for(int i = 0; i < attachmentsForThisSlot.Count; i++)
            {
                if (attachmentsForThisSlot[i] == null) continue;

                if (attachmentsForThisSlot[i].itemPrefab == null)
                {
                    attachmentItemObjects.Add(null);
                    continue;
                }

                GameObject obj = Instantiate(attachmentsForThisSlot[i].itemPrefab, transform.position, Quaternion.identity, transform);
                attachmentItemObjects.Add(obj);
                obj.SetActive(false);
            }
        }

        private void GenerateAttachmentSelectionUIObjectFromPrefab()
        {
            if (attachmentSelectionUIPrefab == null) return;

            Transform spawn;
            if (attachmentUIButtonSpawn != null) spawn = attachmentUIButtonSpawn;
            else spawn = transform;

            GameObject obj = Instantiate(attachmentSelectionUIPrefab.gameObject, spawn.position, Quaternion.LookRotation(Camera.main.transform.forward), spawn);

            AttachmentSelectionUI attachmentSelectionUI = obj.GetComponent<AttachmentSelectionUI>();
            if (attachmentSelectionUI == null)
            {
                Debug.LogError("Could not find Attachment Selection UI component of :" + obj.name + " upon spawning. Disabling attachment slot!");
                gameObject.SetActive(false);
                return;
            }

            attachmentSelectionUI.InitializeAttachmentSelectionUISubsystem(this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.015f);
            Gizmos.color = Color.green;
            if(attachmentUIButtonSpawn != null) Gizmos.DrawWireSphere(attachmentUIButtonSpawn.position, 0.015f);
        }
    }
}
