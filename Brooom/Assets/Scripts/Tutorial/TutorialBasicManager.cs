using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// A component handling basic player and camera manipulations
public class TutorialBasicManager : MonoBehaviour {

	// Just a simple singleton
	public static TutorialBasicManager Instance { get; private set; }

	public GameObject player;

	[Tooltip("Virtual camera which is moved around and rotated to focus on a specific object as part of the tutorial.")]
	public TutorialCamera cutsceneCamera;

	private Vector3 initialPlayerPosition;
	private Vector3 initialPlayerRotation;


	public void ResetPlayerPositionAndRotation() {
		player.transform.position = initialPlayerPosition;
		player.transform.eulerAngles = initialPlayerRotation;
	}

	public void MovePlayerTo(Vector3 position) {
		player.transform.position = position;
	}

	public void RotatePlayerTowards(Transform target) {
		// Only rotation around Y
		Vector3 lookDirection = target.position - player.transform.position;
		Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, lookDirection);
		player.transform.eulerAngles = player.transform.eulerAngles.WithY(rotation.eulerAngles.y);
	}

	public void DisablePlayerActions(bool includingLookingAround = true) {
		player.GetComponent<CharacterMovementController>().DisableActions(CharacterMovementController.StopMethod.ImmediateStop);
		player.GetComponentInChildren<SpellInput>().DisableSpellCasting();
		if (includingLookingAround) player.GetComponent<PlayerCameraController>().DisableRotation();
	}
	public void EnablePlayerActions(bool includingLookingAround = true) {
		player.GetComponent<CharacterMovementController>().EnableActions();
		player.GetComponentInChildren<SpellInput>().TryEnableSpellCasting();
		if (includingLookingAround) player.GetComponent<PlayerCameraController>().EnableRotation();
	}

	public void DisableLookingAround() {
		player.GetComponent<PlayerCameraController>().DisableRotation();
	}
	public void EnableLookingAround() {
		player.GetComponent<PlayerCameraController>().EnableRotation();
	}

	private void Awake() {
		Instance = this;
	}

	private void Start() {
		this.initialPlayerPosition = player.transform.position;
		this.initialPlayerRotation = player.transform.eulerAngles;
	}

	private void OnDestroy() {
		Instance = null;
	}

}
