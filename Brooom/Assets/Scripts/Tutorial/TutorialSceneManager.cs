using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A singleton providing easy access to objects in Tutorial scene.
/// It also provides useful methods for setting the scene up and for manipulation with the objects.
/// </summary>
public class TutorialSceneManager : MonoBehaviour {

	// Just a simple singleton
	/// <summary>Singleton instance.</summary>
	public static TutorialSceneManager Instance { get; private set; }

	[Header("Tutorial trigger zone")]
	[Tooltip("Sometimes the player has to enter the zone to progress further in the tutorial.")]
	public TutorialTriggerZone tutorialTriggerZone;
	[Tooltip("Parent object containing all objects which should be enabled when tutorial trigger zone is enabled (e.g. trigger zone, borders).")]
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
	[Tooltip("The player object in the scene.")]
	public GameObject player;

	[Header("HUD")]
	[Tooltip("RectTransform containing equipped spells.")]
	public RectTransform availableSpellsRect;
	[Tooltip("RectTransform containing mana bar.")]
	public RectTransform manaBarRect;
	[Tooltip("RectTransform containing equipped spells and mana bar.")]
	public RectTransform spellsAndManaBarRect;

	[Header("Other")]
	[Tooltip("An opponent circling around the level, used as a potential spell target.")]
	[SerializeField] GameObject opponent;


	#region Level elements manipulation

	/// <summary>
	/// Resets everything in the scene to the initial state (e.g. reseting player's position and rotation, 
	/// hiding tutorial trigger zone, hoops, bonuses and opponent).
	/// </summary>
	public void ResetAll() {
		TutorialBasicManager.Instance.ResetPlayerPositionAndRotation();
		HideTutorialTriggerZone();
		HideSimpleTrack();
		UtilsMonoBehaviour.SetActiveForAll(speedBonuses, false);
		UtilsMonoBehaviour.SetActiveForAll(manaBonuses, false);
		UtilsMonoBehaviour.SetActiveForAll(rechargeBonuses, false);
		opponent.SetActive(false);
	}

	/// <summary>
	/// Shows tutorial trigger zone with border around it and hides border separating the trigger zone from the rest of the level.
	/// </summary>
	public void ShowTutorialTriggerZone() {
		triggerZoneParent.SetActive(true);
		borderToTriggerZone.SetActive(false);
	}
	/// <summary>
	/// Hides tutorial trigger zone with borders around it and shows border separating the trigger zone from the rest of the level.
	/// </summary>
	public void HideTutorialTriggerZone() {
		triggerZoneParent.SetActive(false);
		borderToTriggerZone.SetActive(true);
	}

	/// <summary>
	/// Shows hoops and checkpoints creating a simple track.
	/// </summary>
	public void ShowSimpleTrack() {
		UtilsMonoBehaviour.SetActiveForAll(hoops, true);
		UtilsMonoBehaviour.SetActiveForAll(checkpoints, true);
	}
	/// <summary>
	/// Hides hoops and checkpoints which are part of a simple track.
	/// </summary>
	public void HideSimpleTrack() {
		UtilsMonoBehaviour.SetActiveForAll(hoops, false);
		UtilsMonoBehaviour.SetActiveForAll(checkpoints, false);
	}

	/// <summary>
	/// Shows at least one of each possible spell target (e.g. opponent, bonus).
	/// </summary>
	public void ShowAllPossibleSpellTargets() {
		// Opponent
		opponent.SetActive(true);
		// Bonuses
		UtilsMonoBehaviour.SetActiveForAll(manaBonuses, true);
		// TODO: Hoop (in the future)
	}
	#endregion

	private void Awake() {
		// Srt singleton instance
		Instance = this;
	}

	private void OnDestroy() {
		// Reset singleton instance
		Instance = null;
	}
}
