using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// A component representing a checkbox used in Settings.
/// </summary>
public class SettingsCheckbox : MonoBehaviour {

	[Tooltip("Toggle used to switch between enabled and disabled state.")]
	[SerializeField] Toggle toggle;
	[Tooltip("Image used as a background. Its color is changed based on the current state.")]
	[SerializeField] Image backgroundImage;
	[Tooltip("Color used for the background when the toggle is on.")]
	[SerializeField] Color onColor = Color.white;
	[Tooltip("Color used for the background when the toggle is off.")]
	[SerializeField] Color offColor = Color.white;


	/// <summary>
	/// Changes color of the checkbox background based on the given state (on/off).
	/// Called whenever the state of the checkbox changes.
	/// </summary>
	/// <param name="value">Current state of the checkbox.</param>
	public void OnValueChanged(bool value) {
		SetBackgroundColor(value);
	}

	private void OnEnable() {
		SetBackgroundColor(toggle.isOn);
	}

	// Changes background's color to the one corresponding to the given value (on/off)
	private void SetBackgroundColor(bool value) {
		if (value) backgroundImage.color = onColor;
		else backgroundImage.color = offColor;
	}

}
