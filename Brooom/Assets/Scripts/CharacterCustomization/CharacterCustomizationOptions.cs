using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// A ScriptableObject holding information about all different character customization options.
/// </summary>
[CreateAssetMenu(menuName = "Customization / Character Customization Options", fileName = "CharacterCustomizationOptions")]
public class CharacterCustomizationOptions : ScriptableObject
{
    // Skin color
    public MaterialColorCustomization skinTones = new();
    // Hair style - Mesh
    public MeshAndMaterialCustomization hair = new();
    // Hair color - Color
    public MaterialColorCustomization hairColor = new();
    // Outfit - Mesh + Materials
    public MeshAndMaterialCustomization outfits = new();
    // Shoes - Mesh + Materials
    public MeshAndMaterialCustomization shoes = new();
    // TODO: Face - Texture
}

/// <summary>
/// A base of the classes holding data about available customization options for a specific part (e.g. skin, outfit).
/// </summary>
[System.Serializable]
public abstract class Customization {

    /// <summary>
    /// Enumerates over all available customization options for this specific part.
    /// For example, if there are several possible meshes each with several possible materials, each particular combination will be returned separately.
    /// </summary>
    /// <returns><c>IEnumerable</c> containing all specific customization variants.</returns>
    public abstract IEnumerable<CustomizationVariantData> EnumerateVariants();

    /// <summary>
    /// Enumerates over all customization variants available for this part and counts them.
    /// </summary>
    /// <returns>Number of customization variants available.</returns>
    public int GetVariantCount() {
        int i = 0;
        foreach (var variant in EnumerateVariants()) i++;
        return i;
    }

    /// <summary>
    /// Enumerates over all customization variants available for this part and gets the one on the given index.
    /// </summary>
    /// <param name="index">Index of the customization variant to be returned within a list of all variants available.</param>
    /// <returns>Customization variant on the given index.</returns>
    public CustomizationVariantData GetVariantAt(int index) {
        int i = 0;
        foreach (var variant in EnumerateVariants()) {
            if (i == index) return variant;
            i++;
        }
        return null;
    }
    /// <summary>
    /// Selects a random customization variant from all available for this part.
    /// </summary>
    /// <returns>Random customization variant.</returns>
    public CustomizationVariantData GetRandomVariant() {
        int index = Random.Range(0, GetVariantCount());
        return GetVariantAt(index);
    }
}

/// <summary>
/// A class holding data about available customization options for a specific part where only color is changed.
/// </summary>
[System.Serializable]
public class MaterialColorCustomization : Customization {
    [Tooltip("A list of colors available as customization options.")]
    public List<Color> colors;

    /// <inheritdoc/>
	public override IEnumerable<CustomizationVariantData> EnumerateVariants() {
        foreach (Color color in colors) {
            yield return new CustomizationVariantData(color);
        }
	}
}

/// <summary>
/// A class holding data about available customization options for a specific part where meshes as well as materials are changed (each mesh can have several material options).
/// </summary>
[System.Serializable]
public class MeshAndMaterialCustomization : Customization {
    [Tooltip("A list of available meshes with their different material combinations.")]
    public List<MeshAndMaterialVariant> meshVariants;

    /// <inheritdoc/>
    public override IEnumerable<CustomizationVariantData> EnumerateVariants() {
        foreach (MeshAndMaterialVariant meshVariant in meshVariants) {
            foreach (MaterialCustomization materialsVariant in meshVariant.materialVariants) {
                yield return new CustomizationVariantData(meshVariant.displayName + "|" + materialsVariant.displayName, meshVariant.itemMesh, materialsVariant.materials);
            }
        }
	}
}

/// <summary>
/// A class containing all material combinations for a specific mesh.
/// </summary>
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

/// <summary>
/// A class containing an array of materials associated with name. This makes it more organized in the Inspector.
/// </summary>
[System.Serializable]
public class MaterialCustomization {
    [Tooltip("Name of the material array. It is displayed in the Inspector.")]
    public string displayName;
    [Tooltip("Materials creating the variant (their order is important and should correspond to the order in which materials are set for a mesh).")]
    public Material[] materials;
}

/// <summary>
/// A class containing all necessary data regarding a specific customization variant of a specific part.
/// It is used to generate options in the UI.
/// </summary>
public class CustomizationVariantData {
    /// <summary>Color associated with this variant.</summary>
    public Color assignedColor;
    /// <summary>Name which is used as identification in a save file.</summary>
    public string variantName;
    /// <summary>Mesh associated with this variant.</summary>
    public Mesh assignedMesh;
    /// <summary>Materials associated with this variant (which are then used for the Mesh).</summary>
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


/// <summary>
/// The object-oriented description of character appearance.
/// It contains character's name and <c>CustomizationVariantData</c> for each customizable part.
/// </summary>
public class CharacterCustomizationData {
    /// <summary>Name selected for the character.</summary>
    public string characterName;
    /// <summary>Customization option selected for skin color.</summary>
    public CustomizationVariantData skinColor;
    /// <summary>Customization option selected for hairstyle.</summary>
    public CustomizationVariantData hairStyle;
    /// <summary>Customization option selected for hair color.</summary>
    public CustomizationVariantData hairColor;
    /// <summary>Customization option selected for outfit.</summary>
    public CustomizationVariantData outfit;
    /// <summary>Customization option selected for shoes.</summary>
    public CustomizationVariantData shoes;

    /// <summary>
    /// Initializes all parts to their default customization options (i.e. the first option in the list) and selects an ampty string as a name.
    /// </summary>
    /// <param name="customizationOptions">All available customization options.</param>
    public void InitializeToDefaultValues(CharacterCustomizationOptions customizationOptions) {
        characterName = "";
        // Take the first variant
        skinColor = customizationOptions.skinTones.EnumerateVariants().First();
        hairStyle = customizationOptions.hair.EnumerateVariants().First();
        hairColor = customizationOptions.hairColor.EnumerateVariants().First();
        outfit = customizationOptions.outfits.EnumerateVariants().First();
        shoes = customizationOptions.shoes.EnumerateVariants().First();
    }

    /// <summary>
    /// Initializes all parts to random customization options (i.e. taking a random option in the list) and selects a random name from an already prepared list of names.
    /// </summary>
    /// <param name="customizationOptions">All available customization options.</param>
    public void InitializeToRandomValues(CharacterCustomizationOptions customizationOptions) {
        characterName = NamesManagement.GetRandomName();
        skinColor = customizationOptions.skinTones.GetRandomVariant();
        hairStyle = customizationOptions.hair.GetRandomVariant();
        hairColor = customizationOptions.hairColor.GetRandomVariant();
        outfit = customizationOptions.outfits.GetRandomVariant();
        shoes = customizationOptions.shoes.GetRandomVariant();
    }

    /// <summary>
    /// Prepares a <c>CharacterCustomizationSaveData</c> instance describing currently selected customization options so they can be then stored persistently.
    /// </summary>
    /// <returns><c>CharacterCustomizationSaveData</c> instance describing currently selected customization options.</returns>
    public CharacterCustomizationSaveData GetSaveData() {
        return new CharacterCustomizationSaveData {
            characterName = characterName,
            skinColor = skinColor.assignedColor,
            hairStyleName = hairStyle.variantName,
            hairColor = hairColor.assignedColor,
            outfitName = outfit.variantName,
            shoesName = shoes.variantName
        };
    }

    /// <summary>
    /// Sets currently selected customization options based on the given data.
    /// </summary>
    /// <param name="saveData"><c>CharacterCustomizationSaveData</c> instance describing customization options to be selected.</param>
    /// <param name="customizationOptions">All available customization options.</param>
    public void LoadFromSaveData(CharacterCustomizationSaveData saveData, CharacterCustomizationOptions customizationOptions) {
        // First load the primitive types
        characterName = saveData.characterName;
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

    // If any part doesn't have customization option assigned, this method assigns the default one
    private void ReplaceNullsWithDefaults(CharacterCustomizationOptions customizationOptions) {
        if (characterName == null) characterName = "";
        Color newColor = new();
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


/// <summary>
/// The smallest class describing current appearance of a character, i.e. chosen customization options.
/// Player's customization pereferences are stored using this class which is serialized into a json file.
/// Selected customization option for a specific part is described either by a <c>Color</c> (when only color can be changed) or by a <c>string</c> (when mesh is changed together with color).
/// </summary>
[System.Serializable]
public class CharacterCustomizationSaveData {
    /// <summary>Name which the player chose for the character.</summary>
    public string characterName;
    /// <summary>Skin color which the player chose for the character.</summary>
    public Color skinColor;
    /// <summary>Name of the hairstyle which the player chose for the character.</summary>
    public string hairStyleName;
    /// <summary>Hair color which the player chose for the character.</summary>
    public Color hairColor;
    /// <summary>Name of the outfit which the player chose for the character.</summary>
    public string outfitName;
    /// <summary>Name of the shoes which the player chose for the character.</summary>
    public string shoesName;
}