using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class ButtonPointerDownAudio : MonoBehaviour, IPointerDownHandler {

	[Tooltip("This audio event will be played when the button is clicked and it is interactable.")]
	[SerializeField] FMODUnity.EventReference interactableClickEvent;

	[Tooltip("This audio event will be played when the button is clicked and it is not interactable.")]
	[SerializeField] FMODUnity.EventReference noninteractableClickEvent;


	Button button;

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
