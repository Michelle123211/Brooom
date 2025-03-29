using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SimpleTutorialPanel : MonoBehaviour {

	[Tooltip("TextMesh Pro used to display the main text of the panel.")]
	[SerializeField] protected TextMeshProUGUI content;

	[Tooltip("Whether a sound effect should be played when the panel is opened/closed.")]
	[SerializeField] private bool playSoundEffect = true;

	public virtual void ShowPanel(string text) {
		this.content.text = text;
		gameObject.TweenAwareEnable();
		if (playSoundEffect) AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelOpen);
	}

	public virtual void HidePanel() {
		gameObject.TweenAwareDisable();
		if (playSoundEffect) AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelClose);
	}

}
