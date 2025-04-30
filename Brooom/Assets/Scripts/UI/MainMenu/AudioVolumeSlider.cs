using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// A component representing a slider used in Settings to change volume of a particular VCA.
/// </summary>
public class AudioVolumeSlider : MonoBehaviour {

	[Tooltip("VCA whose volume is changed by this slider.")]
	[SerializeField] VCA linkedVCA;

	[Tooltip("Corresponding slider component.")]
	[SerializeField] Slider slider;

	[Tooltip("Curve used to convert slider values to volume values (between 0 and 1) which are then set in VCA.")]
	[SerializeField] AnimationCurve volumeConversionCurve;

	/// <summary>
	/// Sets the VCA's volume to a value from <c>volumeConversionCurve</c> corresponding to the given value.
	/// It is called whenever the audio slider's value changes.
	/// </summary>
	/// <param name="value">Number between 0 and 1 corresponding to the volume level.</param>
	public void ChangeVolume(float value) {
		AudioManager.Instance.ChangeVCAVolume(linkedVCA, volumeConversionCurve.Evaluate(value));
	}

	/// <summary>
	/// Sets the slider's value to the given value and immediately changes the corresponding VCA's volume accordingly.
	/// </summary>
	/// <param name="value">Number between 0 and 1.</param>
	public void SetValue(float value) {
		slider.value = value;
		ChangeVolume(value);
	}

	/// <summary>
	/// Gets the current value of the slider.
	/// </summary>
	/// <returns>Slider's current value.</returns>
	public float GetValue() {
		return slider.value;
	}
}
