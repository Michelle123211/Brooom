using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class TooltipBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	[Tooltip("The tooltip is shown with a short delay (in seconds) after the mouse enters the corresponding element.")]
	public float delay = 0.5f;

	[Tooltip("If the tooltip is localized, given texts will be used as keys for localization. Otherwise the text will be used as is.")]
	public bool isLocalized = true;

	private float countdown;
	private bool pointerHasEntered = false;


	protected abstract void PassTextToTooltipController();


	public void OnPointerEnter(PointerEventData eventData) {
		countdown = delay;
		pointerHasEntered = true;
	}

	public void OnPointerExit(PointerEventData eventData) {
		pointerHasEntered = false;
		// Hide tooltip
		TooltipController.Instance.HideTooltip();
	}


	private void Update() {
		// Update the countdown of tooltip delay
		if (pointerHasEntered && countdown > 0) {
			countdown -= Time.deltaTime;
			if (countdown < 0) {
				// Show tooltip
				TooltipController.Instance.SetIsLocalized(isLocalized);
				PassTextToTooltipController();
				TooltipController.Instance.ShowTooltip();
			}
		}
	}
}
