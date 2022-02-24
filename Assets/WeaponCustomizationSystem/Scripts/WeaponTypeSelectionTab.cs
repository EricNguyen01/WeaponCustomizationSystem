using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WeaponCustomizationSystem
{
    /*
     * This class represents the weapon type selection UI tab and carries all the tab clicking, opening, closing, and contents generation logics
     * Tab functions, however, will be handled by UITabsManager
     * When a weapon type UI tab is clicked, its corresponding weapon item buttons are displayed
     * For instance, when Assault Rifle tab is opened, AR buttons will be displayed while all other weapon types buttons will be disabled.
     * These weapon buttons can then be selected to display the gun on screen for customization
     */
    public class WeaponTypeSelectionTab : UITab
    {
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color defaultColor;

        [SerializeField] [Tooltip("The Content game object under Weapon Item Selection Panel's Viewport object.\nThis is where Weapon Item Buttons will be spawned and child under!")]
        private GameObject weaponItemSelectionContent;

        [SerializeField] [Tooltip("The weapon type of this tab. Default = Assault Rifle.")]
        private WeaponItem.WeaponType weaponType = WeaponItem.WeaponType.AssaultRifle;

        [SerializeField] [Tooltip("The weapon item UI slot prefab that will be spawned under the Content obj of the WeaponItemSelectionPanel")]
        private WeaponItemButton weaponItemButtonPrefab;

        //the weapon item buttons spawned by this weapon type selection tab
        private List<WeaponItemButton> weaponItemButtonsOfThisType = new List<WeaponItemButton>();
        private WeaponItemButton selectedWeaponButton;

        private Image weaponTypeSelectionUIImage;

        public override void Awake()
        {
            weaponTypeSelectionUIImage = GetComponent<Image>();
        }

        //This method is inherited from UITab class but DOES NOT call the base method in base class itself.
        //This method is called when the player click on the Weapon Type Selection Tab button in the Weapon Type Selection Panel UI
        public override void OnTabClickedEvent()
        {
            //UITabsManager handles the Tabs change
            UITabsManager.UITabsManagerInstance.ChangeWeaponTypeSelectionTab(this);
        }

        public override void OpenTab()
        {
            weaponTypeSelectionUIImage.color = selectedColor;

            //if a weapon type tab is opened but no weapon of this type exists -> tab is opened but no action is carried out and nothing is displayed!
            if (weaponItemButtonsOfThisType == null || weaponItemButtonsOfThisType.Count == 0) return;

            //if there are weapons of this type exist, display the corresponding selection buttons for such weapons
            for (int i = 0; i < weaponItemButtonsOfThisType.Count; i++)
            {
                weaponItemButtonsOfThisType[i].gameObject.SetActive(true);
            }

            //if first time opening tab (no previous weapon selected) -> get the weapon from the 1st weapon selection button
            if(selectedWeaponButton == null)
            {
                selectedWeaponButton = weaponItemButtonsOfThisType[0];
            }

            //display the weapon (weapon display is done in OnWeaponSelected() func in WeaponItemButton.cs)
            if (!isInInspectMode)
            {
                selectedWeaponButton.OnWeaponSelected();
                //weaponTypeSelectionUIImage.color = selectedColor;
            }
            else
            {
                //if in inspector mode, DO NOTHING to the current displayed weapon or color. ONLY disable this tab and its spawned buttons
                gameObject.SetActive(true);
            }

            base.OpenTab();
        }

        public override void CloseTab()
        {
            if (!isInInspectMode) weaponTypeSelectionUIImage.color = defaultColor;

            if (weaponItemButtonsOfThisType == null || weaponItemButtonsOfThisType.Count == 0) return;

            for (int i = 0; i < weaponItemButtonsOfThisType.Count; i++)
            {
                weaponItemButtonsOfThisType[i].gameObject.SetActive(false);
            }

            if (!isInInspectMode)
            {
                if (selectedWeaponButton != null) selectedWeaponButton.OnWeaponDeSelected();
            }
            else
            {
                //if in inspector mode, DO NOTHING to the current displayed weapon or color. ONLY disable this tab and its spawned buttons
                gameObject.SetActive(false);
            }

            base.CloseTab();
        }

        //This method is called when the player clicked on a weapon item button that is spawned by this weapon type tab
        public void NewWeaponButtonSelected(WeaponItemButton weaponItemButton)
        {
            if (weaponItemButton == selectedWeaponButton) return;//if the player clicked on the currently selected weapon -> do nothing
            
            //else
            selectedWeaponButton.OnWeaponDeSelected();
            selectedWeaponButton = weaponItemButton;
            selectedWeaponButton.OnWeaponSelected();
        }

        //This method is called by UITabsManager
        //If there's any weapon of this tab's weapon type exists, a button will be generated for it.
        //This button can then be later clicked on to display that weapon for customization
        public void GenerateWeaponButtonsForThisWeaponTypeTab()
        {
            if(CustomizationInventory.CustomizationInventoryInstance.weaponList == null || CustomizationInventory.CustomizationInventoryInstance.weaponList.Count == 0)
            {
                Debug.LogError("Customization Inventory Weapon list is not set!");
                return;
            }

            List<WeaponItem> weapons = CustomizationInventory.CustomizationInventoryInstance.weaponList;

            for (int i = 0; i < weapons.Count; i++)
            {
                if(weapons[i].weaponType == weaponType)
                {
                    GameObject obj = Instantiate(weaponItemButtonPrefab.gameObject, weaponItemSelectionContent.transform);
                    WeaponItemButton weaponItemButton = obj.GetComponent<WeaponItemButton>();

                    if (weaponItemButton == null)
                    {
                        Debug.LogError("Could not find WeaponItemButton component of :" + obj.name + " upon spawning!");
                        continue;
                    }

                    weaponItemButton.SetButtonData(weapons[i], this);
                    weaponItemButtonsOfThisType.Add(weaponItemButton);
                }
            }
        }
    }
}
