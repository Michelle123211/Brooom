using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Customization / Character Customization Options", fileName = "CharacterCustomizationOptions")]
public class CharacterCustomizationOptions : ScriptableObject
{
    // Skin color
    public MaterialColorCustomization skinTones = new MaterialColorCustomization();
    // Hair style - Mesh
    public MeshAndMaterialCustomization hair = new MeshAndMaterialCustomization();
    // Hair color - Color
    public MaterialColorCustomization hairColor = new MaterialColorCustomization();
    // Outfit - Mesh + Materials
    public MeshAndMaterialCustomization outfits = new MeshAndMaterialCustomization();
    // Shoes - Mesh + Materials
    public MeshAndMaterialCustomization shoes = new MeshAndMaterialCustomization();
    // TODO: Face - Texture
}

// A base of the classes holding data about available customization options for a specific part (e.g. skin, outfit)
[System.Serializable]
public abstract class Customization {
    public abstract IEnumerable<CustomizationVariantData> EnumerateVariants();
}

// A class holding data about available customization options changing only color
[System.Serializable]
public class MaterialColorCustomization : Customization {
    public List<Color> colors;

	public override IEnumerable<CustomizationVariantData> EnumerateVariants() {
        foreach (Color color in colors) {
            yield return new CustomizationVariantData(color);
        }
	}
}

// A class holding data about available customization options changing meshes and material combinations
[System.Serializable]
public class MeshAndMaterialCustomization : Customization {
    [Tooltip("A list of available meshes with their different material combinations.")]
    public List<MeshAndMaterialVariant> meshVariants;

	public override IEnumerable<CustomizationVariantData> EnumerateVariants() {
        foreach (MeshAndMaterialVariant meshVariant in meshVariants) {
            foreach (MaterialCustomization materialsVariant in meshVariant.materialVariants) {
                yield return new CustomizationVariantData(meshVariant.displayName + "|" + materialsVariant.displayName, meshVariant.itemMesh, materialsVariant.materials);
            }
        }
	}
}

// All material combinations connected to a specific mesh
[System.Serializable]
public class MeshAndMaterialVariant {
    [Tooltip("The name is used as the name of the dropdown too.")]
    public string displayName;
    [Tooltip("The mesh representing the item customizable even further with different material combinations.")]
    public Mesh itemMesh;
    [Tooltip("These have no function, it is simply for documentation purposes to easily map each material in the variant to its purpose.")]
    public List<string> materialNames;
    [Tooltip("Different combinations of materials creating different variants.")]
    public List<MaterialCustomization> materialVariants;
}

// An array of materials associated with name to make it more organized in the Inspector
[System.Serializable]
public class MaterialCustomization {
    [Tooltip("The name is used as the name of the dropdown too.")]
    public string displayName;
    [Tooltip("Materials creating the variant. The order is important.")]
    public Material[] materials;
}

// All necessary data regarding a specific customization option (used to generate options in the UI]
public class CustomizationVariantData {
    public Color assignedColor;
    public string variantName; // this name is used as identification in the save file
    public Mesh assignedMesh;
    public Material[] assignedMaterials;

    public CustomizationVariantData(Color color) {
        assignedColor = color;
    }

    public CustomizationVariantData(string name, Mesh mesh, Material[] materials) {
        variantName = name;
        assignedMesh = mesh;
        assignedMaterials = materials;
    }
}

public enum CustomizationType { 
    MaterialColor,
    MeshAndMaterials
}


// The description of appearance of the character
public class CharacterCustomizationData {
    public string playerName;
    public CustomizationVariantData skinColor;
    public CustomizationVariantData hairStyle;
    public CustomizationVariantData hairColor;
    public CustomizationVariantData outfit;
    public CustomizationVariantData shoes;

    public void InitializeToDefaultValues(CharacterCustomizationOptions customizationOptions) {
        playerName = "";
        // Take the first variant
        skinColor = customizationOptions.skinTones.EnumerateVariants().First();
        hairStyle = customizationOptions.hair.EnumerateVariants().First();
        hairColor = customizationOptions.hairColor.EnumerateVariants().First();
        outfit = customizationOptions.outfits.EnumerateVariants().First();
        shoes = customizationOptions.shoes.EnumerateVariants().First();
    }

    public CharacterCustomizationSaveData GetSaveData() {
        return new CharacterCustomizationSaveData {
            playerName = playerName,
            skinColor = skinColor.assignedColor,
            hairStyleName = hairStyle.variantName,
            hairColor = hairColor.assignedColor,
            outfitName = outfit.variantName,
            shoesName = shoes.variantName
        };
    }

    public void LoadFromSaveData(CharacterCustomizationSaveData saveData, CharacterCustomizationOptions customizationOptions) {
        // First load the primitive types
        playerName = saveData.playerName;
        skinColor = new CustomizationVariantData(saveData.skinColor);
        hairColor = new CustomizationVariantData(saveData.hairColor);
        // Then search for the variants according to their names among...
        // ... hair styles
        foreach (var variant in customizationOptions.hair.EnumerateVariants()) {
            if (variant.variantName == saveData.hairStyleName) {
                hairStyle = variant;
                break;
            }
        }
        // ... outfits
        foreach (var variant in customizationOptions.outfits.EnumerateVariants()) {
            if (variant.variantName == saveData.outfitName) {
                outfit = variant;
                break;
            }
        }
        // ... shoes
        foreach (var variant in customizationOptions.shoes.EnumerateVariants()) {
            if (variant.variantName == saveData.shoesName) {
                shoes = variant;
                break;
            }
        }
        // If anything was not found, use the default value instead
        ReplaceNullsWithDefaults(customizationOptions);
    }

    private void ReplaceNullsWithDefaults(CharacterCustomizationOptions customizationOptions) {
        if (playerName == null) playerName = "";
        Color newColor = new Color();
        if (skinColor.assignedColor == newColor)
            skinColor = customizationOptions.skinTones.EnumerateVariants().First();
        if (hairColor.assignedColor == newColor)
            hairColor = customizationOptions.hairColor.EnumerateVariants().First();
        if (hairStyle == null)
            hairStyle = customizationOptions.hair.EnumerateVariants().First();
        if (outfit == null)
            outfit = customizationOptions.outfits.EnumerateVariants().First();
        if (shoes == null)
            shoes = customizationOptions.shoes.EnumerateVariants().First();
    }
}


// The smallest class describing the current appearance of the character
[System.Serializable]
public class CharacterCustomizationSaveData {
    public string playerName;
    public Color skinColor;
    public string hairStyleName;
    public Color hairColor;
    public string outfitName;
    public string shoesName;
}