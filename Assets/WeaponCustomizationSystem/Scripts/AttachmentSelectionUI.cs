using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace WeaponCustomizationSystem
{
    /*
     * This script represents the attachment selection subsystem where it holds reference to the attachment selection enable UI button which 
     * when clicked will display the attachment selection box where different attachments can be cycled through and equip/unequip
     * Attachment selection enable UI button, attachment cycles, equip, and unequip logics, 
     * and other logics of this subsystem are handled by this script.
     */
    public class AttachmentSelectionUI : UITab//, IPointerClickHandler, IPointerDownHandler
    {
        [Header("Attachment Selection UI Components")]
        [SerializeField] private Image attachmentSelectionBoxUIPanel;
        [SerializeField] private Image attachmentSelectionBoxUIImage;
        [SerializeField] private TextMeshProUGUI attachmentNameText;
        [SerializeField] private Image attachmentSelectionEnableButtonImage;
        
        [Header("Button States Colors")]
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color defaultColor;

        private Canvas attachmentSelectionUICanvas;
        private AttachmentSlot attachmentSlotOfThisAttachmntSelectionUI;
        private CanvasGroup canvasGroup;

        private int currentAttachmentCycle = 0;
        private bool attachmentSelectionBoxUIAlreadyEnabled = false;

        public static event System.Action<AttachmentSelectionUI> OnAttachmentUIButtonClicked;

        public override void Awake()
        {
            attachmentSelectionUICanvas = GetComponent<Canvas>();
            attachmentSelectionUICanvas.worldCamera = Camera.main;

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if(attachmentSelectionEnableButtonImage == null)
            {
                Debug.LogWarning("Attachment Selection Enable Button Image component is not assigned. Button will not change color on click!");
            }
            if(attachmentSelectionBoxUIImage == null)
            {
                Debug.LogError("Attachment Selection Box UI Image component is not found in: " + name + ". Make sure 1 is assigned for the attachment selection subsystem to work!");
            }
            if (attachmentSelectionBoxUIPanel == null)
            {
                Debug.LogError("Attachment Selection Box UI Object is not found in: " + name + ". Make sure 1 is assigned for the attachment selection subsystem to work!");
                return;
            }

            if (attachmentSelectionBoxUIPanel.gameObject.activeInHierarchy) attachmentSelectionBoxUIPanel.gameObject.SetActive(false);
        }

        public override void OnEnable()
        {
            OpenTab();//auto and always open tab whenever this obj is enabled

            WeaponTypeSelectionTab.OnWeaponSelectInTransition += TemporaryDisableInteraction;
            AttachmentSelectionUI.OnAttachmentUIButtonClicked += DisableAttachmentSelectionUIOnOtherButtonClicked;
        }

        public override void OnDisable()
        {
            CloseTab();//auto and always close tab whenver this obj is disabled

            WeaponTypeSelectionTab.OnWeaponSelectInTransition -= TemporaryDisableInteraction;
            AttachmentSelectionUI.OnAttachmentUIButtonClicked -= DisableAttachmentSelectionUIOnOtherButtonClicked;
        }

        //This func only calls once when an attachment slot instance spawns this attachment selection UI subsystem on startup
        public void InitializeAttachmentSelectionUISubsystem(AttachmentSlot slot)
        {
            //set the attachment slot instance that spawned this attachment selection UI subsystem
            attachmentSlotOfThisAttachmntSelectionUI = slot;

            if (attachmentSlotOfThisAttachmntSelectionUI.currentlyEquippedAttachment != null)
            {
                for (int i = 0; i < attachmentSlotOfThisAttachmntSelectionUI.attachmentsForThisSlot.Count; i++)
                {
                    if(attachmentSlotOfThisAttachmntSelectionUI.currentlyEquippedAttachment == attachmentSlotOfThisAttachmntSelectionUI.attachmentsForThisSlot[i])
                    {
                        currentAttachmentCycle = i;
                        break;
                    }
                }
            }
            else
            {
                if(attachmentSlotOfThisAttachmntSelectionUI.attachmentsForThisSlot.Count == 0)
                {
                    Debug.LogError("Attachments In Attachments List For :" + name + " Has no attachments.\n Make sure there is always 1 attachment of type Empty as a default attachment in inventory even when no other attachments are available!");
                    gameObject.SetActive(true);
                    return;
                }
                for(int i = 0; i < attachmentSlotOfThisAttachmntSelectionUI.attachmentsForThisSlot.Count; i++)
                {
                    
                    if(attachmentSlotOfThisAttachmntSelectionUI.attachmentsForThisSlot[i].attachmentType == AttachmentItem.AttachmentType.Empty)
                    {
                        currentAttachmentCycle = i;
                        break;
                    }
                }
            }

            DisplayAttachmentIconAndName();

            //perform equip
            attachmentSlotOfThisAttachmntSelectionUI.EquipAttachment(attachmentSlotOfThisAttachmntSelectionUI.attachmentsForThisSlot[currentAttachmentCycle]);
        }

        public override void OpenTab()
        {
            EnableAttachmentSelectionUI(false);

            //only run this statement when coming out of inspect mode (weapon parent was not disabled, only this was and needed to be reabled again!)
            //normally, this OpenTab() func is called once when the game obj becomes enabled (enable is done when the parent weapon obj is enabled)
            if (isInInspectMode) gameObject.SetActive(true);

            base.OpenTab();//registering this UITab to the active tabs list of UITabsManager if not already
        }

        public override void CloseTab()
        {
            if (isInInspectMode)
            {
                if(gameObject.activeInHierarchy) gameObject.SetActive(false);
                //EnableAttachmentSelectionUI(false);
            }

            base.CloseTab();
        }

        //UnityEvent
        //Clicking attachment selection box enable button will trigger this function -> attachment selection box will be displayed
        public void OnAttachmentSelectionBoxEnableButtonClicked()
        {
            //send an event to the other attachmentUI buttons -> if another is in used, close that one and open this one instead!
            //Closing of the other button's attachment selection UI is handled in the button after receiving the event broadcast from this button.
            OnAttachmentUIButtonClicked?.Invoke(this);

            //Enable the attachment selection UI obj with the attachment UI component attached
            if(!attachmentSelectionBoxUIAlreadyEnabled) EnableAttachmentSelectionUI(true);
            else EnableAttachmentSelectionUI(false);
        }

        public void OnNextAttachmentButtonClicked()
        {
            if (attachmentSlotOfThisAttachmntSelectionUI == null)
            {
                Debug.LogError("Attachment Slot Component of this Attachment Selection UI not found! Disabling object!");
                gameObject.SetActive(false);
                return;
            }

            attachmentSlotOfThisAttachmntSelectionUI.UnEquipAttachment();

            if(currentAttachmentCycle == attachmentSlotOfThisAttachmntSelectionUI.attachmentsForThisSlot.Count - 1)
            {
                currentAttachmentCycle = 0;
            }
            else
            {
                currentAttachmentCycle += 1;
            }

            DisplayAttachmentIconAndName();

            attachmentSlotOfThisAttachmntSelectionUI.EquipAttachment(attachmentSlotOfThisAttachmntSelectionUI.attachmentsForThisSlot[currentAttachmentCycle]);
        }

        public void OnPreviousAttachmentButtonClicked()
        {
            if (attachmentSlotOfThisAttachmntSelectionUI == null)
            {
                Debug.LogError("Attachment Slot Component of this Attachment Selection UI not found! Disabling object!");
                gameObject.SetActive(false);
                return;
            }

            attachmentSlotOfThisAttachmntSelectionUI.UnEquipAttachment();

            if (currentAttachmentCycle == 0)
            {
                currentAttachmentCycle = attachmentSlotOfThisAttachmntSelectionUI.attachmentsForThisSlot.Count - 1;
            }
            else
            {
                currentAttachmentCycle -= 1;
            }

            DisplayAttachmentIconAndName();

            attachmentSlotOfThisAttachmntSelectionUI.EquipAttachment(attachmentSlotOfThisAttachmntSelectionUI.attachmentsForThisSlot[currentAttachmentCycle]);
        }

        private void DisplayAttachmentIconAndName()
        {
            if(attachmentSlotOfThisAttachmntSelectionUI == null)
            {
                Debug.LogError("Attachment Slot Component of this Attachment Selection UI not found! Disabling object!");
                gameObject.SetActive(false);
                return;
            }

            if (attachmentSlotOfThisAttachmntSelectionUI.attachmentsForThisSlot[currentAttachmentCycle].itemIcon != null)
            {
                attachmentSelectionBoxUIImage.color = new Color(attachmentSelectionBoxUIImage.color.r, attachmentSelectionBoxUIImage.color.g, attachmentSelectionBoxUIImage.color.b, 1f);
                attachmentSelectionBoxUIImage.sprite = attachmentSlotOfThisAttachmntSelectionUI.attachmentsForThisSlot[currentAttachmentCycle].itemIcon;
            }
            else
            {
                attachmentSelectionBoxUIImage.color = new Color(attachmentSelectionBoxUIImage.color.r, attachmentSelectionBoxUIImage.color.g, attachmentSelectionBoxUIImage.color.b, 0f);
                attachmentSelectionBoxUIImage.sprite = null;
            }

            if (attachmentNameText != null)
            {
                attachmentNameText.text = attachmentSlotOfThisAttachmntSelectionUI.attachmentsForThisSlot[currentAttachmentCycle].itemName;
            }
        }

        private void EnableAttachmentSelectionUI(bool enabled)
        {
            if (attachmentSelectionBoxUIPanel == null) return;

            if (enabled)
            {
                if(!attachmentSelectionBoxUIPanel.gameObject.activeInHierarchy) attachmentSelectionBoxUIPanel.gameObject.SetActive(true);

                if(attachmentSelectionEnableButtonImage != null) attachmentSelectionEnableButtonImage.color = selectedColor;

                attachmentSelectionBoxUIAlreadyEnabled = true;

                return;
            }

            if (attachmentSelectionBoxUIPanel.gameObject.activeInHierarchy)
            {
                attachmentSelectionBoxUIPanel.gameObject.SetActive(false);

                if (attachmentSelectionEnableButtonImage != null) attachmentSelectionEnableButtonImage.color = defaultColor;

                attachmentSelectionBoxUIAlreadyEnabled = false;
            }
        }

        //C# event: triggered when another attachment UI Button that is not this one is pressed
        //if triggered -> set this button's attachment selection UI obj to inactive 
        //the other button that was clicked will be displaying its attachment selection UI instead!
        public void DisableAttachmentSelectionUIOnOtherButtonClicked(AttachmentSelectionUI button)
        {
            if (button == this) return;
            EnableAttachmentSelectionUI(false);
        }

        public void TemporaryDisableInteraction(bool disabled)
        {
            EnableAttachmentSelectionUI(false);
            if (canvasGroup != null)
            {
                if (disabled) canvasGroup.interactable = false;
                else canvasGroup.interactable = true;
            }
        }
    }
}
