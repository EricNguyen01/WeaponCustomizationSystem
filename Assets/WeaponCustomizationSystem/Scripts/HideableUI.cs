using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WeaponCustomizationSystem
{
    public class HideableUI : MonoBehaviour
    {
        [SerializeField] private bool animTransitionInsteadOfHide = false;
        [SerializeField] private bool setInvisibleOnHide = false;
        [SerializeField] private AnimationClip hideAnimation;
        [SerializeField] private AnimationClip unHideAnimation;

        private Animation animationComponent;
        private CanvasGroup canvasGroup;
        public float hideDuration { get; private set; }
        public float unHideDuration { get; private set; }

        //public static event System.Action<IHideableUI, bool> OnHideableUIActive;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            animationComponent = GetComponent<Animation>();

            if(animationComponent == null || hideAnimation == null || unHideAnimation == null)
            {
                if (animTransitionInsteadOfHide)
                {
                    Debug.LogWarning("Animation clip components of HideableUI: " + name + " not found. UI hide/unhide anim transition is disabled!");
                    animTransitionInsteadOfHide = false;
                }
            }

            if (animTransitionInsteadOfHide)
            {
                if (setInvisibleOnHide)
                {
                    Debug.LogWarning("Can't have both anim transition and invisible settings for HideableUI: " + name + " disabling invisible on hide!");
                    setInvisibleOnHide = false;
                }
                hideDuration = hideAnimation.length;
                unHideDuration = unHideAnimation.length;
            }
        }

        private void OnEnable()
        {
            //OnHideableUIActive?.Invoke(GetComponent<IHideableUI>(), true);
            WeaponTypeSelectionTab.OnWeaponSelectInTransition += TemporaryDisableInteraction;
        }

        private void OnDisable()
        {
            //OnHideableUIActive?.Invoke(GetComponent<IHideableUI>(), false);
            WeaponTypeSelectionTab.OnWeaponSelectInTransition -= TemporaryDisableInteraction;
        }

        public void Hide()
        {
            TemporaryDisableInteraction(true);
            if (setInvisibleOnHide) canvasGroup.alpha = 0f;
            if (animTransitionInsteadOfHide)
            {
                animationComponent.clip = hideAnimation;
                Debug.Log("Play Hide Anim!");
                animationComponent.Play();
            }
        }

        public void UnHide()
        {
            if (animTransitionInsteadOfHide)
            {
                animationComponent.clip = unHideAnimation;
                Debug.Log("Play UnHide Anim!");
                animationComponent.Play();
                StartCoroutine(ReEnableInteractionAfterAnim());
                return;
            }

            TemporaryDisableInteraction(false);
            if (setInvisibleOnHide) canvasGroup.alpha = 1f;
        }

        public void TemporaryDisableInteraction(bool disabled)
        {
            if(canvasGroup != null)
            {
                if (disabled) canvasGroup.interactable = false;
                else canvasGroup.interactable = true;
            }
        }

        private IEnumerator ReEnableInteractionAfterAnim()
        {
            yield return new WaitUntil(() => !animationComponent.isPlaying);
            TemporaryDisableInteraction(false);
        }
    }
}
