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

    private CharacterCustomizationOptions customizationOptions;
    private CharacterCustomizationData customizationData = new CharacterCustomizationData();

    private string currentName;

    public void ReturnBack() {
        SceneLoader.Instance.LoadScene(Scene.MainMenu);
    }

    public void StartGame() {
        // Reset the game state
        PlayerState.Instance.ResetState();
        // Save all the choices (including the name)
        CharacterCustomizationData currentCustomization = character.GetCustomizationData();
        currentCustomization.characterName = currentName;
        PlayerState.Instance.CharacterCustomization = currentCustomization;
        // Set flag that the game was started
        PlayerPrefs.SetString("GameStarted", "true");
        Messaging.SendMessage("GameStarted");
        Analytics.Instance.LogEvent(AnalyticsCategory.Other, $"Character created with name {currentCustomization.characterName}, skin color {currentCustomization.skinColor.assignedColor}, hairstyle {currentCustomization.hairStyle.variantName}, hair color {currentCustomization.hairColor.assignedColor}, outfit {currentCustomization.outfit.variantName} and shoes {currentCustomization.shoes.variantName}.");
        Analytics.Instance.LogEvent(AnalyticsCategory.Game, "New game started after character creation.");
        // Change scene
        if (SettingsUI.enableTutorial) SceneLoader.Instance.LoadScene(Scene.Tutorial);
        else SceneLoader.Instance.LoadScene(Scene.Race); // if tutorial is disabled, go directly to race
    }

    public void NameChanged(string name) {
        currentName = name;
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.KeyDown);
        if (string.IsNullOrEmpty(name)) {
            continueButton.interactable = false;
        } else {
            continueButton.interactable = true;
        }
    }

    public void RandomizeName() {
        // Select a random name from a large list
        currentName = NamesManagement.GetRandomName();
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
