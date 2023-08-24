using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour {

	[SerializeField] Slider mouseSensitivitySlider;
	[SerializeField] AudioVolumeSlider masterVolumeSlider;
	[SerializeField] AudioVolumeSlider musicVolumeSlider;
	[SerializeField] AudioVolumeSlider ambienceVolumeSlider;
	[SerializeField] AudioVolumeSlider soundEffectsVolumeSlider;

	public static float mouseSensitivity = 3;

	public void ChangeMouseSensitivity(float value) {
		SettingsUI.mouseSensitivity = value;
	}

	public void LoadSettingsValues() {
		SettingsSaveData savedValues = SaveSystem.LoadSettings();
		if (savedValues != null) {
			SettingsUI.mouseSensitivity = savedValues.mouseSensitivity;
			masterVolumeSlider.SetValue(savedValues.masterVolume);
			musicVolumeSlider.SetValue(savedValues.musicVolume);
			ambienceVolumeSlider.SetValue(savedValues.ambienceVolume);
			soundEffectsVolumeSlider.SetValue(savedValues.soundEffectsVolume);
		}
	}

	private void OnEnable() {
		LoadSettingsValues();
		mouseSensitivitySlider.value = SettingsUI.mouseSensitivity;
	}

	private void OnDisable() {
		// Save settings values
		SaveSystem.SaveSettings(new SettingsSaveData {
			mouseSensitivity = SettingsUI.mouseSensitivity,
			masterVolume = masterVolumeSlider.GetValue(),
			musicVolume = musicVolumeSlider.GetValue(),
			ambienceVolume = ambienceVolumeSlider.GetValue(),
			soundEffectsVolume = soundEffectsVolumeSlider.GetValue()
		});
	}
}
