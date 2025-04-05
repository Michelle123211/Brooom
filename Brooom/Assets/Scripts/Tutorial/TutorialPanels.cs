using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TutorialPanels : MonoBehaviour {

	[Tooltip("Panels with different alignment used to display main tutorial text (with an option to display information that the user needs to click to continue).")]
	[SerializeField] List<TutorialPanelWithAlignment> panels = new List<TutorialPanelWithAlignment>();
	[Tooltip("Panel in the top-left corner displaying information")]
	[SerializeField] SimpleTutorialPanel escapePanel;

	private bool canBeSkipped = false; // if ESC should skip current tutorial immediately (without pause menu)

	public void ShowTutorialPanel(string text, bool showClickToContinue = false, TutorialPanelAlignment alignment = TutorialPanelAlignment.Bottom) {
		foreach (var panel in panels) {
			// Show only the panel corresponding to the selected alignment, hide all the others
			if (panel.alignment == alignment) {
				panel.ShowPanel(text, showClickToContinue);
			} else {
				panel.HidePanel();
			}
		}
	}

	public void HideTutorialPanel() {
		foreach (var panel in panels) {
			panel.HidePanel();
		}
	}

	public void ShowEscapePanel(bool withPause = true) {
		canBeSkipped = !withPause; // if there is no pause menu, then ESC skips tutorial directly
		string localizationKey = withPause ? "TutorialLabelEscapeToPause" : "TutorialLabelEscapeToSkip";
		escapePanel.ShowPanel(LocalizationManager.Instance.GetLocalizedString(localizationKey));
	}

	public void HideEscapePanel() {
		canBeSkipped = false;
		escapePanel.HidePanel();
	}

	public void HideAllTutorialPanels() {
		HideTutorialPanel();
		escapePanel.HidePanel();
	}

	// The following methods may be used as coroutines to be able to wait until they finish
	private bool wasClick = false;
	public IEnumerator ShowTutorialPanelAndWaitForClick(string text, TutorialPanelAlignment alignment = TutorialPanelAlignment.Bottom) {
		// Show panel
		ShowTutorialPanel(text, true, alignment);
		yield return new WaitForSecondsRealtime(0.3f);
		// Wait for click
		wasClick = false;
		yield return new WaitUntil(() => wasClick);
		// Hide panel
		HideTutorialPanel();
		yield return new WaitForSecondsRealtime(0.3f);
	}
	public IEnumerator ShowTutorialPanelAndWaitUntilVisible(string text, TutorialPanelAlignment alignment = TutorialPanelAlignment.Bottom) {
		ShowTutorialPanel(text, false, alignment);
		yield return new WaitForSecondsRealtime(0.3f);
	}
	public IEnumerator HideTutorialPanelAndWaitUntilInvisible() {
		HideTutorialPanel();
		yield return new WaitForSecondsRealtime(0.3f);
	}

	private void Update() {
		// Detect clicks - necessary for ShowPanelTextAndWaitForClick()
		if (Input.GetMouseButtonDown(0) && (!GamePause.isPauseMenuVisible || GamePause.PauseState == GamePauseState.Running)) {
			// Only if game is running or pause menu is not visible
			wasClick = true;
		}
		// Handle ESC for skipping tutorial
		if (InputManager.Instance.GetBoolValue("Pause") && canBeSkipped) {
			Tutorial.Instance.SkipCurrentTutorialStage();
		}
	}

}
