using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


/// <summary>
/// A component representing UI of the Settings screen.
/// It provides methods for applying changes and is responsible for storing these changes persistently.
/// </summary>
public class SettingsUI : MonoBehaviour {

	[Tooltip("Slider for changing mouse sensitivity.")]
	[SerializeField] Slider mouseSensitivitySlider;
	[Tooltip("Checkbox for enabling or disabling tutorial.")]
	[SerializeField] Toggle tutorialToggle;
	[Tooltip("Checkbox for enabling or disabling training before race.")]
	[SerializeField] Toggle trainingToggle;
	[Tooltip("Slider for changing master volume.")]
	[SerializeField] AudioVolumeSlider masterVolumeSlider;
	[Tooltip("Slider for changing music volume.")]
	[SerializeField] AudioVolumeSlider musicVolumeSlider;
	[Tooltip("Slider for changing ambience volume.")]
	[SerializeField] AudioVolumeSlider ambienceVolumeSlider;
	[Tooltip("Slider for changing sound effects volume.")]
	[SerializeField] AudioVolumeSlider soundEffectsVolumeSlider;

	/// <summary>Current mouse sensitivity value set in settings.</summary>
	public static float mouseSensitivity = 3;
	/// <summary>Whether tutorial should be enabled, based on settings.</summary>
	public static bool enableTutorial = true;
	/// <summary>Whether training before race should be skipped, based on settings.</summary>
	public static bool skipTraining = false;

	/// <summary>
	/// Changes current mouse sensitivity value which is used in camera controllers.
	/// Called whenever value of the corresponding slider changes.
	/// </summary>
	/// <param name="value">Current mouse sensitivity value.</param>
	public void ChangeMouseSensitivity(float value) {
		SettingsUI.mouseSensitivity = value;
	}

	/// <summary>
	/// Changes setting saying whether tutorial should be enabled or disabled.
	/// Called whenever value of the corresponding checkbox changes.
	/// </summary>
	/// <param name="value"><c>true</c> if tutorial should be enabled, <c>false</c> if disabled.</param>
	public void SetTutorialEnabled(bool value) {
		SettingsUI.enableTutorial = value;
	}

	/// <summary>
	/// Changes setting saying whether training before race should be skipped or not.
	/// Called whenever value of the corresponding checkbox changes.
	/// </summary>
	/// <param name="value"><c>true</c> if training before race should be skipped, <c>false</c> otherwise.</param>
	public void SetTrainingSkipped(bool value) { 
		SettingsUI.skipTraining = value;
	}

	/// <summary>
	/// Loads current settings' values from a save file.
	/// </summary>
	public void LoadSettingsValues() {
		SettingsSaveData savedValues = SaveSystem.LoadSettings();
		if (savedValues != null) {
			SettingsUI.mouseSensitivity = savedValues.mouseSensitivity;
			SettingsUI.enableTutorial = savedValues.enableTutorial;
			SettingsUI.skipTraining = savedValues.skipTraining;
			masterVolumeSlider.SetValue(savedValues.masterVolume);
			musicVolumeSlider.SetValue(savedValues.musicVolume);
			ambienceVolumeSlider.SetValue(savedValues.ambienceVolume);
			soundEffectsVolumeSlider.SetValue(savedValues.soundEffectsVolume);
		}
	}

	/// <summary>
	/// Saves current settings' values into a save file.
	/// </summary>
	public void SaveSettingsValues() {
		SaveSystem.SaveSettings(new SettingsSaveData {
			mouseSensitivity = SettingsUI.mouseSensitivity,
			enableTutorial = enableTutorial,
			skipTraining = skipTraining,
			masterVolume = masterVolumeSlider.GetValue(),
			musicVolume = musicVolumeSlider.GetValue(),
			ambienceVolume = ambienceVolumeSlider.GetValue(),
			soundEffectsVolume = soundEffectsVolumeSlider.GetValue()
		});
	}

	private void OnEnable() {
		// Initialize values of UI elements
		LoadSettingsValues();
		mouseSensitivitySlider.value = SettingsUI.mouseSensitivity;
		tutorialToggle.isOn = SettingsUI.enableTutorial;
		trainingToggle.isOn = SettingsUI.skipTraining;
		Analytics.Instance.LogEvent(AnalyticsCategory.Game, "Settings opened.");
	}

	private void OnDisable() {
		// Save changes
		SaveSettingsValues();
		Analytics.Instance.LogEvent(AnalyticsCategory.Game, $"Settings closed with the following values: mouse sensitivity {mouseSensitivity}, enable tutorial {enableTutorial}, skip training {skipTraining}, master volume {masterVolumeSlider.GetValue()}, music volume {musicVolumeSlider.GetValue()}, ambience volume {ambienceVolumeSlider.GetValue()}, SFX volume {soundEffectsVolumeSlider.GetValue()}.");
	}
}
