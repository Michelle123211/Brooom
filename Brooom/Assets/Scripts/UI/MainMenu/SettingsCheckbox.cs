using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsCheckbox : MonoBehaviour {

	[SerializeField] Toggle toggle;
	[SerializeField] Image backgroundImage;
	[SerializeField] Color onColor = Color.white;
	[SerializeField] Color offColor = Color.white;

	public void OnValueChanged(bool value) {
		SetBackgroundColor(value);
	}

	private void OnEnable() {
		SetBackgroundColor(toggle.isOn);
	}

	private void SetBackgroundColor(bool value) {
		if (value) backgroundImage.color = onColor;
		else backgroundImage.color = offColor;
	}

}
