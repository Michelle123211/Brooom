using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// A base component for a tooltip displayed on the screen on mouse hover.
/// It uses <c>TooltipController</c> to set content of the tooltip and to display/hide it 
/// (there is only one tooltip object in the scene, controlled by <c>TooltipController</c>, which is reused).
/// </summary>
public abstract class TooltipBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	[Tooltip("The tooltip is shown with a short delay (in seconds) after the mouse enters the corresponding element.")]
	public float delay = 0.5f;

	[Tooltip("If the tooltip is localized, given texts will be used as keys for localization. Otherwise the text will be used as is.")]
	public bool isLocalized = true;

	private float countdown; // remaining time until the tooltip appears
	private bool pointerHasEntered = false;
	private bool isTooltipDisplayed = false;


	/// <summary>
	/// Sets tooltip's content using the <c>TooltipController</c>.
	/// </summary>
	protected abstract void PassTextToTooltipController();

	/// <summary>
	/// Starts countdown before a tooltip is displayed (because it is displayed after a short delay).
	/// Invoked when mouse enters the corresponding element.
	/// </summary>
	public void OnPointerEnter(PointerEventData eventData) {
		countdown = delay;
		pointerHasEntered = true;
	}

	/// <summary>
	/// Stops countdown before a tooltip is displayed, and hides the tooltip if it is displayed already.
	/// Invoked when mouse exits the corresponding element.
	/// </summary>
	public void OnPointerExit(PointerEventData eventData) {
		pointerHasEntered = false;
		// Hide tooltip
		TooltipController.Instance.HideTooltip();
		isTooltipDisplayed = false;
	}

	// Updates countdown before a tooltip is displayed, if mouse is over the element, shows the tooltip when the time is up
	private void Update() {
		// Update the countdown of tooltip delay
		if (pointerHasEntered && countdown >= 0) {
			countdown -= Time.unscaledDeltaTime;
			if (countdown < 0 && !isTooltipDisplayed) {
				// Show tooltip
				TooltipController.Instance.SetIsLocalized(isLocalized);
				PassTextToTooltipController();
				TooltipController.Instance.ShowTooltip();
				isTooltipDisplayed = true;
			}
		}
	}

	private void OnDestroy() {
		if (pointerHasEntered) TooltipController.Instance.HideTooltip();
	}

	private void OnDisable() {
		if (pointerHasEntered) TooltipController.Instance.HideTooltip();
	}
}
