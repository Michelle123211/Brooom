using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;


// A base class for HUD outside of Race scene (e.g. for Tutorial or Testing Track)
// Displaying only speed, altitude and spell slots
public class SimplifiedHUD : MonoBehaviour {

	[Header("Basic info")]
	[SerializeField] TextMeshProUGUI speedText;
	[SerializeField] TextMeshProUGUI altitudeText;

	[Header("Spells")]
	[SerializeField] protected RaceSpellsUI spellsUI;

	protected CharacterMovementController playerMovementController;

	#region Speed and altitude
	public void UpdatePlayerState() {
		speedText.text = Math.Round(playerMovementController.GetCurrentSpeed(), 1).ToString();
		altitudeText.text = Math.Round(playerMovementController.GetCurrentAltitude(), 1).ToString();
	}
	#endregion

	protected virtual void Start_Derived() { }
	protected virtual void Update_Derived() { }

	private void Start() {
		// Initialize data fields
		playerMovementController = UtilsMonoBehaviour.FindObjectOfTypeAndTag<CharacterMovementController>("Player");
		// Initialize and show spells UI
		spellsUI.Initialize(playerMovementController.gameObject);

		Start_Derived();
	}

	private void Update() {
		UpdatePlayerState();
		Update_Derived();
	}

}