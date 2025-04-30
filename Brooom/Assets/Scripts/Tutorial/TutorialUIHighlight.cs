using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


/// <summary>
/// A component capable of highlighting a particular rectangular part of the screen (based on a <c>Rect</c> or <c>RectTransform</c>),
/// all other parts of the screen are faded out.
/// It is also possible to choose whether raycasts should be blocked in the highlighted area (so it is not possible to click
/// on something through it), or not.
/// </summary>
public class TutorialUIHighlight : MonoBehaviour {

	[Tooltip("RectTransform component of Canvas in which all UI elements for highlighting an area are located.")]
	[SerializeField] RectTransform canvasRectTransform;
	private CanvasScaler canvasScaler; // CanvasScaler component of Canvas in which all UI elements for highlighting an area are located

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


	/// <summary>
	/// Highlights an area of the screen given by the <c>RectTransform</c> with the given padding, while blocking raycasts everywhere around it.
	/// </summary>
	/// <param name="rectTransform"><c>RectTransform</c> specifying an area to be highlighted. If it is <c>null</c>, the whole screen is highlighted (and raycasts may be still blocked).</param>
	/// <param name="blockRaycasts"><c>true</c> if raycasts should be blocked in the highlighted area as well, <c>false</c> otherwise.</param>
	/// <param name="padding">Padding around the <c>RectTransform</c> which will be highlighted as well.</param>
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
		position.x = canvasScaler.scaleFactor * canvasRectTransform.rect.width / 2f + position.x; // works for different aspect ratios
		position.y = canvasScaler.scaleFactor * canvasRectTransform.rect.height / 2f - position.y;

		Highlight(new Rect(position - Vector3.one * padding, rectTransform.rect.size + Vector2.one * padding * 2), blockRaycasts);
	}

	/// <summary>
	/// Highlights an area of the screen given by the <c>Rect</c>, while blocking raycasts everywhere around it.
	/// Coordinates (0, 0) are at the top-left corner and increase in the direction to the bottom-right corner.
	/// </summary>
	/// <param name="rect"><c>Rect</c> specifying an area to be highlighted.</param>
	/// <param name="blockRaycasts"><c>true</c> if raycasts should be blocked in the highlighted area as well, <c>false</c> otherwise.</param>
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

	/// <summary>
	/// Resets everything (size, position, etc.) to stop highlighting and to stop blocking raycasts.
	/// </summary>
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

	// The following three methods may be used from coroutines to be able to wait until they finish

	/// <summary>
	/// Highlights an area of the screen given by the <c>RectTransform</c> with the given padding, while blocking raycasts everywhere around it,
	/// and waits until it is finished (the tween is completed).
	/// </summary>
	/// <param name="rectTransform"><c>RectTransform</c> specifying an area to be highlighted. If it is <c>null</c>, the whole screen is highlighted (and raycasts may be still blocked).</param>
	/// <param name="blockRaycasts"><c>true</c> if raycasts should be blocked in the highlighted area as well, <c>false</c> otherwise.</param>
	/// <param name="padding">Padding around the <c>RectTransform</c> which will be highlighted as well.</param>
	/// <returns>Running over several frames, yielding until some condition is met.</returns>
	public IEnumerator HighlightAndWaitUntilFinished(RectTransform rectTransform, bool blockRaycasts = false, int padding = 0) {
		Highlight(rectTransform, blockRaycasts, padding);
		yield return new WaitForSecondsRealtime(tweenDuration);
	}
	/// <summary>
	/// Highlights an area of the screen given by the <c>Rect</c>, while blocking raycasts everywhere around it,
	/// and waits until it is finished (the tween is completed).
	/// </summary>
	/// <param name="rect"><c>Rect</c> specifying an area to be highlighted.</param>
	/// <param name="blockRaycasts"><c>true</c> if raycasts should be blocked in the highlighted area as well, <c>false</c> otherwise.</param>
	/// <returns>Running over several frames, yielding until some condition is met.</returns>
	public IEnumerator HighlightAndWaitUntilFinished(Rect rect, bool blockRaycasts = false) {
		Highlight(rect, blockRaycasts);
		yield return new WaitForSecondsRealtime(tweenDuration);
	}
	/// <summary>
	/// Resets everything (size, position, etc.) to stop highlighting and to stop blocking raycasts,
	/// and waits until it is finished (the tween is completed).
	/// </summary>
	/// <returns>Running over several frames, yielding until some condition is met.</returns>
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
