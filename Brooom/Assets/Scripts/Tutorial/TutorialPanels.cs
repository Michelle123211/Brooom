using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


/// <summary>
/// A component providing methods to show two different types of panels with instructions during tutorial.
/// First panel is <c>SimpleTutorialPanel</c> displaying information in the top-left corner about using ESC to skip the tutorial
/// (the input is also detected and handled here) or to pause the game (handled in <c>GamePause</c>).
/// Second panel is <c>TutorialPanelWithAlignment</c> displaying usual instructions with an option to show information about clicking to continue 
/// and with possibility to choose the panel's placement.
/// It also provides means of waiting until a tutorial panel is (in)visible or until a click occurs.
/// </summary>
public class TutorialPanels : MonoBehaviour {

	[Tooltip("Panels with different alignment used to display main tutorial text (with an option to display information that the player needs to click to continue).")]
	[SerializeField] List<TutorialPanelWithAlignment> panels = new List<TutorialPanelWithAlignment>();
	[Tooltip("Panel in the top-left corner displaying information about using ESC to to skip tutorial or to pause the game.")]
	[SerializeField] SimpleTutorialPanel escapePanel;

	private bool canBeSkipped = false; // if ESC should skip current tutorial immediately (without pause menu), or should show pause menu as usual

	/// <summary>
	/// Shows only the panel corresponding to the selected alignment (hides all others), displaying the given text and an information
	/// about clicking to continue, if requested.
	/// </summary>
	/// <param name="text">Content of the panel.</param>
	/// <param name="showClickToContinue">Whether the information, that the player has to click to continue, should be displayed.</param>
	/// <param name="alignment">Selected alignment of the panel.</param>
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

	/// <summary>
	/// Hides all tutorial panels, except the panel with information about using ESC.
	/// </summary>
	public void HideTutorialPanel() {
		foreach (var panel in panels) {
			panel.HidePanel();
		}
	}

	/// <summary>
	/// Shows a panel in the top-left corner displaying information about the option to use ESC.
	/// </summary>
	/// <param name="withPause"><c>true</c> if ESC should show pause menu, <c>false</c> if it should skip the current tutorial immediately.</param>
	public void ShowEscapePanel(bool withPause = true) {
		canBeSkipped = !withPause; // if there is no pause menu, then ESC skips tutorial directly
		string localizationKey = withPause ? "TutorialLabelEscapeToPause" : "TutorialLabelEscapeToSkip";
		escapePanel.ShowPanel(LocalizationManager.Instance.GetLocalizedString(localizationKey));
	}

	/// <summary>
	/// Hides a panelwith information about using ESC.
	/// </summary>
	public void HideEscapePanel() {
		canBeSkipped = false;
		escapePanel.HidePanel();
	}

	/// <summary>
	/// Hides all tutorial panels.
	/// </summary>
	public void HideAllTutorialPanels() {
		HideTutorialPanel();
		HideEscapePanel();
	}

	// The following three methods may be used from coroutines to be able to wait until they finish

	private bool wasClick = false;
	/// <summary>
	/// Shows a tutorial panel with the given text, the selected alignment and an information about clicking to continue. 
	/// Then waits until a click occurs, hides the panel and finishes.
	/// </summary>
	/// <param name="text">Content of the panel.</param>
	/// <param name="alignment">Selected alignment of the panel.</param>
	/// <returns>Running over several frames, yielding until some condition is met.</returns>
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
	/// <summary>
	/// Shows a tutorial panel with the given text, the selected alignment and without an information about clicking to continue.
	/// Then waits until the panel is fully visible (tween is completed) and finishes.
	/// </summary>
	/// <param name="text">Content of the panel.</param>
	/// <param name="alignment">Selected alignment of the panel.</param>
	/// <returns>Running over several frames, yielding until some condition is met.</returns>
	public IEnumerator ShowTutorialPanelAndWaitUntilVisible(string text, TutorialPanelAlignment alignment = TutorialPanelAlignment.Bottom) {
		ShowTutorialPanel(text, false, alignment);
		yield return new WaitForSecondsRealtime(0.3f);
	}
	/// <summary>
	/// Hides a tutorial panel, waits until it is hidden completely (tween is completed) and finishes.
	/// </summary>
	/// <returns>Running over several frames, yielding until some condition is met.</returns>
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
		if (InputManager.Instance.GetBoolValue("Pause") && canBeSkipped && Tutorial.Instance.IsInProgress()) {
			canBeSkipped = false;
			Tutorial.Instance.SkipCurrentTutorialStage();
		}
	}

}
