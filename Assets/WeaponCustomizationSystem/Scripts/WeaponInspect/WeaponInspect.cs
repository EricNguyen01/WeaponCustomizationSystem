using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.EventSystems;

namespace WeaponCustomizationSystem
{
    /*
     * This script is to be placed on the weapon game object itself
     * The script will handle the positioning and rotation of the weapon on inspection started and ended
     * The script will handle the rotation of the weapon following mouse inputs from the users during the inspect process
     */
    public class WeaponInspect : MonoBehaviour
    {
        private Camera inspectCam;
        private CinemachineBrain cinemachineBrain;
        private CinemachineVirtualCamera currentVirtualCam;

        private Transform camTransform;
        private float originalCamFOV;
        private float inspectStartCamFOV;
        private float currentCamFOV;

        private GameObject currentWeaponObjectInCustomization;
        private bool isInspectEnabled = false;
        private bool isInTransition = false;

        [Header("Weapon Inspect Config")]
        [SerializeField] [Range(50f, 150f)] private float inspectDragRotateSpeed = 85f;
        [SerializeField] [Range(0.3f, 1f)] private float inspectDragAccelerateTime = 0.5f;

        [SerializeField] 
        [Tooltip("Time it takes for the weapon being customized to move to its inspect pos and rot after inspect button is clicked")] 
        [Range(0f, 0.5f)]
        private float inspectTransitionTime = 0.24f;

        [SerializeField] 
        [Tooltip("Move the weapon closer to the camera by this multiplier amount from the original position - " +
                "0f means move all the way to the cam while 1f means no movement at all.")] 
        [Range(0f, 1f)]
        private float inspectDistanceReductionMultiplier = 0.88f;

        [SerializeField] [Tooltip("Weapon will be offset by this amount to the right (world's dir)")]
        private float inspectRightOffsetFromCamCenter = 0f;

        [SerializeField] [Tooltip("Weapon will be offset by this amount to the left (world's dir)")]
        private float inspectLeftOffsetFromCamCenter = 0f;

        [SerializeField] 
        [Tooltip("Camera FOV reduction multiplier on weapon inspect - 0 = most zoomed in while 1 = no change in zoom level.")] 
        [Range(0f, 1f)]
        private float camFOVReductionMultiplier = 0.8f;

        [SerializeField] private float camFOVMaxValueClamp = 150f;//cannot increase the FOV above this value
        [SerializeField] private float camFOVMinValueClamp = 30f;//cannot reduce the FOV below this value

        private Vector3 weaponOriginalPos;
        private Quaternion weaponOriginalRot;

        Vector3 inspectStartPos;
        Quaternion inspectStartRot;

        public static event System.Action<bool> OnWeaponInInspection;
        public static event System.Action<bool> OnWeaponInspectInTransition;

        private void Awake()
        {
            inspectCam = Camera.main;
            cinemachineBrain = inspectCam.GetComponent<CinemachineBrain>();
        }

        private void OnEnable()
        {
            WeaponItemButton.OnWeaponButtonSelectedAndWeaponObjectEnabled += OnCurrentWeaponInCustomizationChanged;
            WeaponInspectButton.OnWeaponInspected += OnWeaponInspectEnabled;
        }

        private void OnDisable()
        {
            WeaponItemButton.OnWeaponButtonSelectedAndWeaponObjectEnabled -= OnCurrentWeaponInCustomizationChanged;
            WeaponInspectButton.OnWeaponInspected -= OnWeaponInspectEnabled;

            StopAllCoroutines();
        }

        private void OnInspectStarted()
        {
            //Find the correct camera being used for inspect (either cinemachine or normal cam)
            //check if theres an inspect config override component being placed on the cam and if there is one, replace the config values.
            GetValidCamerasAndInspectConfigOverride();
            
            //record the original pos and rot first
            weaponOriginalPos = transform.position;
            weaponOriginalRot = transform.rotation;

            OnWeaponInInspection?.Invoke(true);//trigger in weapon inspection event

            //Transition the weapon to the center of camera and then have camera FOV zooms in on the weapon
            ProcessCamFOVAndWeaponPlacementOnInspectStarted();
        }

        private IEnumerator InspectLoopCoroutine()
        {
            //if for some reasons inspect is not enabled but this coroutine is alr started, do nothing and move to next frames without exiting the coroutine
            if (!isInspectEnabled) yield return null;

            float mouseHoldTime = 0f;
            float acceleration = 0f;
            float accelTime = 0f;

            while (isInspectEnabled)
            {
                if (Input.GetButtonUp("Fire1"))
                {
                    mouseHoldTime = 0f;
                    acceleration = 0f;
                    accelTime = 0f;
                }

                if (Input.GetButton("Fire1"))
                {
                    mouseHoldTime += Time.fixedDeltaTime;

                    if (mouseHoldTime >= 0.12f)
                    {
                        //lerp acceleration rate within the set acceleration time
                        //acceleration lerp from 0f (no speed or rotation) to 1f (max speed and fastest rotation)
                        if (accelTime <= inspectDragAccelerateTime)
                        {
                            accelTime += Time.fixedDeltaTime;
                            acceleration = Mathf.Lerp(0f, 1f, accelTime / inspectDragAccelerateTime);
                        }
                        else
                        {
                            accelTime = inspectDragAccelerateTime;
                            acceleration = 1f;
                        }

                        //calculate weapon inspect drag rotation and drag speed with acceleration
                        float xDelta = Input.GetAxis("Mouse X");
                        float yDelta = Input.GetAxis("Mouse Y");
                        Vector3 eulerAnglesChange = new Vector3(-yDelta, xDelta);
                        Vector3 eulerAngles = transform.eulerAngles;
                        eulerAngles += eulerAnglesChange * inspectDragRotateSpeed * acceleration * Time.fixedDeltaTime;
                        transform.eulerAngles = eulerAngles;
                    }
                }

                //adjust FOV based on mouse scroll
                MouseScrollFOVAdjustDuringInspect();

                yield return new WaitForFixedUpdate();
            }
        }

        private void OnInspectEnded()
        {
            //Coroutine to lerp the weapon back to its original pos/rot and then have camera FOV zooms out to its original zoom lv.
            StartCoroutine(CamAndWeaponTransitionCoroutine(inspectStartPos, transform.rotation, currentCamFOV, weaponOriginalPos, weaponOriginalRot, originalCamFOV, inspectTransitionTime));

            //wait for above transition coroutine to finish before disabling weapon inspection (stop rotating weapon following inputs)
            StartCoroutine(SetInspectStatusAfterTransition(false));
        }

        private void ProcessCamFOVAndWeaponPlacementOnInspectStarted()
        {
            //do some valid checks first............................................

            //Get the transform of the valid inspect cam (cinemachine or normal cam) and calculates cam FOV multiplier
            if (currentVirtualCam != null)//if using cinemachine cams
            {
                camTransform = currentVirtualCam.transform;
                inspectStartCamFOV = currentVirtualCam.m_Lens.FieldOfView * camFOVReductionMultiplier;
            }
            if (currentVirtualCam == null && inspectCam != null)//if using normal cams
            {
                camTransform = inspectCam.transform;
                inspectStartCamFOV = inspectCam.fieldOfView * camFOVReductionMultiplier;
            }
            if(currentVirtualCam == null && inspectCam == null)//if no cam exists
            {
                Debug.LogWarning("No valid camera for weapon inspection found. Disabling weapon inspect!");
                StopAllCoroutines();
                enabled = false;
                return;
            }

            //if for some reasons cam transform is still null even if a cam exists in scene and is in use for inspect
            if(camTransform == null)
            {
                Debug.LogWarning("In use camera's transform component is NULL!");
                StopAllCoroutines();
                enabled = false;
                return;
            }

            //all valid checks done!............................................

            //calculate and set the weapon's starting inspect pos
            inspectStartPos = CalculateInspectStartPosFromCam(camTransform);

            //calculate weapon rotation facing cam
            inspectStartRot = CalculateInspectStartRotationFromCam(camTransform, inspectStartPos);

            //coroutine to lerp the cam's FOV and weapon's pos and rot to their appropriate values for inspect
            StartCoroutine(CamAndWeaponTransitionCoroutine(weaponOriginalPos, weaponOriginalRot, originalCamFOV, inspectStartPos, inspectStartRot, inspectStartCamFOV, inspectTransitionTime));

            //wait for above transition coroutine to finish before enabling weapon inspection (rotating weapon following inputs)
            StartCoroutine(SetInspectStatusAfterTransition(true));
        }

        private IEnumerator CamAndWeaponTransitionCoroutine(Vector3 fromPos, Quaternion fromRot, float fromFOV, Vector3 toPos, Quaternion toRot, float toFOV, float time)
        {
            float currentTime = 0f;

            isInspectEnabled = false;
            isInTransition = true;

            StopCoroutine(InspectLoopCoroutine());

            OnWeaponInspectInTransition?.Invoke(true);

            while(currentTime <= time)
            {
                currentTime += Time.fixedDeltaTime;

                //lerp pos
                transform.position = Vector3.Lerp(fromPos, toPos, currentTime / time);

                //lerp rot
                transform.rotation = Quaternion.Lerp(fromRot, toRot, currentTime / time);

                //lerp FOV
                if (currentVirtualCam != null) currentVirtualCam.m_Lens.FieldOfView = Mathf.Lerp(fromFOV, toFOV, currentTime / time);
                else if (currentVirtualCam == null && inspectCam != null) inspectCam.fieldOfView = Mathf.Lerp(fromFOV, toFOV, currentTime / time);

                yield return new WaitForFixedUpdate();
            }

            //make sure that all values (pos, rot, and fov) are correct and fixed (sometimes values are slightly off after lerping)
            transform.position = toPos;

            transform.rotation = toRot;

            if (currentVirtualCam != null)
            {
                currentVirtualCam.m_Lens.FieldOfView = toFOV;
                currentCamFOV = currentVirtualCam.m_Lens.FieldOfView;
            }
            else if (currentVirtualCam == null && inspectCam != null)
            {
                inspectCam.fieldOfView = toFOV;
                currentCamFOV = inspectCam.fieldOfView;
            }

            //set and trigger inspect transition finish event
            isInTransition = false;
            OnWeaponInspectInTransition?.Invoke(false);
        }

        private IEnumerator SetInspectStatusAfterTransition(bool inspectEnableStatus)
        {
            yield return new WaitUntil(() => !isInTransition);//wait until inspect transition is done

            //set whether the user can inspect the weapon freely (weapon updates its rotation following inputs)
            isInspectEnabled = inspectEnableStatus;

            //if inspect is enabled (user in inspect mode) -> start inspect loop coroutine and break out of this coroutine.
            if (isInspectEnabled)
            {
                StartCoroutine(InspectLoopCoroutine());
                yield break;
            }

            //else if inspect is not enabled which means that the user has exit inspect mode -> set weapon in inspection event to false and stop inspect loop
            OnWeaponInInspection?.Invoke(false);
            StopCoroutine(InspectLoopCoroutine());
        }

        private Vector3 CalculateInspectStartPosFromCam(Transform camTransform)
        {
            Vector3 pos;

            //get the distance bt/ weapon and cam
            float distanceFromCam = Vector3.Distance(camTransform.position, transform.position);
            //get the position on the cam forward dir (blue axis) based on the dist found above (a point on the blue arrow dir with found dist)
            pos = camTransform.position + camTransform.forward * distanceFromCam;

            //move the weapon on the red axis according to the right/left offset modifier values first
            Vector3 originalPosModX = transform.position + transform.right * -(inspectRightOffsetFromCamCenter - inspectLeftOffsetFromCamCenter);
            //find distance from the weapon (after moving on red axis) to the founded point on the cam's blue axis found on line 215
            float distanceFromNewXPos = (pos - originalPosModX).magnitude;
            //find distance from the weapon (original pos) to the founded point on the cam's blue axis found on line 215
            float distanceFromOriginal = (pos - transform.position).magnitude;
            //find the difference bt/ the 2 distances found above
            float distDiff = distanceFromNewXPos - distanceFromOriginal;
            //find the dist that the weapon needs to move on the right axis to get to the right position which is the center of the cam with or without offsets
            float moveDist = distanceFromOriginal - distDiff;

            //get the point that the weapon should end up at after moving by the above founded distance on the red axis
            Vector3 tempPos = transform.position + transform.right * moveDist;
            //get the direction from that end point to the camera
            Vector3 dir = tempPos - camTransform.position;
            //the final pos that the weapon will end up at will be somewhere along the line found in line 231
            //distance reduction multiplier = 0 means the point will be at the start of the line while 1 means it will be at the end.
            pos = camTransform.position + dir.normalized * (distanceFromCam * inspectDistanceReductionMultiplier);

            return pos;//return the final pos found
        }

        private Quaternion CalculateInspectStartRotationFromCam(Transform camTransform, Vector3 weaponPos)
        {
            Quaternion rot;

            float distanceFromCam = Vector3.Distance(camTransform.position, transform.position);
            Vector3 pos = camTransform.position + camTransform.forward * (distanceFromCam * inspectDistanceReductionMultiplier);

            //direction from camera to weapon inspect start pos doesnt take into account offset (hence the weaponPos.x - (rightoffset - leftoffset))
            Vector3 dir = camTransform.position - pos;
            dir.Normalize();
            rot = Quaternion.LookRotation(dir);

            return rot;
        }

        private void MouseScrollFOVAdjustDuringInspect()
        {
            float scrollY = Input.mouseScrollDelta.y;

            if (currentVirtualCam != null)
            {
                currentVirtualCam.m_Lens.FieldOfView -= scrollY * 40f * Time.fixedDeltaTime;
                currentVirtualCam.m_Lens.FieldOfView = Mathf.Clamp(currentVirtualCam.m_Lens.FieldOfView, camFOVMinValueClamp, camFOVMaxValueClamp);
                currentCamFOV = currentVirtualCam.m_Lens.FieldOfView;
            }
            else if (currentVirtualCam == null && inspectCam != null)
            {
                inspectCam.fieldOfView -= scrollY * 40f * Time.fixedDeltaTime;
                inspectCam.fieldOfView = Mathf.Clamp(inspectCam.fieldOfView, camFOVMinValueClamp, camFOVMaxValueClamp);
                currentCamFOV = inspectCam.fieldOfView;
            }
        }

        private void GetValidCamerasAndInspectConfigOverride()
        {
            WeaponInspectConfigOverride inspectConfigOverride = null;

            //check for valid inspect cam availability and record cam's original FOV
            if (cinemachineBrain != null)//for cinemachine cams
            {
                currentVirtualCam = cinemachineBrain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>();

                //for some reasons, sometimes get virtual cam component (above statement) cannot find the cinemachine virtual cam component in virtual cam object 
                //if the above happens, we have to resort to FindObjectsOfType.
                if (currentVirtualCam == null)
                {
                    foreach (CinemachineVirtualCamera cam in FindObjectsOfType<CinemachineVirtualCamera>())
                    {
                        if (cam.Priority == cinemachineBrain.ActiveVirtualCamera.Priority)
                        {
                            currentVirtualCam = cam;
                            break;
                        }
                    }
                }
                if (currentVirtualCam != null)
                {
                    originalCamFOV = currentVirtualCam.m_Lens.FieldOfView;
                    inspectConfigOverride = currentVirtualCam.gameObject.GetComponent<WeaponInspectConfigOverride>();
                }
            }
            if (currentVirtualCam == null && inspectCam != null) //for normal non-cinemachine cams
            {
                originalCamFOV = inspectCam.fieldOfView;
                inspectConfigOverride = inspectCam.gameObject.GetComponent<WeaponInspectConfigOverride>();
            }
            if (currentVirtualCam == null && inspectCam == null) //if no valid cam is found - disable script
            {
                Debug.LogWarning("No camera for weapon inspection found in scene. Disabling weapon inspect!");
                StopAllCoroutines();
                enabled = false;
                return;
            }

            //if an inspect config override script is found on the current camera being used for weapon inspection -> replace the values accordingly
            if (inspectConfigOverride != null)
            {
                inspectDistanceReductionMultiplier = inspectConfigOverride.inspectDistanceReductionMultiplier;
                inspectRightOffsetFromCamCenter = inspectConfigOverride.inspectRightOffsetFromCamCenter;
                inspectLeftOffsetFromCamCenter = inspectConfigOverride.inspectLeftOffsetFromCamCenter;
                camFOVReductionMultiplier = inspectConfigOverride.camFOVReductionMultiplier;
                camFOVMaxValueClamp = inspectConfigOverride.camFOVMaxValueClamp;
                camFOVMinValueClamp = inspectConfigOverride.camFOVMinValueClamp;
            }
        }

        /*
         * C# Events subscription functions below:..............................................
         */

        //This function subscribed to the WeaponItemButton.cs' OnWeaponButtonSelectedAndWeaponObjectEnabled event
        //GameObject weaponObj - the weapon game object
        //bool isSelected - is this weapon game obj the one being clicked on(become active) OR the one being replaced by the one that was clicked on(becoming inactive)
        public void OnCurrentWeaponInCustomizationChanged(GameObject weaponObj, bool isSelected)
        {
            currentWeaponObjectInCustomization = weaponObj;
        }

        public void OnWeaponInspectEnabled(bool inspectEnabled)
        {
            //check if the current weapon in customization is not null and is the exact same weapon game object that this script attached to
            if (currentWeaponObjectInCustomization == null || currentWeaponObjectInCustomization != gameObject) return;

            //if inspection is enabled
            if (inspectEnabled)
            {
                OnInspectStarted();
                return;
            }

            //else
            OnInspectEnded();
        }
    }
}
