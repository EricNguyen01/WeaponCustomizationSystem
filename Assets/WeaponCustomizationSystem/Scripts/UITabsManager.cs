using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponCustomizationSystem
{
    /*
     * Handles all the switching, opening, and closing of different tabs within the weapon customization system
     */
    public class UITabsManager : MonoBehaviour
    {
        public static UITabsManager UITabsManagerInstance;

        [field: SerializeField]
        [field: Tooltip("The UI Tab to open by default upon start")]
        public WeaponTypeSelectionTab defaultWeaponTypeSelectionTab { get; private set; }

        private List<WeaponTypeSelectionTab> weaponTypeSelectionTabList = new List<WeaponTypeSelectionTab>();
        private List<UITab> activeUITabsList = new List<UITab>();
        private List<IHideableUI> hideableUIsList = new List<IHideableUI>();

        private WeaponTypeSelectionTab currentWeaponTypeSelectionTab;//keep track of the current opening tab
        private WeaponTypeSelectionTab previousWeaponTypeSelectionTab;//keep track of the previously opened tab
        public bool isInInspectMode { get; set; } = false;

        private void Awake()
        {
            if (UITabsManagerInstance == null)
            {
                UITabsManagerInstance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void OnEnable()
        {
            HideableUI.OnHideableUIActive += AddOrRemoveHideableUIsToList;
        }

        private void OnDisable()
        {
            HideableUI.OnHideableUIActive -= AddOrRemoveHideableUIsToList;
        }

        private void Start()
        {
            SetWeaponTypeSelectionTabsContents();
            SetAndOpenDefaultWeaponTypeSelectionTabWhileClosingOthers();
        }

        public void ChangeWeaponTypeSelectionTab(WeaponTypeSelectionTab tabToChangeTo)
        {
            currentWeaponTypeSelectionTab.CloseTab();
            previousWeaponTypeSelectionTab = currentWeaponTypeSelectionTab;
            currentWeaponTypeSelectionTab = tabToChangeTo;
            currentWeaponTypeSelectionTab.OpenTab();
        }

        public void EnableInspectMode(bool enableStatus)
        {
            if (enableStatus)
            {
                isInInspectMode = true;
                HideAllUITabsAndUIElements(enableStatus);
                return;
            }

            isInInspectMode = false;
            HideAllUITabsAndUIElements(enableStatus);
        }

        public void AddUITabToList(UITab tab)
        {
            if (!activeUITabsList.Contains(tab)) activeUITabsList.Add(tab);
        }

        public void AddOrRemoveHideableUIsToList(IHideableUI hideableUI, bool add)
        {
            if (add)
            {
                if (!hideableUIsList.Contains(hideableUI)) hideableUIsList.Add(hideableUI);
                return;
            }
    
            if (hideableUIsList.Contains(hideableUI)) hideableUIsList.Remove(hideableUI);
        }

        public void RemoveUITabFromList(UITab tab)
        {
            if (activeUITabsList.Contains(tab)) activeUITabsList.Remove(tab);
        }

        private void HideAllUITabsAndUIElements(bool hide)
        {
            if (activeUITabsList == null || activeUITabsList.Count == 0) return;

            if (hide)
            {
                for(int i = 0; i < activeUITabsList.Count; i++)
                {
                    //set inspect mode status first before closing the UITabs so that if system is in inspect mode,
                    //only the UI will close and not the weapon/attachment
                    activeUITabsList[i].SetInspectStatus(isInInspectMode);

                    activeUITabsList[i].CloseTab();
                }

                for(int i = 0; i < hideableUIsList.Count; i++)
                {
                    hideableUIsList[i].Hide();
                }

                return;
            }

            for (int i = 0; i < activeUITabsList.Count; i++)
            {
                activeUITabsList[i].OpenTab();

                //set the inspect mode later than OpenTab() so that the weapon is not enabled again
                activeUITabsList[i].SetInspectStatus(isInInspectMode);
            }

            for (int i = 0; i < hideableUIsList.Count; i++)
            {
                hideableUIsList[i].UnHide();
            }
        }

        //This method finds all the weapon type selection tabs in the scene and populates the tabs with their respective contents
        private void SetWeaponTypeSelectionTabsContents()
        {
            foreach(WeaponTypeSelectionTab tab in FindObjectsOfType<WeaponTypeSelectionTab>())
            {
                //Generates weapon buttons for any existing weapon of this type
                //If none exists and no placeholder for empty weapon is assigned -> generate nothing under this weapon type tab
                tab.GenerateWeaponButtonsForThisWeaponTypeTab();
                if (!weaponTypeSelectionTabList.Contains(tab)) weaponTypeSelectionTabList.Add(tab);
            }
        }

        //This method of opening/closing default tab must always be called in Start
        //because Awake is when the individual Tabs are setting up their data/buttons and should not be meddled with
        private void SetAndOpenDefaultWeaponTypeSelectionTabWhileClosingOthers()
        {
            if (defaultWeaponTypeSelectionTab == null)
            {
                if (weaponTypeSelectionTabList == null || weaponTypeSelectionTabList.Count == 0)
                {
                    Debug.LogError("No available Weapon Type Selection Tab found!");
                    return;
                }
            }

            for (int i = 0; i < weaponTypeSelectionTabList.Count; i++)
            {
                weaponTypeSelectionTabList[i].CloseTab();
            }

            if (defaultWeaponTypeSelectionTab != null)
            {
                defaultWeaponTypeSelectionTab.OpenTab();
                currentWeaponTypeSelectionTab = defaultWeaponTypeSelectionTab;
            }
            else if (weaponTypeSelectionTabList != null && weaponTypeSelectionTabList.Count > 0)
            {
                currentWeaponTypeSelectionTab = weaponTypeSelectionTabList[0];
                currentWeaponTypeSelectionTab.OpenTab();
            }
        }
    }
}
