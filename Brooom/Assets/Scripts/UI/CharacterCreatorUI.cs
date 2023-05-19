using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreatorUI : MonoBehaviour
{
    [Header("Character customization options")]
    [SerializeField] CharacterCustomizationOptions customizationOptions;
    [Header("Character model parts")]
    [SerializeField] Material skinMaterial;
    [SerializeField] SkinnedMeshRenderer outfitRenderer;

    [Header("UI elements")]
    [SerializeField] CustomizationOptionUI optionPrefab;
    [SerializeField] ToggleGroup skinTonesOptions;
    [SerializeField] ToggleGroup outfitOptions;
    [SerializeField] Button continueButton;

    private string currentName;

    public void ReturnBack() {
        SceneLoader.Instance.LoadScene(Scene.MainMenu);
    }

    public void StartGame() {
        // TODO: Save all the choices (including the name)
        SceneLoader.Instance.LoadScene(Scene.Tutorial);
    }

    public void NameChanged(string name) {
        currentName = name;
        if (string.IsNullOrEmpty(name)) {
            continueButton.interactable = false;
        } else {
            continueButton.interactable = true;
        }
    }

	private void OnEnable() {
        // List all the skin tone options
        FillCustomizationOptions(customizationOptions.skinTones, skinTonesOptions, skinMaterial);
        // List all the outfit options
        FillCustomizationOptions(customizationOptions.outfits, outfitOptions, outfitRenderer);
    }

    private void FillCustomizationOptions(Customization customization, ToggleGroup parentGroup, Material changedMaterial) {
        FillCustomizationOptions(customization, parentGroup, changedMaterial, null);
    }
    private void FillCustomizationOptions(Customization customization, ToggleGroup parentGroup, SkinnedMeshRenderer changedRenderer) {
        FillCustomizationOptions(customization, parentGroup, null, changedRenderer);
    }
    private void FillCustomizationOptions(Customization customization, ToggleGroup parentGroup, Material changedMaterial, SkinnedMeshRenderer changedRenderer) {
        Toggle defaultToggle = null;
        foreach (var variant in customization.EnumerateVariants()) {
            CustomizationOptionUI option = Instantiate<CustomizationOptionUI>(optionPrefab, parentGroup.transform);
            Toggle toggle = option.GetComponent<Toggle>();
            toggle.group = parentGroup;
            if (changedMaterial != null)
                option.Initialize(changedMaterial, variant);
            else
                option.Initialize(changedRenderer, variant);
            // TODO: Change to loading saved state
            if (defaultToggle == null)
                defaultToggle = toggle;
        }
        // Activate the button corresponding to the current outfit
        defaultToggle.isOn = true;
    }
}
