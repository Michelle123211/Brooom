using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomizationOptionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI numberLabel;
    [SerializeField] private Image colorImage;

    // Customization data
    private Material changedMaterial;
    private SkinnedMeshRenderer changedRenderer;
    private CustomizationVariantData assignedData;

    public void Initialize(Material material, CustomizationVariantData data) {
        changedMaterial = material;
        assignedData = data;
        colorImage.color = assignedData.assignedColor; // displaying the assigned color
        numberLabel.gameObject.SetActive(false);
    }
    public void Initialize(SkinnedMeshRenderer renderer, CustomizationVariantData data) {
        changedRenderer = renderer;
        assignedData = data;
        numberLabel.text = assignedData.variantIndex.ToString(); // numbers are displayed instead of color
    }

    public void OnActiveChanged(bool isActive) {
        if (isActive) {
            // Apply customization
            assignedData.ApplyCustomization(changedMaterial, changedRenderer);
            // TODO: Save the selected option
        }
    }
}
