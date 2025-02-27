using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioVolumeSlider : MonoBehaviour {

	[Tooltip("VCA whose volume is changed by this slider.")]
	[SerializeField] VCA linkedVCA;

	[Tooltip("Corresponding slider component.")]
	[SerializeField] Slider slider;

	[Tooltip("Curve used to convert slider values to volume values (between 0 and 1) which are then set in VCA.")]
	[SerializeField] AnimationCurve volumeConversionCurve;

	public void ChangeVolume(float value) {
		AudioManager.Instance.ChangeVCAVolume(linkedVCA, volumeConversionCurve.Evaluate(value));
	}

	public void SetValue(float value) {
		slider.value = value;
		ChangeVolume(value);
	}

	public float GetValue() {
		return slider.value;
	}
}
