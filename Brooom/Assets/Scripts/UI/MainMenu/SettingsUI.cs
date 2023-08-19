using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour {

	[SerializeField] Slider mouseSensitivitySlider;
	[SerializeField] Slider musicVolumeSlider;
	[SerializeField] Slider ambienceVolumeSlider;
	[SerializeField] Slider soundEffectsVolumeSlider;

	public static float mouseSensitivity = 3;

	public static float musicVolume = 100;
	public static float ambienceVolume = 100;
	public static float soundEffectsVolume = 100;

	public void ChangeMouseSensitivity(float value) {
		SettingsUI.mouseSensitivity = value;
	}

	public void ChangeMusicVolume(float value) {
		SettingsUI.musicVolume = value;
		// TODO: Change actual music volume
	}

	public void ChangeAmbienceVolume(float value) {
		SettingsUI.ambienceVolume = value;
		// TODO: Change actual ambience volume
	}

	public void ChangeSoundEffectsVolume(float value) {
		SettingsUI.soundEffectsVolume = value;
		// TODO: Change actual sound effects volume
	}

	public void LoadSettingsValues() {
		SettingsSaveData savedValues = SaveSystem.LoadSettings();
		if (savedValues != null) {
			SettingsUI.mouseSensitivity = savedValues.mouseSensitivity;
			SettingsUI.musicVolume = savedValues.musicVolume;
			SettingsUI.ambienceVolume = savedValues.ambienceVolume;
			SettingsUI.soundEffectsVolume = savedValues.soundEffectsVolume;
		}
	}

	private void InitializeValues() {
		mouseSensitivitySlider.value = SettingsUI.mouseSensitivity;
		musicVolumeSlider.value = SettingsUI.musicVolume;
		ambienceVolumeSlider.value = SettingsUI.ambienceVolume;
		soundEffectsVolumeSlider.value = SettingsUI.soundEffectsVolume;
	}

	private void OnEnable() {
		LoadSettingsValues();
		InitializeValues();
	}

	private void OnDisable() {
		// Save settings values
		SaveSystem.SaveSettings(new SettingsSaveData {
			mouseSensitivity = SettingsUI.mouseSensitivity,
			musicVolume = SettingsUI.musicVolume,
			ambienceVolume = SettingsUI.ambienceVolume,
			soundEffectsVolume = SettingsUI.soundEffectsVolume
		});
	}
}
