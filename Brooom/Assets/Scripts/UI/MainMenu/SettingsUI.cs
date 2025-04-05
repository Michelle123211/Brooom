using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour {

	[SerializeField] Slider mouseSensitivitySlider;
	[SerializeField] Toggle tutorialToggle;
	[SerializeField] Toggle trainingToggle;
	[SerializeField] AudioVolumeSlider masterVolumeSlider;
	[SerializeField] AudioVolumeSlider musicVolumeSlider;
	[SerializeField] AudioVolumeSlider ambienceVolumeSlider;
	[SerializeField] AudioVolumeSlider soundEffectsVolumeSlider;

	public static float mouseSensitivity = 3;
	public static bool enableTutorial = true;
	public static bool skipTraining = false;

	public void ChangeMouseSensitivity(float value) {
		SettingsUI.mouseSensitivity = value;
	}

	public void SetTutorialEnabled(bool value) {
		SettingsUI.enableTutorial = value;
	}

	public void SetTrainingSkipped(bool value) { 
		SettingsUI.skipTraining = value;
	}

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
		LoadSettingsValues();
		mouseSensitivitySlider.value = SettingsUI.mouseSensitivity;
		tutorialToggle.isOn = SettingsUI.enableTutorial;
		trainingToggle.isOn = SettingsUI.skipTraining;
	}

	private void OnDisable() {
		SaveSettingsValues();
	}
}
