using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAppearance : MonoBehaviour
{
    [Header("Character model parts")]
    public Material skinMaterial;
    public Material hairMaterial;
    public SkinnedMeshRenderer headRenderer;
    public SkinnedMeshRenderer hairRenderer;
    public SkinnedMeshRenderer handsRenderer;
    public SkinnedMeshRenderer outfitRenderer;
    public SkinnedMeshRenderer shoesRenderer;

    [HideInInspector] public string characterName;

    private CustomizationVariantData currentSkinCustomization;
    private CustomizationVariantData currentHairColorCustomization;
    private CustomizationVariantData currentHairStyleCustomization;
    private CustomizationVariantData currentOutfitCustomization;
    private CustomizationVariantData currentShoesCustomization;

    private CharacterCustomizationOptions customizationOptions;

    public void InitializeFromCustomizationData(CharacterCustomizationData customizationData) {
        characterName = customizationData.characterName;
        // Apply all the customizations
        ApplyCustomization(CustomizedPart.SkinTone, customizationData.skinColor);
        ApplyCustomization(CustomizedPart.HairColor, customizationData.hairColor);
        ApplyCustomization(CustomizedPart.HairStyle, customizationData.hairStyle);
        ApplyCustomization(CustomizedPart.Outfit, customizationData.outfit);
        ApplyCustomization(CustomizedPart.Shoes, customizationData.shoes);
    }

    public void RandomizeCharacterCustomization() {
        CharacterCustomizationData customizationData = new CharacterCustomizationData();
        customizationData.InitializeToRandomValues(customizationOptions);
        InitializeFromCustomizationData(customizationData);
    }

    public CharacterCustomizationData GetCustomizationData() {
        return new CharacterCustomizationData {
            skinColor = currentSkinCustomization,
            hairColor = currentHairColorCustomization,
            hairStyle = currentHairStyleCustomization,
            outfit = currentOutfitCustomization,
            shoes = currentShoesCustomization
        };
    }

    // Applies the given customization to the character (changes the given material or renderer)
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

public enum CustomizedPart { 
    SkinTone,
    HairColor,
    HairStyle,
    Outfit,
    Shoes
}
