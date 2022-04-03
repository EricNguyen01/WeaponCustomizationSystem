using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WeaponCustomizationSystem
{
    public class WeaponItemButton : MonoBehaviour
    {
        [Header("Color Settings")]
        [SerializeField] private Color32 selectedColor;
        [SerializeField] private Color32 defaultColor;

        private bool enableWeaponSelectTransition = false;
        private float transitionHorizontalOffset;

        private Transform weaponSpawnTransform;

        private WeaponTypeSelectionTab thisWeaponTypeTab;
        private WeaponItem weaponItemOfThisButton;
        private GameObject weaponObjectSpawned;

        private Image weaponButtonIcon;
        private TextMeshProUGUI weaponButtonText;
        private Image weaponImage;

        public static event System.Action<GameObject, bool> OnWeaponButtonSelectedAndWeaponObjectEnabled;

        private void Awake()
        {
            weaponButtonIcon = GetComponent<Image>();
            weaponImage = GetComponent<Image>();
            weaponButtonText = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void SetButtonData(WeaponItem weaponItem, WeaponTypeSelectionTab typeTab, Transform spawnTransform)
        {
            weaponItemOfThisButton = weaponItem;
            thisWeaponTypeTab = typeTab;
            weaponSpawnTransform = spawnTransform;

            weaponObjectSpawned = Instantiate(weaponItemOfThisButton.itemPrefab, weaponSpawnTransform.position, weaponSpawnTransform.rotation);
            weaponObjectSpawned.SetActive(false);

            if (weaponItem.itemIcon != null) weaponButtonIcon.sprite = weaponItem.itemIcon;
            weaponButtonText.text = weaponItem.itemName;
        }

        public void SetWeaponTransitionData(bool enableTransition, float horizontalOffset)
        {
            enableWeaponSelectTransition = enableTransition;
            transitionHorizontalOffset = horizontalOffset;
        }

        //This method is called when the player clicked a weapon button in the Weapon Item Selection Panel UI
        //Event is handled by UnityEvent in the UI Button Component attached to the same game obj that this script is on
        public void OnWeaponButtonClicked()
        {
            //Weapons change is handled by Weapon Type Selection Tab of the same type
            thisWeaponTypeTab.NewWeaponButtonSelected(this);
        }

        public void OnWeaponSelected()
        {
            weaponObjectSpawned.SetActive(true);
            weaponImage.color = selectedColor;

            if (enableWeaponSelectTransition)
            {
                Vector3 weaponSelectTransitionDestination = weaponObjectSpawned.transform.position;
                Vector3 weaponSelectTransitionStartOffset = weaponObjectSpawned.transform.position - weaponObjectSpawned.transform.right * transitionHorizontalOffset;
                thisWeaponTypeTab.WeaponTransitionProcess(weaponObjectSpawned, weaponSelectTransitionStartOffset, weaponSelectTransitionDestination, true);
            }

            //send an event to the color customization board that a new weapon is active and in customization
            //so that the color customization board can adjust
            OnWeaponButtonSelectedAndWeaponObjectEnabled?.Invoke(weaponObjectSpawned, true);
        }

        //This function is called when WeaponTypeSelectionTab.cs' CloseTab() is called 
        //When the weapon type tab of this weapon button is closed -> this button will be disabled (if is currently enabled and in used)
        //which will also disable its corresponding weapon object in scene
        public void OnWeaponDeSelected()
        {
            weaponImage.color = defaultColor;
            OnWeaponButtonSelectedAndWeaponObjectEnabled?.Invoke(weaponObjectSpawned, false);

            if (enableWeaponSelectTransition)
            {
                Vector3 weaponTransitionStart = weaponObjectSpawned.transform.position;
                Vector3 weaponTransitionEndOffset = weaponObjectSpawned.transform.position + weaponObjectSpawned.transform.right * transitionHorizontalOffset;
                thisWeaponTypeTab.WeaponTransitionProcess(weaponObjectSpawned, weaponTransitionStart, weaponTransitionEndOffset, false);
            }
            else weaponObjectSpawned.SetActive(false);
        }

        public void EnableWeaponSelectionTransition(bool enabled)
        {
            enableWeaponSelectTransition = enabled;
        }
    }
}
