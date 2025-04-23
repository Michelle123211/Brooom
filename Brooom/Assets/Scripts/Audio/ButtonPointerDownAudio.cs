using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/// <summary>
/// A component responsible for playing audio when a <c>Button</c> (to which this component is attached) is clicked.
/// Different sound may be played depending on whether the <c>Button</c> is interactable, or not.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonPointerDownAudio : MonoBehaviour, IPointerDownHandler {

	[Tooltip("This audio event will be played when the button is clicked and it is interactable.")]
	[SerializeField] FMODUnity.EventReference interactableClickEvent;

	[Tooltip("This audio event will be played when the button is clicked and it is not interactable.")]
	[SerializeField] FMODUnity.EventReference noninteractableClickEvent;


	Button button;

	// Plays corresponding sound when the button is clicked
	public void OnPointerDown(PointerEventData eventData) {
		if (button.interactable) {
			if (!interactableClickEvent.IsNull) AudioManager.Instance.PlayOneShot(interactableClickEvent);
		} else {
			if (!noninteractableClickEvent.IsNull) AudioManager.Instance.PlayOneShot(noninteractableClickEvent);
		}
	}

	void Start() {
		button = GetComponent<Button>();
	}
}
