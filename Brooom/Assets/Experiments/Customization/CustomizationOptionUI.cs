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
            ApplyCustomization();
            // TODO: Save the selected option
        }
    }

    // Applies this customization (changes the assigned material or renderer)
    private void ApplyCustomization() {
        switch (assignedData.type) {
            case CustomizationType.MaterialColor: // Changing Color of a Material
                changedMaterial.color = assignedData.assignedColor;
                break;
            case CustomizationType.MeshAndMaterials: // Changing Mesh and Materials
                changedRenderer.sharedMesh = assignedData.assignedMesh;
                changedRenderer.sharedMaterials = assignedData.assignedMaterials;
                break;
        }
    }
}
