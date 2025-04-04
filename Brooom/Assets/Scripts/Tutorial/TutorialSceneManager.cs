using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSceneManager : MonoBehaviour {

	// Just a simple singleton
	[HideInInspector] public static TutorialSceneManager Instance { get; private set; }

	[Header("Tutorial trigger zone")]
	[Tooltip("Sometimes the player has to enter the zone to progress further in the tutorial.")]
	public TutorialTriggerZone tutorialTriggerZone;
	[Tooltip("Parent object containing all objects which should be enabled when tutorial trigger zone is enabled.")]
	[SerializeField] GameObject triggerZoneParent;
	[Tooltip("Border separating tutorial trigger zone from the rest of the level. It should be disabled when trigger zoen is enabled.")]
	[SerializeField] GameObject borderToTriggerZone;

	[Header("Track elements")]
	[Tooltip("All hoops in the level.")]
	public List<Hoop> hoops;
	[Tooltip("All checkpoints in the level.")]
	public List<Hoop> checkpoints;

	[Header("Bonuses")]
	[Tooltip("All speed bonuses in the level.")]
	public List<Bonus> speedBonuses;
	[Tooltip("All mana bonuses in the level.")]
	public List<Bonus> manaBonuses;
	[Tooltip("All recharge bonuses in the level.")]
	public List<Bonus> rechargeBonuses;

	[Header("Player")]
	public GameObject player;
	private Vector3 initialPlayerPosition;
	private Vector3 initialPlayerRotation;

	[Header("Other")]
	[Tooltip("An opponent circling around the level, used as a potential spell target.")]
	[SerializeField] GameObject opponent;
	[Tooltip("Virtual camera which is moved around and rotated to focus on a specific object as part of the tutorial.")]
	public TutorialCamera cutsceneCamera;

	#region Level elements manipulation

	public void ResetAll() {
		ResetPlayerPositionAndRotation();
		HideTutorialTriggerZone();
		HideSimpleTrack();
		UtilsMonoBehaviour.SetActiveForAll(speedBonuses, false);
		UtilsMonoBehaviour.SetActiveForAll(manaBonuses, false);
		UtilsMonoBehaviour.SetActiveForAll(rechargeBonuses, false);
		opponent.SetActive(false);
	}

	public void ShowTutorialTriggerZone() {
		triggerZoneParent.SetActive(true);
		borderToTriggerZone.SetActive(false);
	}

	public void HideTutorialTriggerZone() {
		triggerZoneParent.SetActive(false);
		borderToTriggerZone.SetActive(true);
	}

	public void ShowSimpleTrack() {
		UtilsMonoBehaviour.SetActiveForAll(hoops, true);
		UtilsMonoBehaviour.SetActiveForAll(checkpoints, true);
	}

	public void HideSimpleTrack() {
		UtilsMonoBehaviour.SetActiveForAll(hoops, false);
		UtilsMonoBehaviour.SetActiveForAll(checkpoints, false);
	}
	#endregion

	#region Player manipulation
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
	#endregion

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
