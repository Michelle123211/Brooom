using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


/// <summary>
/// A class holding references to objects in the Player Overview scene which are important for tutorial.
/// These are then used for example when highlighting something on the screen.
/// </summary>
public class TutorialPlayerOverviewReferences : MonoBehaviour {

	// Just a simple singleton
	/// <summary>Singleton instance.</summary>
	public static TutorialPlayerOverviewReferences Instance { get; private set; }

	[Header("Player overview")]

	[Tooltip("RectTransform containing coins amount.")]
	public RectTransform coins;
	[Tooltip("RectTransform containing global leaderboard.")]
	public RectTransform leaderboard;
	[Tooltip("RectTransform containing graph of player stats.")]
	public RectTransform graph;
	[Tooltip("RectTransform containing equipped spells.")]
	public RectTransform equippedSpellsOverview;

	[Header("Shop")]

	[Tooltip("A broom object in the scene which provides preview in the shop. It can be used to get a list of available upgrades.")]
	public Broom broom;
	[Tooltip("An object containing all shop UI. It can be used to check whether the shop is opened, or not.")]
	public GameObject shopUI;
	[Tooltip("RectTransform containing shop button.")]
	public RectTransform shopButton;
	[Tooltip("RectTransform containing spells catalogue.")]
	public RectTransform spells;
	[Tooltip("RectTransform containing broom upgrades catalogue.")]
	public RectTransform broomUpgrades;
	[Tooltip("RectTransform containing equipped spells.")]
	public RectTransform equippedSpellsShop;
	[Tooltip("Object containing selection of spells to assign to a slot.")]
	public GameObject spellSelection;
	[Tooltip("RectTransform containing button for entering Testing track.")]
	public RectTransform testingTrackButton;


	private bool areGraphLabelsInitialized = false;
	private Dictionary<string, RectTransform> statsLabels; // stat name --> reference to RectTransform of corresponding graph label

	/// <summary>
	/// Gets a <c>RectTransform</c> of a graph label corresponding to a stat of the given name.
	/// </summary>
	/// <param name="localizedStatName">Localized name of the stat.</param>
	/// <returns><c>RectTransform</c> of a graph label corresponding to the given stat.</returns>
	public RectTransform GetGraphLabel(string localizedStatName) {
		if (!areGraphLabelsInitialized) InitializeGraphLabels();
		return statsLabels.GetValueOrDefault(localizedStatName);
	}

	// Prepares a Dictionary mapping localized stat name to a RectTransform of a corresponding graph label
	private void InitializeGraphLabels() {
		statsLabels = new Dictionary<string, RectTransform>();
		// Find all labels in the scene
		List<RadarGraphLabelUI> labelsInScene = UtilsMonoBehaviour.FindObjects<RadarGraphLabelUI>();
		// Get list of all localized stat names
		List<string> statNames = PlayerStats.GetListOfStatNames();
		for (int i = 0; i < statNames.Count; i++)
			statNames[i] = LocalizationManager.Instance.GetLocalizedString("PlayerStat" + statNames[i]);
		// Find out for which stat each label is and then store it in Dictionary
		foreach (var labelInScene in labelsInScene) {
			TextMeshProUGUI labelText = labelInScene.GetComponentInChildren<TextMeshProUGUI>();
			foreach (var statName in statNames) {
				// Compare first letters
				if (labelText.text[0] == statName[0]) {
					statsLabels.Add(statName, labelInScene.GetComponent<RectTransform>());
					break;
				}
			}
		}

		areGraphLabelsInitialized = true;
	}

	/// <summary>
	/// Goes through spell slots in the spell selection and finds the first non-empty one.
	/// </summary>
	/// <returns>The first non-empty slot in the spell selection.</returns>
	public RectTransform GetSpellFromSelection() {
		// Go through slots in spell selection and find the first non-empty one
		List<SpellSelectionSlotUI> spellsInSelection = UtilsMonoBehaviour.FindObjects<SpellSelectionSlotUI>();
		foreach (var spellInSelection in spellsInSelection) {
			SpellSlotUI spellSlot = spellInSelection.GetComponentInChildren<SpellSlotUI>();
			if (spellSlot.assignedSpell != null && !string.IsNullOrEmpty(spellSlot.assignedSpell.Identifier))
				return spellInSelection.GetComponent<RectTransform>();
		}
		return null;
	}

	private void Awake() {
		// Set singleton instance
		Instance = this;
	}

	private void OnDestroy() {
		// Reset singleton instance
		Instance = null;
	}

}
