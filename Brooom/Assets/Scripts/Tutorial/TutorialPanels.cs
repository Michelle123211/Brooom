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
		string localizationKey = withPause ? "TutorialLabelEscapeToPause" : "TutorialLabelEscapeToSkip";
		escapePanel.ShowPanel(LocalizationManager.Instance.GetLocalizedString(localizationKey));
	}

	public void HideEscapePanel() { 
		escapePanel.HidePanel();
	}

	public void HideAllTutorialPanels() {
		HideTutorialPanel();
		escapePanel.HidePanel();
	}

}
