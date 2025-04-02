using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsCheckbox : MonoBehaviour {

	[SerializeField] Image backgroundImage;
	[SerializeField] Color onColor = Color.white;
	[SerializeField] Color offColor = Color.white;

	public void OnValueChanged(bool value) {
		if (value) backgroundImage.color = onColor;
		else backgroundImage.color = offColor;
	}

}
