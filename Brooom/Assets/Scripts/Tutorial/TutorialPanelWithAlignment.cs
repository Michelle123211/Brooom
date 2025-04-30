using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


/// <summary>
/// A component representing a more complex panel with instructions shown during tutorial.
/// This panel has an alignment asigned, to indicate where on the screen it is located.
/// This may be then used from outside, e.g. to select the panel with the most suitable placement.
/// It can also display an additional line "Click to continue".
/// If this is visible, the panel detects input from the player and plays sound effect if the left mouse button is clicked.
/// </summary>
public class TutorialPanelWithAlignment : SimpleTutorialPanel {

	[Tooltip("What is the alignment of the tooltip. This is used to choose the correct panel based on which alignment is required.")]
	public TutorialPanelAlignment alignment;
	[Tooltip("TextMesh Pro used to display information that the player has to click to continue.")]
	[SerializeField] private TextMeshProUGUI clickToContinue;

	private bool isClickToContinue = false; // whether the panel is displayed and "Click to continue" label is present

	/// <summary>
	/// Sets the content, shows localized information about clicking to continue if required, and shows the tutorial panel.
	/// </summary>
	/// <param name="text">Content of the panel.</param>
	/// <param name="showClickToContinue">Whether the information, that the player has to click to continue, should be displayed.</param>
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

	/// <inheritdoc/>
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

/// <summary>
/// Possible alignments of tutorial panel.
/// </summary>
public enum TutorialPanelAlignment {
	Top,
	Middle,
	Bottom
}
