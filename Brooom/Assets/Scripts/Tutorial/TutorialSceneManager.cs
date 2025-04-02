using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSceneManager : MonoBehaviour {

	[Header("Tutorial trigger zone")]
	[Tooltip("Sometimes the player has to enter the zone to progress further in the tutorial.")]
	public TutorialTriggerZone tutorialTriggerZone;
	[Tooltip("Parent object containing all objects which should be enabled when tutorial trigger zone is enabled.")]
	[SerializeField] GameObject triggerZoneParent;
	[Tooltip("Border separating tutorial trigger zone from the rest of the level. It should be disabled when trigger zoen is enabled.")]
	[SerializeField] GameObject borderToTriggerZone;

	[Header("Track elements")]
	[Tooltip("Parent object containing all objects which are part of a simple track.")]
	[SerializeField] GameObject simpleTrackParent;

	[Header("Bonuses")]
	[Tooltip("Parent object containing all speed bonus objects.")]
	[SerializeField] GameObject speedBonusParent;
	[Tooltip("Parent object containing all mana bonus objects.")]
	[SerializeField] GameObject manaBonusParent;
	[Tooltip("Parent object containing all recharge bonus objects.")]
	[SerializeField] GameObject rechargeBonusParent;

	[Header("Other")]
	public GameObject player;
	[Tooltip("An opponent circling around the level, used as a potential spell target.")]
	[SerializeField] GameObject opponent;
	[Tooltip("Virtual camera which is moved around and rotated to focus on a specific object as part of the tutorial.")]
	public TutorialCamera cutsceneCamera;

	#region Level elements manipulation
	public void ShowTutorialTriggerZone() {
		triggerZoneParent.SetActive(true);
		borderToTriggerZone.SetActive(false);
	}

	public void HideTutorialTriggerZone() {
		triggerZoneParent.SetActive(false);
		borderToTriggerZone.SetActive(true);
	}

	public void ShowSimpleTrack() {
		simpleTrackParent.SetActive(true);
	}

	public void HideSimpleTrack() {
		simpleTrackParent.SetActive(true);
	}
	#endregion

	#region Player manipulation
	public void RotatePlayerTowards(Transform target) {
		// Only rotation around Y
		Vector3 lookDirection = target.position - player.transform.position;
		Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, lookDirection);
		player.transform.eulerAngles = player.transform.eulerAngles.WithY(rotation.eulerAngles.y);
	}
	#endregion

}
