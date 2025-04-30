using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component making sure that Main Menu content is expanding to use the whole height available,
/// i.e. UI elements, which should be in corners (e.g., exit button, settings button), stay in corners even with different aspect ratios.
/// Width is taken care of automatically thanks to <c>RectTransform</c>'s size and <c>CanvasScaler</c> component.
/// </summary>
[ExecuteInEditMode]
public class MainMenuContentHeight : MonoBehaviour {

	private RectTransform rectTransform;

	private void Update() {
		// Modify RectTransform's height to fill the whole space available
		RectTransform parentRectTransform = transform.parent.GetComponent<RectTransform>();
		float height = parentRectTransform.rect.height;
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
	}

	private void Awake() {
		this.rectTransform = GetComponent<RectTransform>();
	}

}
