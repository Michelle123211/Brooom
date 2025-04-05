using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialUIHighlight : MonoBehaviour {

	[Tooltip("CanvasScaler component of Canvas in which all UI elements for highlighting an area are located.")]
	[SerializeField] RectTransform canvasRectTransform;
	private CanvasScaler canvasScaler;

	[Header("Parameters")]
	[Tooltip("How long it takes to highlight something (move highlight there and adjust size of the highlight).")]
	[SerializeField] float tweenDuration = 0.3f;

	[Header("UI elements for highlight")]
	[Tooltip("An Image used to cutout a rectangle out of fadeout overlay to highlight an area.")]
	[SerializeField] RectTransform highlightMask;
	[Tooltip("An InverseImageMask used as a fadeout overlay from which a rectangle is cutout to highlight an area.")]
	[SerializeField] InverseImageMask highlightFade;

	[Header("UI elements for blocking raycasts")]
	[Tooltip("Image blocking raycasts to the left of the highlighted area.")]
	[SerializeField] RectTransform left;
	[Tooltip("Image blocking raycasts to the right of the highlighted area.")]
	[SerializeField] RectTransform right;
	[Tooltip("Image blocking raycasts above the highlighted area.")]
	[SerializeField] RectTransform top;
	[Tooltip("Image blocking raycasts below the highlighted area.")]
	[SerializeField] RectTransform bottom;

	// Whether area around the highlighted one should fade out to black next time we highlight something (i.e. nothing is highlighted right now)
	private bool shouldFadeOut = true;

	// Highlights area given by RectTransform, blocks raycasts everywhere around it
	// If blockRaycasts is true, raycasts in the highlighted area will be blocked as well
	public void Highlight(RectTransform rectTransform, bool blockRaycasts = false, int padding = 0) {
		// If rectTransform is null, stop highlighting but consider blocking raycasts
		if (rectTransform == null) {
			StopHighlighting();
			highlightFade.raycastTarget = blockRaycasts;
			return;
		}
		// Get corners of RectTransform
		Vector3[] corners = new Vector3[4];
		rectTransform.GetWorldCorners(corners);
		// Get coordinates of top-left corner in screen space
		Vector3 position = canvasRectTransform.InverseTransformPoint(corners[1]);
		position.x = canvasScaler.scaleFactor * canvasRectTransform.rect.width / 2f + position.x; // works for different aspect ratio
		position.y = canvasScaler.scaleFactor * canvasRectTransform.rect.height / 2f - position.y;

		Highlight(new Rect(position - Vector3.one * padding, rectTransform.rect.size + Vector2.one * padding * 2), blockRaycasts);
	}

	// Highlights area given by Rect, blocks raycasts everywhere around it
	// Coordinates (0, 0) are at the top-left corner and increase in the direction to bottom-right corner
	// If blockRaycasts is true, raycasts in the highlighted area will be blocked as well
	public void Highlight(Rect rect, bool blockRaycasts = false) {
		// Block raycasts anywhere around the highlighted area
		left.sizeDelta = left.sizeDelta.WithX(rect.x);
		right.sizeDelta = right.sizeDelta.WithX(canvasScaler.referenceResolution.x - rect.x - rect.width);
		top.sizeDelta = top.sizeDelta.WithY(rect.y);
		bottom.sizeDelta = bottom.sizeDelta.WithY(canvasScaler.referenceResolution.y - rect.y - rect.height);
		// Set the highlighted area
		highlightFade.raycastTarget = blockRaycasts; // if necessary, block raycasts also in the highlighted area
		highlightMask.DOSizeDelta(new Vector2(rect.width, rect.height), tweenDuration).SetUpdate(true);
		highlightMask.DOAnchorPos(new Vector2(rect.x, -rect.y), tweenDuration).SetUpdate(true);
		// If this is the first highlight (it was stopped before or did not even start), then tween fade color as well
		if (shouldFadeOut) {
			highlightFade.color = Color.black.WithA(0f);
			highlightFade.DOColor(Color.black.WithA(0.95f), tweenDuration).SetUpdate(true);
			shouldFadeOut = false;
		}
	}

	public void StopHighlighting() {
		// Reset everything (size, position etc.)
		left.sizeDelta = left.sizeDelta.WithX(0);
		right.sizeDelta = right.sizeDelta.WithX(0);
		top.sizeDelta = top.sizeDelta.WithY(0);
		bottom.sizeDelta = bottom.sizeDelta.WithY(0);
		highlightFade.DOColor(Color.black.WithA(0f), tweenDuration).SetUpdate(true);
		highlightFade.raycastTarget = false;
		highlightMask.DOSizeDelta(new Vector2(canvasScaler.referenceResolution.x, canvasScaler.referenceResolution.y), tweenDuration).SetUpdate(true);
		highlightMask.DOAnchorPos(Vector2.zero, tweenDuration).SetUpdate(true);
		// Note down that next time we should fade out to black again
		shouldFadeOut = true;
	}

	// The following methods may be used as coroutines to be able to wait until they finish
	public IEnumerator HighlightAndWaitUntilFinished(RectTransform rectTransform, bool blockRaycasts = false, int padding = 0) {
		Highlight(rectTransform, blockRaycasts, padding);
		yield return new WaitForSecondsRealtime(tweenDuration);
	}
	public IEnumerator HighlightAndWaitUntilFinished(Rect rect, bool blockRaycasts = false) {
		Highlight(rect, blockRaycasts);
		yield return new WaitForSecondsRealtime(tweenDuration);
	}
	public IEnumerator StopHighlightingAndWaitUntilFinished() {
		StopHighlighting();
		yield return new WaitForSecondsRealtime(tweenDuration);
	}

	private void OnSceneLoaded(Scene scene) {
		// Sometimes, after loading a scene, the fade is not visible for some reason, even though functionally it is there
		// - Changing the sprite helped every time so hopefully this will fix it
		Sprite fadeSprite = highlightFade.sprite;
		highlightFade.sprite = null;
		highlightFade.sprite = fadeSprite;
	}

	private void Start() {
		this.canvasScaler = canvasRectTransform.GetComponent<CanvasScaler>();
		highlightFade.color = Color.black.WithA(0f);
		SceneLoader.Instance.onSceneLoaded += OnSceneLoaded;
	}

}
