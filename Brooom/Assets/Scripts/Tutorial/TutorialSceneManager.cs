using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// A component handling manipulations with objects in Tutorial scene
public class TutorialSceneManager : MonoBehaviour {

	// Just a simple singleton
	public static TutorialSceneManager Instance { get; private set; }

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

	[Header("HUD")]
	[Tooltip("RectTransform containing available spells.")]
	public RectTransform availableSpellsRect;
	[Tooltip("RectTransform containing mana bar.")]
	public RectTransform manaBarRect;
	[Tooltip("RectTransform containing available spells and mana bar.")]
	public RectTransform spellsAndManaBarRect;

	[Header("Other")]
	[Tooltip("An opponent circling around the level, used as a potential spell target.")]
	[SerializeField] GameObject opponent;

	#region Level elements manipulation

	public void ResetAll() {
		TutorialBasicManager.Instance.ResetPlayerPositionAndRotation();
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

	public void ShowAllPossibleSpellTargets() {
		// Opponent
		opponent.SetActive(true);
		// Bonuses
		UtilsMonoBehaviour.SetActiveForAll(manaBonuses, true);
		// TODO: Hoop (in the future)
	}
	#endregion

	private void Awake() {
		Instance = this;
	}

	private void OnDestroy() {
		Instance = null;
	}
}
