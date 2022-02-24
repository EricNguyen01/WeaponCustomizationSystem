using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WeaponCustomizationSystem
{
    public class PaintIconButton : MonoBehaviour
    {
        private Image paintButtonImageUI;
        private MatSelectionButton matSelectionButtonCurrentlyInUse;

        private void Awake()
        {
            paintButtonImageUI = GetComponent<Image>();
            if(paintButtonImageUI == null)
            {
                Debug.LogWarning("Paint Button Image UI not found on button: " + name + ". An image UI component is needed so the button can use its color to change weapons' paints!");
                gameObject.SetActive(false);
            }
        }

        private void ChangePaintColor(Color color)
        {
            if (paintButtonImageUI == null) return;

            paintButtonImageUI.color = color;
        }

        public void GetCurrentMatToApplyButtonPaint(MatSelectionButton matSelectionButton)
        {
            matSelectionButtonCurrentlyInUse = matSelectionButton;
        }

        //Unity Event function in the paint button
        public void ApplyPaintOnClicked()
        {
            if(matSelectionButtonCurrentlyInUse != null)
            {
                matSelectionButtonCurrentlyInUse.ApplyPaintToMat(paintButtonImageUI.color);
            }
        }

        //UnityEvent function in the reset (color) button
        public void ResetPaintToDefault()
        {
            if (matSelectionButtonCurrentlyInUse != null)
            {
                matSelectionButtonCurrentlyInUse.ResetMatPaintToDefault();
            }
        }
    }
}
