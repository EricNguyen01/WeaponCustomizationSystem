using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace WeaponCustomizationSystem
{
    public class MatSelectionButton : MonoBehaviour
    {
        [SerializeField] private Color buttonActiveColor;
        [SerializeField] private Color buttonInactiveColor;
        private Image buttonImageUI;

        private PaintSelectionUI paintSelectionUIOfThisButton;

        private Material materialOfThisButton;
        private List<MeshRenderer> meshesOfThisButton = new List<MeshRenderer>();

        private Color defaultMatColor;

        public static event System.Action<MatSelectionButton> OnAnotherMatSelectionButtonClicked;

        private void Awake()
        {
            buttonImageUI = GetComponent<Image>();
        }

        private void OnEnable()
        {
            MatSelectionButton.OnAnotherMatSelectionButtonClicked += OnAnotherSameTypeButtonClicked;
        }

        private void OnDisable()
        {
            paintSelectionUIOfThisButton = null;
            materialOfThisButton = null;
            meshesOfThisButton.Clear();

            MatSelectionButton.OnAnotherMatSelectionButtonClicked -= OnAnotherSameTypeButtonClicked;
        }

        public void MatSelectionButtonInitialize(PaintSelectionUI paintSelectionUI, Material mat, List<MeshRenderer> meshes, Color defaultColor)
        {
            paintSelectionUIOfThisButton = paintSelectionUI;
            materialOfThisButton = mat;
            meshesOfThisButton = meshes;
            defaultMatColor = defaultColor;
        }

        //In case if this mat selection button is the top most buttons -> set it active and in use by default
        public void SetMatSelectionButtonAsDefaultOnStartup()
        {
            //set the button as the one being in used as if the player has clicked on it
            OnMatSelectionButtonClicked();
        }

        public void OnAnotherSameTypeButtonClicked(MatSelectionButton button)
        {
            if (button == this) return;

            OnMatSelectionButtonDeselected();
        }

        //Unity Event on Mat selection Button UI
        public void OnMatSelectionButtonClicked()
        {
            OnAnotherMatSelectionButtonClicked?.Invoke(this);
            if (buttonImageUI != null) buttonImageUI.color = buttonActiveColor;

            //do a few checks to make sure the button works only when appropriate conds are met
            if(paintSelectionUIOfThisButton == null) return;
            
            if (paintSelectionUIOfThisButton.paintIconButtons == null || paintSelectionUIOfThisButton.paintIconButtons.Length == 0) return;

            if (materialOfThisButton == null || meshesOfThisButton == null || meshesOfThisButton.Count == 0) return;

            //conds met -> transfer data to paint buttons
            for(int i = 0; i < paintSelectionUIOfThisButton.paintIconButtons.Length; i++)
            {
                paintSelectionUIOfThisButton.paintIconButtons[i].GetCurrentMatToApplyButtonPaint(this);
            }

            //also transfer data to reset paint button if one is assigned!
            if (paintSelectionUIOfThisButton.resetColorButton != null) paintSelectionUIOfThisButton.resetColorButton.GetCurrentMatToApplyButtonPaint(this);
        }

        public void OnMatSelectionButtonDeselected()
        {
            if (buttonImageUI != null) buttonImageUI.color = buttonInactiveColor;
        }

        //Calls on a paint icon button click event
        public void ApplyPaintToMat(Color color)
        {
            ChangePaint(color);
        }

        //Calls on reset (color) button click event
        public void ResetMatPaintToDefault()
        {
            ChangePaint(defaultMatColor);
        }

        private void ChangePaint(Color color)
        {
            //loop through the list of mesh renderers associate with the material that this Mat Button is referencing
            for(int i = 0; i < meshesOfThisButton.Count; i++)
            {
                //for each mesh renderer, loop through its materials list
                for(int j = 0; j < meshesOfThisButton[i].materials.Length; j++)
                {
                    //if a material matches the material of this button -> change its color to the color of the paint icon button that was clicked 
                    if(meshesOfThisButton[i].materials[j].name == materialOfThisButton.name)
                    {
                        if (meshesOfThisButton[i].materials[j].HasProperty("_AlphaColor"))
                        {
                            meshesOfThisButton[i].materials[j].SetColor("_AlphaColor", color);
                            break;
                        }
                        if (meshesOfThisButton[i].materials[j].HasProperty("_BaseColor"))
                        {
                            meshesOfThisButton[i].materials[j].SetColor("_BaseColor", color);
                            break;
                        }

                        meshesOfThisButton[i].materials[j].color = color;

                        break;//break out of this inner loop
                    }
                }
            }
        }
    }
}
