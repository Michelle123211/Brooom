using System.Collections;
using System.Collections.Generic;
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
        int i = 0;
        foreach (MeshAndMaterialVariant meshVariant in meshVariants) {
            foreach (MaterialCustomization materialsVariant in meshVariant.materialVariants) {
                i++;
                yield return new CustomizationVariantData(i, meshVariant.itemMesh, materialsVariant.materials);
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
    public CustomizationType type;
    public Color assignedColor;
    public int variantIndex;
    public Mesh assignedMesh;
    public Material[] assignedMaterials;

    public CustomizationVariantData(Color color) {
        type = CustomizationType.MaterialColor;
        assignedColor = color;
    }

    public CustomizationVariantData(int index, Mesh mesh, Material[] materials) {
        type = CustomizationType.MeshAndMaterials;
        variantIndex = index;
        assignedMesh = mesh;
        assignedMaterials = materials;
    }
}

public enum CustomizationType { 
    MaterialColor,
    MeshAndMaterials
}