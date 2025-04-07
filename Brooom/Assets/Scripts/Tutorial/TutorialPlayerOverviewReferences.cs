using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


// This class holds references to important objects inside of PlayerOverview scene
// - These are then used e.g. when highlighting something during tutorial
public class TutorialPlayerOverviewReferences : MonoBehaviour {

	// Just a simple singleton
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

	[Tooltip("A broom object in the scene which provides preview in the shop.")]
	public Broom broom;
	[Tooltip("An object containing all shop UI.")]
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
	private Dictionary<string, RectTransform> statsLabels;

	public RectTransform GetGraphLabel(string localizedStatName) {
		if (!areGraphLabelsInitialized) InitializeGraphLabels();
		return statsLabels.GetValueOrDefault(localizedStatName);
	}

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

	public RectTransform GetSpellFromSelection() {
		// Go through slots in spell seelction and find the first non-empty one
		List<SpellSelectionSlotUI> spellsInSelection = UtilsMonoBehaviour.FindObjects<SpellSelectionSlotUI>();
		foreach (var spellInSelection in spellsInSelection) {
			SpellSlotUI spellSlot = spellInSelection.GetComponentInChildren<SpellSlotUI>();
			if (spellSlot.assignedSpell != null && !string.IsNullOrEmpty(spellSlot.assignedSpell.Identifier))
				return spellInSelection.GetComponent<RectTransform>();
		}
		return null;
	}

	private void Awake() {
		Instance = this;
	}

	private void OnDestroy() {
		Instance = null;
	}

}
