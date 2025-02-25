using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioVolumeSlider : MonoBehaviour {

	[Tooltip("VCA whose volume is changed by this slider.")]
	[SerializeField] VCA linkedVCA;

	[Tooltip("Corresponding slider component.")]
	[SerializeField] Slider slider;

	public void ChangeVolume(float value) {
		AudioManager.Instance.ChangeVCAVolume(linkedVCA, value);
	}

	public void SetValue(float value) {
		slider.value = value;
		ChangeVolume(value);
	}

	public float GetValue() {
		return slider.value;
	}
}
