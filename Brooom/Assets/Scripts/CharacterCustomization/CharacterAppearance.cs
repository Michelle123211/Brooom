using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component responsible for changing the character's appearance based on selected customization variants.
/// </summary>
public class CharacterAppearance : MonoBehaviour
{
    [Header("Character model parts")]
    [Tooltip("SkinnedMeshRenderer component which contains mesh for head.")]
    public SkinnedMeshRenderer headRenderer;
    [Tooltip("SkinnedMeshRenderer component which contains mesh for hair.")]
    public SkinnedMeshRenderer hairRenderer;
    [Tooltip("SkinnedMeshRenderer component which contains mesh for hands.")]
    public SkinnedMeshRenderer handsRenderer;
    [Tooltip("SkinnedMeshRenderer component which contains mesh for outfit.")]
    public SkinnedMeshRenderer outfitRenderer;
    [Tooltip("SkinnedMeshRenderer component which contains mesh for shoes.")]
    public SkinnedMeshRenderer shoesRenderer;

    /// <summary>Name assigned to the character.</summary>
    [HideInInspector] public string characterName;

    // Currently selected (and applied) customization variants
    private CustomizationVariantData currentSkinCustomization;
    private CustomizationVariantData currentHairColorCustomization;
    private CustomizationVariantData currentHairStyleCustomization;
    private CustomizationVariantData currentOutfitCustomization;
    private CustomizationVariantData currentShoesCustomization;

    private CharacterCustomizationOptions customizationOptions; // all available customization options

    /// <summary>
    /// Takes customization options from the given customization data and changes character's appearance accordingly.
    /// </summary>
    /// <param name="customizationData">Customization options to set for the character.</param>
    public void InitializeFromCustomizationData(CharacterCustomizationData customizationData) {
        characterName = customizationData.characterName;
        // Apply all the customizations
        ApplyCustomization(CustomizedPart.SkinTone, customizationData.skinColor);
        ApplyCustomization(CustomizedPart.HairColor, customizationData.hairColor);
        ApplyCustomization(CustomizedPart.HairStyle, customizationData.hairStyle);
        ApplyCustomization(CustomizedPart.Outfit, customizationData.outfit);
        ApplyCustomization(CustomizedPart.Shoes, customizationData.shoes);
    }

    /// <summary>
    /// Selects random customization options and changes character's appearance accordingly.
    /// </summary>
    public void RandomizeCharacterCustomization() {
        CharacterCustomizationData customizationData = new CharacterCustomizationData();
        customizationData.InitializeToRandomValues(customizationOptions);
        InitializeFromCustomizationData(customizationData);
    }

    /// <summary>
    /// Converts current customization options set for this character to a <c>CharacterCustomizationData</c>.
    /// </summary>
    /// <returns><c>CharacterCustomizationData</c> of this character's appearance.</returns>
    public CharacterCustomizationData GetCustomizationData() {
        return new CharacterCustomizationData {
            skinColor = currentSkinCustomization,
            hairColor = currentHairColorCustomization,
            hairStyle = currentHairStyleCustomization,
            outfit = currentOutfitCustomization,
            shoes = currentShoesCustomization
        };
    }

    /// <summary>
    /// Applies the given customization to the character, i.e. changes material color and/or mesh of a corresponding renderer.
    /// </summary>
    /// <param name="part">Character part which is to be modified by the customization.</param>
    /// <param name="customization">Customization variant to be applied to the given part.</param>
    public void ApplyCustomization(CustomizedPart part, CustomizationVariantData customization) {
        if (part == CustomizedPart.SkinTone) {
            ApplyCustomization(headRenderer.material, customization);
            ApplyCustomization(handsRenderer.material, customization);
            currentSkinCustomization = customization;
        } else if (part == CustomizedPart.HairColor) {
            ApplyCustomization(hairRenderer.material, customization);
            currentHairColorCustomization = customization;
        } else if (part == CustomizedPart.HairStyle) {
            ApplyCustomization(hairRenderer, customization);
            ApplyCustomization(hairRenderer.material, currentHairColorCustomization); // re-apply the hair color
            currentHairStyleCustomization = customization;
        } else if (part == CustomizedPart.Outfit) {
            ApplyCustomization(outfitRenderer, customization);
            currentOutfitCustomization = customization;
        } else if (part == CustomizedPart.Shoes) {
            ApplyCustomization(shoesRenderer, customization);
            currentShoesCustomization = customization;
        }
    }

    // Changing Color of a Material
    private void ApplyCustomization(Material material, CustomizationVariantData customization) {
        material.color = customization.assignedColor;
    }

    // Changing Mesh and Materials
    private void ApplyCustomization(SkinnedMeshRenderer renderer, CustomizationVariantData customization) {
        renderer.sharedMesh = customization.assignedMesh;
        renderer.sharedMaterials = customization.assignedMaterials;
    }

    void Awake() {
        customizationOptions = PlayerState.Instance.customizationOptions;
        // Initialize to the player appearance
        if (gameObject.CompareTag("Player"))
            InitializeFromCustomizationData(PlayerState.Instance.CharacterCustomization);
        // Or to the default values
        else {
            CharacterCustomizationData customizationData = new CharacterCustomizationData();
            customizationData.InitializeToDefaultValues(customizationOptions);
            InitializeFromCustomizationData(customizationData);
        }
    }
}

/// <summary>
/// Different character parts which can be customized.
/// </summary>
public enum CustomizedPart { 
    SkinTone,
    HairColor,
    HairStyle,
    Outfit,
    Shoes
}
