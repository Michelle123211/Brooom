using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAppearance : MonoBehaviour
{
    [Header("Character model parts")]
    public Material skinMaterial;
    public Material hairMaterial;
    public SkinnedMeshRenderer hairRenderer;
    public SkinnedMeshRenderer outfitRenderer;
    public SkinnedMeshRenderer shoesRenderer;

    private CustomizationVariantData currentSkinCustomization;
    private CustomizationVariantData currentHairColorCustomization;
    private CustomizationVariantData currentHairStyleCustomization;
    private CustomizationVariantData currentOutfitCustomization;
    private CustomizationVariantData currentShoesCustomization;

    public void InitializeFromCustomizationData(CharacterCustomizationData customizationData) {
        // Apply all the customizations
        ApplyCustomization(CustomizedPart.SkinTone, customizationData.skinColor);
        ApplyCustomization(CustomizedPart.HairColor, customizationData.hairColor);
        ApplyCustomization(CustomizedPart.HairStyle, customizationData.hairStyle);
        ApplyCustomization(CustomizedPart.Outfit, customizationData.outfit);
        ApplyCustomization(CustomizedPart.Shoes, customizationData.shoes);
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
            ApplyCustomization(skinMaterial, customization);
            currentSkinCustomization = customization;
        } else if (part == CustomizedPart.HairColor) {
            ApplyCustomization(hairMaterial, customization);
            currentHairColorCustomization = customization;
        } else if (part == CustomizedPart.HairStyle) {
            ApplyCustomization(hairRenderer, customization);
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

    // Start is called before the first frame update
    void Awake()
    {
        InitializeFromCustomizationData(PlayerState.Instance.CharacterCustomization);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public enum CustomizedPart { 
    SkinTone,
    HairColor,
    HairStyle,
    Outfit,
    Shoes
}
