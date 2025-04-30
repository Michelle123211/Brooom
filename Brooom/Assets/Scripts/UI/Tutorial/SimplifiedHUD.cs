using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;


/// <summary>
/// A base class for representing a very basic HUD which can be used outside of Race scene (e.g., in Tutorial or Testing Track).
/// Compared to <c>RaceUI</c>, it displays only current speed, current altitude and equipped spell slots, 
/// and updates them automatically (there is no need to call update methods externally).
/// Derived classes may add some more functionality.
/// </summary>
public class SimplifiedHUD : MonoBehaviour {

	[Header("Basic info")]
	[Tooltip("A label displaying current speed.")]
	[SerializeField] TextMeshProUGUI speedText;
	[Tooltip("A label displaying current altitude.")]
	[SerializeField] TextMeshProUGUI altitudeText;

	[Header("Spells")]
	[Tooltip("HUD part responsible for displaying slots with equipped spells in the race.")]
	[SerializeField] protected RaceSpellsUI spellsUI;

	/// <summary><c>CharacterMovementController</c> of the player, used to obtain current speed and altitude.</summary>
	protected CharacterMovementController playerMovementController;

	#region Speed and altitude
	/// <summary>
	/// Updates the current speed and altitude values which are displayed in the HUD based on values obtained from the player's <c>CharacterMovementController</c>.
	/// </summary>
	public void UpdatePlayerState() {
		speedText.text = Math.Round(playerMovementController.GetCurrentSpeed(), 1).ToString();
		altitudeText.text = Math.Round(playerMovementController.GetCurrentAltitude(), 1).ToString();
	}
	#endregion

	/// <summary>
	/// This method replaces the standard <c>Start()</c> method (and is called from the inherited <c>Start()</c>) 
	/// to provide the derived classes an opportunity to perform their own initialization.
	/// </summary>
	protected virtual void Start_Derived() { }
	/// <summary>
	/// This method replaces the standard <c>Update()</c> method (and is called from the inherited <c>Update()</c>) 
	/// to provide the derived classes an opportunity to perform their own update.
	/// </summary>
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