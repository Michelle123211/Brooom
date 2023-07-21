using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterCreatorUI : MonoBehaviour
{
    [Header("Character model")]
    [SerializeField] CharacterAppearance character;

    [Header("UI elements")]
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] CustomizationOptionUI optionPrefab;
    [SerializeField] ToggleGroup skinTonesOptions;
    [SerializeField] ToggleGroup hairStyleOptions;
    [SerializeField] ToggleGroup hairColorOptions;
    [SerializeField] ToggleGroup outfitOptions;
    [SerializeField] ToggleGroup shoesOptions;
    [SerializeField] Button continueButton;

    [Header("Parameters")]
    [Tooltip("Name of the file in the StreamingAssets folder containing possible names for the randomization.")]
    public string namesFilename = "names.txt";

    private CharacterCustomizationOptions customizationOptions;
    private CharacterCustomizationData customizationData = new CharacterCustomizationData();

    private string currentName;

    private List<string> possibleNames;

    public void ReturnBack() {
        SceneLoader.Instance.LoadScene(Scene.MainMenu);
    }

    public void StartGame() {
        // Reset the game state
        PlayerState.Instance.ResetState();
        // Save all the choices (including the name)
        CharacterCustomizationData currentCustomization = character.GetCustomizationData();
        currentCustomization.playerName = currentName;
        PlayerState.Instance.CharacterCustomization = currentCustomization;
        // Change scene
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

    public void RandomizeName() {
        // Select a random name from a large list
        if (possibleNames == null || possibleNames.Count == 0)
            possibleNames = NamesManagement.LoadNames(namesFilename);
        currentName = possibleNames[Random.Range(0, possibleNames.Count)];
        nameInputField.text = currentName;
    }

    public void RandomizeCharacter() {
        // For each ToggleGroup activate random child
        ActivateRandomToggleInGroup(skinTonesOptions);
        ActivateRandomToggleInGroup(hairStyleOptions);
        ActivateRandomToggleInGroup(hairColorOptions);
        ActivateRandomToggleInGroup(outfitOptions);
        ActivateRandomToggleInGroup(shoesOptions);
    }

    private void ActivateRandomToggleInGroup(ToggleGroup group) {
        Toggle[] options = group.GetComponentsInChildren<Toggle>();
        options[Random.Range(0, options.Length)].isOn = true;
    }

    private void Start() {
        customizationOptions = PlayerState.Instance.customizationOptions;

        // Reset the character to defaults
        customizationData.InitializeToDefaultValues(customizationOptions);
        character.InitializeFromCustomizationData(customizationData);

        // List all the...
        // ... skin tone options
        FillCustomizationOptions(customizationOptions.skinTones, skinTonesOptions, CustomizedPart.SkinTone);
        // ... hair style options
        FillCustomizationOptions(customizationOptions.hair, hairStyleOptions, CustomizedPart.HairStyle);
        // ... hair color options
        FillCustomizationOptions(customizationOptions.hairColor, hairColorOptions, CustomizedPart.HairColor);
        // ... outfit options
        FillCustomizationOptions(customizationOptions.outfits, outfitOptions, CustomizedPart.Outfit);
        // ... shoes options
        FillCustomizationOptions(customizationOptions.shoes, shoesOptions, CustomizedPart.Shoes);
    }

    private void FillCustomizationOptions(Customization customization, ToggleGroup parentGroup, CustomizedPart customizedPart) {
        Toggle defaultToggle = null;
        int i = 1;
        foreach (var variant in customization.EnumerateVariants()) {
            CustomizationOptionUI option = Instantiate<CustomizationOptionUI>(optionPrefab, parentGroup.transform);
            Toggle toggle = option.GetComponent<Toggle>();
            toggle.group = parentGroup;
            option.Initialize(character, customizedPart, variant, i);
            // The first option is the default one
            if (defaultToggle == null)
                defaultToggle = toggle;
            ++i;
        }
        // Activate the button corresponding to the current outfit
        defaultToggle.isOn = true;
    }
}
