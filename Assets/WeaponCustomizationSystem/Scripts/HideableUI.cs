using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WeaponCustomizationSystem
{
    public class HideableUI : MonoBehaviour, IHideableUI
    {
        [SerializeField] private List<Image> UIImagesToHide = new List<Image>();
        [SerializeField] private List<TextMeshProUGUI> UITextsToHide = new List<TextMeshProUGUI>();

        private CanvasGroup canvasGroup;

        public static event System.Action<IHideableUI, bool> OnHideableUIActive;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            OnHideableUIActive?.Invoke(this, true);
            WeaponTypeSelectionTab.OnWeaponSelectInTransition += TemporaryDisableInteraction;
        }

        private void OnDisable()
        {
            OnHideableUIActive?.Invoke(this, false);
            WeaponTypeSelectionTab.OnWeaponSelectInTransition -= TemporaryDisableInteraction;
        }

        private void Start()
        {
            if (UIImagesToHide == null || UIImagesToHide.Count == 0) Debug.LogWarning("HideableUI: " + name + " is in use but no Image components to hide is assigned!");
        }

        public void Hide()
        {
            if (UIImagesToHide != null && UIImagesToHide.Count > 0)
            {
                for(int i = 0; i < UIImagesToHide.Count; i++)
                {
                    if (!UIImagesToHide[i].gameObject.activeInHierarchy) continue;
                    UIImagesToHide[i].enabled = false;
                }
            }
            if (UITextsToHide != null && UITextsToHide.Count > 0)
            {
                for(int i = 0; i < UITextsToHide.Count; i++)
                {
                    if (!UITextsToHide[i].gameObject.activeInHierarchy) continue;
                    UITextsToHide[i].enabled = false;
                }
            }
        }

        public void UnHide()
        {
            if (UIImagesToHide != null && UIImagesToHide.Count > 0)
            {
                for (int i = 0; i < UIImagesToHide.Count; i++)
                {
                    if (UIImagesToHide[i].isActiveAndEnabled) continue;
                    UIImagesToHide[i].enabled = true;
                }
            }
            if (UITextsToHide != null && UITextsToHide.Count > 0)
            {
                for (int i = 0; i < UITextsToHide.Count; i++)
                {
                    if (UITextsToHide[i].isActiveAndEnabled) continue;
                    UITextsToHide[i].enabled = true;
                }
            }
        }

        public void TemporaryDisableInteraction(bool disabled)
        {
            if(canvasGroup != null)
            {
                if (disabled) canvasGroup.interactable = false;
                else canvasGroup.interactable = true;
            }
        }
    }
}
