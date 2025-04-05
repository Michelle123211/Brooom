using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


// This class holds references to important objects inside of PlayerOverview scene
// - These are then used e.g. when highlighting something during tutorial
public class TutorialPlayerOverviewReferences : MonoBehaviour {

	[Header("Player overview")]

	[Tooltip("RectTransform containing coins amount.")]
	public RectTransform coins;
	[Tooltip("RectTransform containing global leaderboard.")]
	public RectTransform leaderboard;
	[Tooltip("RectTransform containing graph of player stats.")]
	public RectTransform graph;

	[Header("Shop")]

	[Tooltip("A broom object in the scene which provides preview in the shop.")]
	public Broom broom;
	[Tooltip("RectTransform containing shop button.")]
	public RectTransform shopButton;
	[Tooltip("RectTransform containing spells catalogue.")]
	public RectTransform spells;
	[Tooltip("RectTransform containing broom upgrades catalogue.")]
	public RectTransform broomUpgrades;


	private bool isInitialized = false;
	private Dictionary<string, RectTransform> statsLabels;

	public RectTransform GetGraphLabel(string localizedStatName) {
		if (!isInitialized) InitializeReferences();
		return statsLabels.GetValueOrDefault(localizedStatName);
	}

	private void InitializeReferences() {
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

		isInitialized = true;
	}

}
