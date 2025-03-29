using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialPanelWithAlignment : SimpleTutorialPanel {

	[Tooltip("What is the alignment of the tooltip. This is used to choose the correct panel based on which alignment is required.")]
	public TutorialPanelAlignment alignment;
	[Tooltip("TextMesh Pro used to display information that the user has to click to continue.")]
	[SerializeField] private TextMeshProUGUI clickToContinue;

	private bool isClickToContinue = false; // whether the panel is displayed and "Click to continue" label is present

	public void ShowPanel(string text, bool showClickToContinue = false) {
		if (showClickToContinue) {
			clickToContinue.text = LocalizationManager.Instance.GetLocalizedString("TutorialLabelClickToContinue");
			clickToContinue.gameObject.SetActive(true);
		} else {
			clickToContinue.gameObject.SetActive(false);
		}
		isClickToContinue = showClickToContinue;
		base.ShowPanel(text);
	}

	public override void HidePanel() {
		isClickToContinue = false;
		base.HidePanel();
	}

	private void Update() {
		// Play sound effect if "Click to continue" is displayed and mouse click occurred
		if (isClickToContinue && Input.GetMouseButtonDown(0)) {
			AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.Click);
		}
	}

}

public enum TutorialPanelAlignment {
	Top,
	Middle,
	Bottom
}
