using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;

namespace WeaponCustomizationSystem
{
    public class TabCamTransition : MonoBehaviour
    {
        [SerializeField] private UnityEvent OnCamTransitionStarted;
        [SerializeField] private UnityEvent OnCamTransitionEnded;
        private CinemachineBrain cinemachineBrain;

        // Start is called before the first frame update
        private void Awake()
        {
            cinemachineBrain = GetComponent<CinemachineBrain>();
            if (cinemachineBrain == null)
            {
                Debug.LogWarning("Cinemachine Brain not found on: " + name + " Disabling component...");
                enabled = false;
            }
        }

        public void WeaponTypeTabChangedCamTransition()
        {
            StartCoroutine(CamTransitionCoroutine());
        }

        private IEnumerator CamTransitionCoroutine()
        {
            OnCamTransitionStarted?.Invoke();
            yield return new WaitForSeconds(cinemachineBrain.m_DefaultBlend.BlendTime);
            OnCamTransitionEnded?.Invoke();
        }
    }
}
