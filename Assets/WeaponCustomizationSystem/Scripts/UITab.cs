using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WeaponCustomizationSystem
{
    /*
     * The base class in which all Tab types (classes) inherited from
     */
    public class UITab : MonoBehaviour
    {

        public virtual void Awake()
        {

        }

        public virtual void OnEnable()
        {

        }

        public virtual void OnDisable()
        {

        }

        public virtual void Start()
        {

        }

        public virtual void OnTabClickedEvent() //Unity Event function
        {

        }

        //.................................Tab States.......................................
        public virtual void OpenTab()
        {
            if(UITabsManager.UITabsManagerInstance == null)
            {
                UITabsManager uiTabManager = FindObjectOfType<UITabsManager>();
                uiTabManager.AddUITabToList(this);
                return;
            }
            UITabsManager.UITabsManagerInstance.AddUITabToList(this);
        }

        public virtual void CloseTab()
        {
            UITabsManager.UITabsManagerInstance.RemoveUITabFromList(this);
        }
    }
}
