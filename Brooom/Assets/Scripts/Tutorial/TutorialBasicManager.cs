using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A singleton handling basic player and camera manipulations useful during tutorial.
/// </summary>
public class TutorialBasicManager : MonoBehaviour {

	// Just a simple singleton
	/// <summary>Singleton instance.</summary>
	public static TutorialBasicManager Instance { get; private set; }

	[Tooltip("The player object in the scene.")]
	public GameObject player;

	[Tooltip("Virtual camera which is moved around and rotated to focus on a specific object as part of the tutorial.")]
	public TutorialCamera cutsceneCamera;

	// Initial player position and rotation - stored so it is possible to reset it
	private Vector3 initialPlayerPosition;
	private Vector3 initialPlayerRotation;


	/// <summary>
	/// Resets the player's position and rotation to initial values.
	/// </summary>
	public void ResetPlayerPositionAndRotation() {
		player.transform.position = initialPlayerPosition;
		player.transform.eulerAngles = initialPlayerRotation;
	}

	/// <summary>
	/// Changes the player's position to the given value.
	/// </summary>
	/// <param name="position">New player's position.</param>
	public void MovePlayerTo(Vector3 position) {
		player.transform.position = position;
	}

	/// <summary>
	/// Rotates the player around the Y axis towards the given target.
	/// </summary>
	/// <param name="target">Target to rotate towards.</param>
	public void RotatePlayerTowards(Transform target) {
		// Only rotation around Y
		Vector3 lookDirection = target.position - player.transform.position;
		Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, lookDirection);
		player.transform.eulerAngles = player.transform.eulerAngles.WithY(rotation.eulerAngles.y);
	}

	/// <summary>
	/// Completely disables the player's actions (movement, spell casting, and also looking around if requested).
	/// </summary>
	/// <param name="includingLookingAround"><c>true</c> if looking around should be disabled as well, <c>false</c> otherwise.</param>
	public void DisablePlayerActions(bool includingLookingAround = true) {
		player.GetComponent<CharacterMovementController>().DisableActions(CharacterMovementController.StopMethod.ImmediateStop);
		player.GetComponentInChildren<SpellInput>().DisableSpellCasting();
		if (includingLookingAround) player.GetComponent<PlayerCameraController>().DisableRotation();
	}
	/// <summary>
	/// Enables all the player's actions (movement, spell casting, and also looking around if requested).
	/// </summary>
	/// <param name="includingLookingAround"><c>true</c> if looking around should be enabled as well, <c>false</c> otherwise.</param>
	public void EnablePlayerActions(bool includingLookingAround = true) {
		player.GetComponent<CharacterMovementController>().EnableActions();
		player.GetComponentInChildren<SpellInput>().TryEnableSpellCasting();
		if (includingLookingAround) player.GetComponent<PlayerCameraController>().EnableRotation();
	}

	/// <summary>
	/// Disables looking around for the player.
	/// </summary>
	public void DisableLookingAround() {
		player.GetComponent<PlayerCameraController>().DisableRotation();
	}
	/// <summary>
	/// Enables looking around for the player.
	/// </summary>
	public void EnableLookingAround() {
		player.GetComponent<PlayerCameraController>().EnableRotation();
	}

	private void Awake() {
		// Set singleton instance
		Instance = this;
	}

	private void Start() {
		// Store initial player position and rotation
		this.initialPlayerPosition = player.transform.position;
		this.initialPlayerRotation = player.transform.eulerAngles;
	}

	private void OnDestroy() {
		// Reset singleton instance
		Instance = null;
	}

}
