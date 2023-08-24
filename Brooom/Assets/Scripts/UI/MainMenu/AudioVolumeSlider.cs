using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioVolumeSlider : MonoBehaviour {
	[Tooltip("Path of the VCA whose volume is changed by this slider.")]
	[SerializeField] string VCAPath = "vca:/Master";

	[Tooltip("Corresponding slider component.")]
	[SerializeField] Slider slider;

	private FMOD.Studio.VCA VCAHandle;

	public void ChangeVolume(float value) {
		if (!VCAHandle.hasHandle()) VCAHandle = FMODUnity.RuntimeManager.GetVCA(VCAPath);
		if (VCAHandle.isValid()) VCAHandle.setVolume(value);
	}

	public void SetValue(float value) {
		slider.value = value;
		ChangeVolume(value);
	}

	public float GetValue() {
		return slider.value;
	}

	private void Awake() {
		VCAHandle = FMODUnity.RuntimeManager.GetVCA(VCAPath);
	}
}
