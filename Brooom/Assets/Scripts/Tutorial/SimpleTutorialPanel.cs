using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


/// <summary>
/// A component representing a simple panel with instructions shown during tutorial.
/// </summary>
public class SimpleTutorialPanel : MonoBehaviour {

	[Tooltip("TextMesh Pro used to display the main text of the panel.")]
	[SerializeField] protected TextMeshProUGUI content;

	[Tooltip("Whether a sound effect should be played when the panel is opened.")]
	[SerializeField] private bool playSoundEffectWhenOpened = true;
	[Tooltip("Whether a sound effect should be played when the panel is closed.")]
	[SerializeField] private bool playSoundEffectWhenClosed = true;

	/// <summary>
	/// Sets the content and shows the tutorial panel.
	/// </summary>
	/// <param name="text">Content of the panel.</param>
	public virtual void ShowPanel(string text) {
		this.content.text = text;
		gameObject.TweenAwareEnable();
		if (playSoundEffectWhenOpened) AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelOpen);
	}
	/// <summary>
	/// Hides the tutorial panel.
	/// </summary>
	public virtual void HidePanel() {
		gameObject.TweenAwareDisable();
		if (playSoundEffectWhenClosed) AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelClose);
	}

}
