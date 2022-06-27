using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace WeaponCustomizationSystem
{
    /*
     * This script, as its name suggested, represents the inspect button that when clicked, will enable the system to enter the inspect mode
     * During inspect mode, all UIs element will be hidden and uninteractable
     * The script will also check for whether the system can enter inspect mode at the moment before entering it.
     * If already entered inspect mode, send an event to the current weapon that is being customized to inspect it (check WeaponInspect.cs)
     */
    public class WeaponInspectButton : MonoBehaviour
    {
        [SerializeField] private bool canInspect = true;
        private bool isInspecting = false;

        private Image inspectButtonImageComponent;
        private TextMeshProUGUI inspectButtonTextMeshPro;
        private Text inspectButtonText;

        [SerializeField] private UnityEvent OnInspect;//unity event to play UI Hide animation (check HideableUI.cs)
        [SerializeField] private UnityEvent OnInspectExit;//unity event to play UI UnHide anim

        public static event System.Action<bool> OnWeaponInspected;//event sent to attachment selection UIs to temporary disable them

        private void Awake()
        {
            inspectButtonImageComponent = GetComponent<Image>();
            inspectButtonTextMeshPro = GetComponentInChildren<TextMeshProUGUI>();
            inspectButtonText = GetComponentInChildren<Text>();
        }

        private void OnEnable()
        {
            WeaponInspect.OnWeaponInspectInTransition += DisableWeaponInspection;
            WeaponTypeSelectionTab.OnWeaponSelectInTransition += DisableWeaponInspection;
        }

        private void OnDisable()
        {
            WeaponInspect.OnWeaponInspectInTransition -= DisableWeaponInspection;
            WeaponTypeSelectionTab.OnWeaponSelectInTransition -= DisableWeaponInspection;
        }

        //Unity Button UI Event Function - called when Inspect Button is clicked
        public void OnInspectButtonClicked()
        {
            if (!canInspect) return;

            if (!isInspecting)
            {
                isInspecting = true;
                OnInspect?.Invoke();//UI transitioning Unity event - move the UI elements out of the screen
                OnWeaponInspected?.Invoke(true);//weapon transitioning into inspect pos/rot C# event
                return;
            }

            OnWeaponInspected?.Invoke(false);//weapon transitioning out of inspect pos/rot C# event
            StartCoroutine(InspectExitDelay());//wait until weapon and cam finished transitioning back to their original fov/pos and rot values
        }

        private IEnumerator InspectExitDelay()
        {
            yield return new WaitUntil(() => canInspect);
            isInspecting = false;
            OnInspectExit?.Invoke();//UI transition Unity event - only move the UI after the weapon has finished transitioning out of inspect pos/rot
        }

        //UnityEvent to block weapon from being inspected
        //Triggers OnCamTransition and OnCamFinishedTransition UnityEvent in Camera object
        //Triggers on weapon change transition (check WeaponTypeSelectionTab.cs and WeaponItemButton.cs)
        public void DisableWeaponInspection(bool disableStatus)
        {
            //if disable weapon inspect received a true -> can inspect and UI display is set to false and vice versa 
            //basically the bools are inverted here
            canInspect = !disableStatus;

            if (inspectButtonImageComponent != null) inspectButtonImageComponent.enabled = !disableStatus;

            foreach (Image imageComp in GetComponentsInChildren<Image>())
            {
                imageComp.enabled = !disableStatus;
            }

            if (inspectButtonTextMeshPro != null)
            {
                inspectButtonTextMeshPro.enabled = !disableStatus;
            }
            else
            {
                if (inspectButtonText != null) inspectButtonText.enabled = !disableStatus;
            }
        }
    }
}
