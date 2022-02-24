using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace WeaponCustomizationSystem
{
    public class PaintSelectionUI : UITab
    {
        [SerializeField] private GameObject matSelectionContentObject;
        [SerializeField] private GameObject paintSelectionContentObject;
        [SerializeField] private MatSelectionButton matSelectionButtonPrefab;

        [field: SerializeField] public PaintIconButton resetColorButton { get; private set; }

        //[SerializeField] private PaintIconButton paintIconButtonPrefab;

        //private MatSelectionButton defaultActiveMatSelectionButton;
        private GameObject currentWeaponObjectToPaint;

        //for now, paints are hard-set in the scene 
        //later, we can auto-generate this using scriptable objects database of paints and skins...
        public PaintIconButton[] paintIconButtons { get; private set; }

        private List<MatSelectionButton> allMatSelectionButtonsList = new List<MatSelectionButton>();

        //this dict within another dict is important.
        //instead of setting the mat/meshrenderer dict everytime a weapon is being customized which is expensive, 
        //this dict caches the value so that the dict only needs to be filled once on every "new" weapon (1st time enable in scene)
        //when the weapon which was enabled and has its value cached in this dict is enabled again, the dict won't need to be filled again!
        private Dictionary<GameObject, Dictionary<Material, List<MeshRenderer>>> weaponMeshesMatDataDict = new Dictionary<GameObject, Dictionary<Material, List<MeshRenderer>>>();

        private Dictionary<Material, Color> matDefaultColorDict = new Dictionary<Material, Color>();

        private int currentMatSelectionButtonSpawned = 0;//keep track of total number of mat selection buttons

        public override void Awake()
        {
            GetPaintIconButtons();
            GetActiveMatSelectionButtonsOrCreationOneIfNoneExists();
        }

        public override void OnEnable()
        {
            OpenTab();
            WeaponItemButton.OnWeaponButtonSelectedAndWeaponObjectEnabled += OnNewWeaponObjectEnabledForCustomization;
        }

        public override void OnDisable()
        {
            WeaponItemButton.OnWeaponButtonSelectedAndWeaponObjectEnabled -= OnNewWeaponObjectEnabledForCustomization;
        }

        private void GetPaintIconButtons()
        {
            if(paintSelectionContentObject == null)
            {
                Debug.LogError("Paint Selection Content object not found! Paint Selection UI Subsystem won't work! Disabling...");
                gameObject.SetActive(false);
                return;
            }

            paintIconButtons = paintSelectionContentObject.GetComponentsInChildren<PaintIconButton>();
        }

        private void GetActiveMatSelectionButtonsOrCreationOneIfNoneExists()
        {
            if(matSelectionContentObject == null)
            {
                Debug.LogError("Mat Selection Content Object field on: " + name + " is missing. Paint Selection Subsystem won't work! Disabling...");
                gameObject.SetActive(false);
                return;
            }

            MatSelectionButton[] matSelectionButtons = matSelectionContentObject.GetComponentsInChildren<MatSelectionButton>();

            if (matSelectionButtons == null || matSelectionButtons.Length == 0)
            {
                if(matSelectionButtonPrefab == null)
                {
                    Debug.LogError("Mat Selection Button Prefab is missing. Paint Selection Subsystem won't work! Disabling...");
                    gameObject.SetActive(false);
                    return;
                }

                GameObject obj = Instantiate(matSelectionButtonPrefab.gameObject, matSelectionContentObject.transform);
                MatSelectionButton matSelectionButton = obj.GetComponent<MatSelectionButton>();
                if(matSelectionButton == null)
                {
                    Debug.LogError("Mat Selection Button script component is missing on Mat Selection Button Prefab!");
                    gameObject.SetActive(false);
                    return;
                }

                //add to all buttons
                allMatSelectionButtonsList.Add(matSelectionButton);

                currentMatSelectionButtonSpawned++;
                TextMeshProUGUI name = matSelectionButton.GetComponentInChildren<TextMeshProUGUI>();
                if (name != null) name.text = name.text + " " + currentMatSelectionButtonSpawned.ToString();
                return;
            }

            for(int i = 0; i < matSelectionButtons.Length; i++)
            {
                //only add the 1st one to active button on startup
                if(i == 0)
                {
                    if(!matSelectionButtons[i].gameObject.activeInHierarchy) matSelectionButtons[i].gameObject.SetActive(true);
                }
                else //inactive the rest
                {
                    if (matSelectionButtons[i].gameObject.activeInHierarchy) matSelectionButtons[i].gameObject.SetActive(false);
                }

                //add to all button whether active or inactive
                if (!allMatSelectionButtonsList.Contains(matSelectionButtons[i]))
                {
                    allMatSelectionButtonsList.Add(matSelectionButtons[i]);
                    currentMatSelectionButtonSpawned++;
                    TextMeshProUGUI name = matSelectionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (name != null) name.text = name.text + " " + currentMatSelectionButtonSpawned.ToString();
                }

            }
        }

        //C# Event: This function is called whenever a new weapon item button instance (check WeaponItemButton.cs) is pressed
        //which enables a weapon in scene to customize
        public void OnNewWeaponObjectEnabledForCustomization(GameObject weaponObject, bool isWeaponEnabled)
        {
            //if new weapon is an "empty" which means that we hit a weapon type that has no weapons in it when browsing the
            //weapon type selection tabs -> do the below if
            if (!isWeaponEnabled || weaponObject == null)
            {
                currentWeaponObjectToPaint = null;
                MatchAndEnableMaterialSelectionButtonsToMaterialNumber(1);//return to default mat selection button num if no weapon to customize paint
                ActiveMatSelectionButtonsSetup(null);//since no weapon is available for customization -> active mat selection should not work 
                return;
            }

            currentWeaponObjectToPaint = weaponObject;

            Dictionary<Material, List<MeshRenderer>> dict = new Dictionary<Material, List<MeshRenderer>>();

            //If the weaponObject parameter turns out to be a weapon that has been processed by this func before, use the values
            //store in the "weaponMeshesMatDataDict" with the key as this weapon object.
            //Else, generate a new key/value pair 
            /*if (weaponMeshesMatDataDict.ContainsKey(currentWeaponObjectToPaint))
            {
                dict = weaponMeshesMatDataDict[currentWeaponObjectToPaint];
            }
            else
            {*/
                dict = SetWeaponMeshMatDict(currentWeaponObjectToPaint);
                //weaponMeshesMatDataDict.Add(currentWeaponObjectToPaint, dict);
            //}

            //Check if number of Mat Selection Button matches number of materials
            //Enable a number of buttons that are equal to the number of mats
            MatchAndEnableMaterialSelectionButtonsToMaterialNumber(dict.Count);

            //Set up the buttons data
            ActiveMatSelectionButtonsSetup(dict);

            SetDefaultActiveMatSelectionButtonOnWeaponChanged();
        }

        private void SetDefaultActiveMatSelectionButtonOnWeaponChanged()
        {
            //find the 1st active mat selection button in list and set it as the default active one then exit completely
            for(int i = 0; i < allMatSelectionButtonsList.Count; i++)
            {
                if (allMatSelectionButtonsList[i].gameObject.activeInHierarchy)
                {
                    //defaultActiveMatSelectionButton = allMatSelectionButtonsList[i];
                    allMatSelectionButtonsList[i].SetMatSelectionButtonAsDefaultOnStartup();
                    return;
                }
            }
        }

        private void MatchAndEnableMaterialSelectionButtonsToMaterialNumber(int num)
        {
            int activeCounts = 0;

            for(int i = 0; i < allMatSelectionButtonsList.Count; i++)
            {
                if (allMatSelectionButtonsList[i].gameObject.activeInHierarchy) activeCounts++;
            }

            if (activeCounts == num) return;

            if(activeCounts < num)
            {
                //if all mat selection buttons in pool(list) is active and still not enough -> instantiate more
                if(activeCounts == allMatSelectionButtonsList.Count)
                {
                    int buttonsNeed = num - activeCounts;

                    for(int i = 0; i < buttonsNeed; i++)
                    {
                        GameObject obj = Instantiate(matSelectionButtonPrefab.gameObject, matSelectionContentObject.transform);
                        MatSelectionButton matSelectionButton = obj.GetComponent<MatSelectionButton>();

                        allMatSelectionButtonsList.Add(matSelectionButton);

                        currentMatSelectionButtonSpawned++;
                        TextMeshProUGUI name = matSelectionButton.GetComponentInChildren<TextMeshProUGUI>();
                        if (name != null) name.text = name.text + " " + currentMatSelectionButtonSpawned.ToString();
                    }

                    return;
                }

                //if not all buttons are active in allMatSelectionButtonList -> active until activeCoutns == num OR all buttons in list are active
                for(int i = 0; i < allMatSelectionButtonsList.Count; i++)
                {
                    //if a button in list is inactive -> activate it!
                    if (!allMatSelectionButtonsList[i].gameObject.activeInHierarchy)
                    {
                        allMatSelectionButtonsList[i].gameObject.SetActive(true);
                        activeCounts++;
                        if (activeCounts == num) return;//if number of buttons = required -> exit func completely
                    }
                }
                //if all buttons in list are active and still not enough -> recursively calling this func again 
                if (activeCounts < num)
                {
                    //when recursively calling this func, the above if will definitely be called until enough buttons are instantiated
                    MatchAndEnableMaterialSelectionButtonsToMaterialNumber(num);
                    return;
                }
            }

            //if active counts > num -> inactive each until active counts = required num
            if(activeCounts > num)
            {
                int count = 0;

                for(int i = 0; i < allMatSelectionButtonsList.Count; i++)
                {
                    if(count < num)
                    {
                        count++;
                        continue;
                    }

                    allMatSelectionButtonsList[i].gameObject.SetActive(false);
                }
            }
        }

        //This func provides the neccessary references and conds for the Active Mat Selection UI Buttons to work properly
        private void ActiveMatSelectionButtonsSetup(Dictionary<Material, List<MeshRenderer>> meshMatData)
        {
            int buttonNum = 0;

            if(meshMatData == null)
            {
                for (int i = 0; i < allMatSelectionButtonsList.Count; i++)
                {
                    if (allMatSelectionButtonsList[i].gameObject.activeInHierarchy)
                    {
                        //setting null to the below parameters will prevent the paint button from setting the paint to "empty" weapon
                        allMatSelectionButtonsList[i].MatSelectionButtonInitialize(null, null, null, Color.black);
                        return;
                    }
                }
            }

            List<Material> matKeys = new List<Material>(meshMatData.Keys);

            if(matKeys == null || matKeys.Count == 0)
            {
                Debug.LogWarning("There are no material in material keys of meshMatData dict!");
                return;
            }

            for(int i = 0; i < allMatSelectionButtonsList.Count; i++)
            {
                if (allMatSelectionButtonsList[i].gameObject.activeInHierarchy)
                {
                    //Get the default color to give to the current Mat Selection Button which will then give to the color Reset button
                    Color defaultColorOfMat;
                    if (!matDefaultColorDict.ContainsKey(matKeys[buttonNum]))
                    {
                        defaultColorOfMat = SetDefaultMaterialColor(matKeys[buttonNum]);
                        matDefaultColorDict.Add(matKeys[buttonNum], defaultColorOfMat);
                    }
                    else
                    {
                        defaultColorOfMat = matDefaultColorDict[matKeys[buttonNum]];
                    }

                    //once default color is determined->
                    //perform transfer of data to current mat selection button in mat selection button list
                    allMatSelectionButtonsList[i].MatSelectionButtonInitialize(this, matKeys[buttonNum], meshMatData[matKeys[buttonNum]], defaultColorOfMat);
                    buttonNum++;
                }
            }
        }

        private Dictionary<Material, List<MeshRenderer>> SetWeaponMeshMatDict(GameObject weaponObj)//gonna be real expensive operation...
        {
            //doing some availability checks first
            if (weaponObj.transform.childCount == 0)
            {
                Debug.LogWarning("Checking object: " + weaponObj.name + " doesnt have any children. Aborting setting weapon meshes and mats dict!");
            }

            if (weaponObj.transform.childCount == 1)
            {
                weaponObj = weaponObj.transform.GetChild(0).gameObject;
            }

            MeshRenderer[] meshRenderers = weaponObj.GetComponentsInChildren<MeshRenderer>();

            if (meshRenderers == null || meshRenderers.Length == 0)
            {
                Debug.LogWarning("No Mesh Renderer components found in obj: " + weaponObj.name + ". Aborting setting weapon meshes and mats dict!");
                return null;
            }
            //checks done!

            List<Material> materialsList = new List<Material>();
            Dictionary<Material, List<MeshRenderer>> weaponMeshesMatDict = new Dictionary<Material, List<MeshRenderer>>();

            //Get the list of all materials instance IDs used by this weapon first
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                Material[] mats = meshRenderers[i].materials;
                for (int j = 0; j < mats.Length; j++)
                {
                    if(materialsList.Count == 0)
                    {
                        materialsList.Add(mats[j]);
                        continue;
                    }

                    bool alreadyContainedMat = false;
                    for(int f = 0; f < materialsList.Count; f++)
                    {
                        if (mats[j].name == materialsList[f].name)
                        {
                            alreadyContainedMat = true;
                            break;
                        }
                    }
                    if (alreadyContainedMat) continue;

                    materialsList.Add(mats[j]);
                }
            }

            //Then iterate again through the just generated list from above 
            for (int i = 0; i < materialsList.Count; i++)
            {
                //create a temp list
                List<MeshRenderer> meshesUsingThisMat = new List<MeshRenderer>();

                //iterate through all the mesh renderer children 
                for (int j = 0; j < meshRenderers.Length; j++)
                {
                    //for each mesh renderer child -> iterate through its materials list
                    for (int f = 0; f < meshRenderers[j].materials.Length; f++)
                    {
                        //if any mat in the list is = to the current checking material in the materialsList 
                        if (meshRenderers[j].materials[f].name == materialsList[i].name)
                        {
                            meshesUsingThisMat.Add(meshRenderers[j]);//add to the temp list
                            break;
                        }
                    }
                }

                //after iterated through all the mesh renderer children objs and all of their mat lists, 
                //we now have a list of all the children obj that use the current material that is being checked in the materialsList
                //the current checking Mat is the key / the list of materials that use it is the value
                //fill dict with the above data
                if (!weaponMeshesMatDict.ContainsKey(materialsList[i])) weaponMeshesMatDict.Add(materialsList[i], meshesUsingThisMat);
            }

            return weaponMeshesMatDict;
        }

        private Color SetDefaultMaterialColor(Material mat)
        {
            Color defaultMatColor;

            if (mat.HasProperty("_AlphaColor"))
            {
                return defaultMatColor = mat.GetColor("_AlphaColor");
            }
            if (mat.HasProperty("_BaseColor"))
            {
                return defaultMatColor = mat.GetColor("_BaseColor");
            }

            return defaultMatColor = mat.color;
        }

        public override void OpenTab()
        {
            //TODO IF TIME ALLOWS: Expand tab (and enabling if coming out of inspect mode) on tab opens!
            if(!gameObject.activeInHierarchy) gameObject.SetActive(true);
            base.OpenTab();
        }

        public override void CloseTab()
        {
            //TODO IF TIME ALLOWS: Minimize tab instead of disable on tab closes!

            if (isInInspectMode) gameObject.SetActive(false);
            base.CloseTab();
        }
    }
}
