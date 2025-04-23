using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// A component managing the whole Character Creator screen with all of its UI elements.
/// </summary>
public class CharacterCreatorUI : MonoBehaviour {

	[Header("Character model")]
	[Tooltip("Character placed in the scene which is used to preview selected customization options.")]
	[SerializeField] CharacterAppearance character;

	[Header("UI elements")]
	[Tooltip("Input field for character's name.")]
	[SerializeField] TMP_InputField nameInputField;
	[Tooltip("Prefab of a single selectable customization option.")]
	[SerializeField] CustomizationOptionUI optionPrefab;
	[Tooltip("A ToggleGroup containing all skin tones options.")]
	[SerializeField] ToggleGroup skinTonesOptions;
	[Tooltip("A ToggleGroup containing all hairstyle options.")]
	[SerializeField] ToggleGroup hairStyleOptions;
	[Tooltip("A ToggleGroup containing all hair color options.")]
	[SerializeField] ToggleGroup hairColorOptions;
	[Tooltip("A ToggleGroup containing all outfit options.")]
	[SerializeField] ToggleGroup outfitOptions;
	[Tooltip("A ToggleGroup containing all shoes options.")]
	[SerializeField] ToggleGroup shoesOptions;
	[Tooltip("Button used to confirm the customization and proceed further.")]
	[SerializeField] Button continueButton;

	private CharacterCustomizationOptions customizationOptions; // all available customization options
	private CharacterCustomizationData customizationData = new CharacterCustomizationData(); // current character appearance (with selected variants)

	private string currentName;

	/// <summary>
	/// Loads Main Menu scene.
	/// </summary>
	public void ReturnBack() {
		SceneLoader.Instance.LoadScene(Scene.MainMenu);
	}

	/// <summary>
	/// Starts a new game while saving all the choices (character name, selected appearance).
	/// </summary>
	public void StartGame() {
		// Reset the game state
		SaveSystem.DeleteBackup();
		PlayerState.Instance.ResetState();
		// Save all the choices (including the name)
		CharacterCustomizationData currentCustomization = character.GetCustomizationData();
		currentCustomization.characterName = currentName;
		PlayerState.Instance.CharacterCustomization = currentCustomization;
		// Set flag that the game was started
		Messaging.SendMessage("GameStarted");
		Analytics.Instance.LogEvent(AnalyticsCategory.Other, $"Character created with name {currentCustomization.characterName}, skin color {currentCustomization.skinColor.assignedColor}, hairstyle {currentCustomization.hairStyle.variantName}, hair color {currentCustomization.hairColor.assignedColor}, outfit {currentCustomization.outfit.variantName} and shoes {currentCustomization.shoes.variantName}.");
		Analytics.Instance.LogEvent(AnalyticsCategory.Game, "New game started after character creation.");
		// Change scene
		if (SettingsUI.enableTutorial) SceneLoader.Instance.LoadScene(Scene.Tutorial);
		else SceneLoader.Instance.LoadScene(Scene.Race); // if tutorial is disabled, go directly to race
	}

	/// <summary>
	/// This method is invoked when value in name input field changes.
	/// It enables the Continue button only if the name is not empty.
	/// </summary>
	/// <param name="name"></param>
	public void NameChanged(string name) {
		currentName = name;
		AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.KeyDown);
		if (string.IsNullOrEmpty(name)) {
			continueButton.interactable = false;
		} else {
			continueButton.interactable = true;
		}
	}

	/// <summary>
	/// Selects a random name from a large list.
	/// </summary>
	public void RandomizeName() {
		currentName = NamesManagement.GetRandomName();
		nameInputField.text = currentName;
	}

	/// <summary>
	/// Selects a random character appearance.
	/// </summary>
	public void RandomizeCharacter() {
		// For each ToggleGroup activate random child
		ActivateRandomToggleInGroup(skinTonesOptions);
		ActivateRandomToggleInGroup(hairStyleOptions);
		ActivateRandomToggleInGroup(hairColorOptions);
		ActivateRandomToggleInGroup(outfitOptions);
		ActivateRandomToggleInGroup(shoesOptions);
	}

	// Selects a random child of the given ToggleGroup and activates it
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

	// Instantiates options for all customization variants of the given part
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
		// Activate the option corresponding to the default value
		defaultToggle.isOn = true;
	}

}
