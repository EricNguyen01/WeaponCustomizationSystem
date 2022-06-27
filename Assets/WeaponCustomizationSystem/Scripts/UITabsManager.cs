using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

        [SerializeField] private UnityEvent OnWeaponTypeTabChanged;

        private List<WeaponTypeSelectionTab> weaponTypeSelectionTabList = new List<WeaponTypeSelectionTab>();
        private List<UITab> activeUITabsList = new List<UITab>();

        private WeaponTypeSelectionTab currentWeaponTypeSelectionTab;//keep track of the current opening tab
        private WeaponTypeSelectionTab previousWeaponTypeSelectionTab;//keep track of the previously opened tab

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

        private void Start()
        {
            SetWeaponTypeSelectionTabsContents();
            SetAndOpenDefaultWeaponTypeSelectionTabWhileClosingOthers();
        }

        public void ChangeWeaponTypeSelectionTab(WeaponTypeSelectionTab tabToChangeTo)
        {
            //trigger UnityEvent - any event registered by pressing the "+" button will be executed after weapon tab changed
            OnWeaponTypeTabChanged?.Invoke();
            //process changing the weapon type tabs
            currentWeaponTypeSelectionTab.CloseTab();
            previousWeaponTypeSelectionTab = currentWeaponTypeSelectionTab;
            currentWeaponTypeSelectionTab = tabToChangeTo;
            currentWeaponTypeSelectionTab.OpenTab();
        }

        public void AddUITabToList(UITab tab)
        {
            if (!activeUITabsList.Contains(tab)) activeUITabsList.Add(tab);
        }

        public void RemoveUITabFromList(UITab tab)
        {
            if (activeUITabsList.Contains(tab)) activeUITabsList.Remove(tab);
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
