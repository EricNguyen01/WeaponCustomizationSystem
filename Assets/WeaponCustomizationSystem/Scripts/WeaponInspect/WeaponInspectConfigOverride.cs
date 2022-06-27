using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponCustomizationSystem
{
    /*
     * This script is to be placed on the camera being used to inspect the current customizable weapon
     * The script overrides the following values in WeaponInspect.cs (check WeaponInspect.cs)
     * TODO: Change this script to a ScriptableObject type
     */
    public class WeaponInspectConfigOverride : MonoBehaviour
    {
        [field: SerializeField]
        [field: Tooltip("Move the weapon closer to the camera by this multiplier amount from the original position - " +
                "0f means move all the way to the cam while 1f means no movement at all.")]
        [field: Range(0f, 1f)]
        public float inspectDistanceReductionMultiplier { get; private set; }

        [field: SerializeField]
        [field: Tooltip("Weapon will be offset by this amount to the right of the camera (cam's red axis on the positive)")]
        public float inspectRightOffsetFromCamCenter { get; private set; }

        [field: SerializeField]
        [field: Tooltip("Weapon will be offset by this amount to the left of the camera (cam's red axis on the negative")]
        public float inspectLeftOffsetFromCamCenter { get; private set; }

        [field: SerializeField]
        [field: Tooltip("Camera FOV reduction multiplier on weapon inspect - 0 = most zoomed in while 1 = no change in zoom level.")]
        [field: Range(0f, 1f)]
        public float camFOVReductionMultiplier { get; private set; }

        [field: SerializeField]
        [field: Tooltip("cannot increase the FOV above this value")]
        public float camFOVMaxValueClamp { get; private set; }//cannot increase the FOV above this value

        [field: SerializeField] 
        [field: Tooltip("cannot reduce the FOV below this value")]
        public float camFOVMinValueClamp { get; private set; }//cannot reduce the FOV below this value
    }
}
