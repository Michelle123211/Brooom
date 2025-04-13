using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

public class ConnectingPairs : ExperimentPart {

    public static string sceneName = "3-ConnectingPairs";

	[SerializeField] CanvasGroup directionsContent;
	[SerializeField] CanvasGroup experimentContent;

	[SerializeField] Transform pairsGridLayout;
	[SerializeField] IconDescriptionPair iconDescriptionPairPrefab;
	[SerializeField] Transform descriptionsGridLayout;
	[SerializeField] SpellDescriptionOption spellDescriptionOptionPrefab;

	private List<string> englishDescriptions;
	private List<string> czechDescriptions;
	private List<SpellDescription> spells;
	private int[] assignedDescriptions;

	private List<SpellDescriptionOption> descriptionObjects;

	private ConnectingPairsStep currentStep = ConnectingPairsStep.Directions;

	public override void OnNextButtonClicked() {
		nextButton.DOFade(0f, 0.3f);
		// Move to next step
		currentStep = (ConnectingPairsStep)((int)currentStep + 1);
		// Do something based on current step
		InitializeStep();
	}

	public void OnSpellDescriptionAssigned(int spellIndex, int descriptionValue) {
		assignedDescriptions[spellIndex] = descriptionValue - 1;
		if (descriptionValue > 0)
			DataLogger.Instance.Log($"Part 3 | Change | {spells[spellIndex].spellName} | {englishDescriptions[descriptionValue - 1]} | {czechDescriptions[descriptionValue - 1]}");
		else
			DataLogger.Instance.Log($"Part 3 | Change | {spells[spellIndex].spellName} |  | ");
		UpdateSelection();
	}

	private void UpdateSelection() {
		// For each description, check if it is assigned to a spell - select/deselect accordingly
		bool allSelected = true;
		for (int i = 0; i < descriptionObjects.Count; i++) {
			bool shouldBeSelected = false;
			foreach (var assignment in assignedDescriptions) {
				if (assignment == i) {
					shouldBeSelected = true;
					break;
				}
			}
			descriptionObjects[i].SelectOrDeselect(shouldBeSelected);
			if (!shouldBeSelected) allSelected = false;
		}
		nextButton.interactable = allSelected;
	}

	private void InitializeStep() {
		switch (currentStep) {
			case ConnectingPairsStep.Directions:
				// Hide everything
				nextButton.alpha = 0;
				directionsContent.alpha = 0;
				experimentContent.alpha = 0;
				// Show directions
				directionsContent.DOFade(1f, 0.3f).OnComplete(() => nextButton.DOFade(1f, 0.3f));
				break;
			case ConnectingPairsStep.Experiment:
				// Hide directions, show experiment info
				directionsContent.DOFade(0f, 0.3f).OnComplete(() => {
					nextButton.interactable = false;
					experimentContent.DOFade(1f, 0.3f).OnComplete(() => nextButton.DOFade(1f, 0.3f));
				});
				break;
			case ConnectingPairsStep.Finished:
				// Save all data
				for (int i = 0; i < assignedDescriptions.Length; i++) {
					DataLogger.Instance.Log($"Part 3 | Submit | {spells[i].spellName} | {englishDescriptions[assignedDescriptions[i]]} | {czechDescriptions[assignedDescriptions[i]]}");
				}
				// Go to next scene
				DataLogger.Instance.Log("----------------------------------------", false);
				experimentContent.DOFade(0f, 0.3f).OnComplete(() => SceneManager.LoadScene(End.sceneName));
				break;
		}
	}

	private void InitializeGridItems() {
		SpellDescription[] spellsArray = Resources.LoadAll<SpellDescription>("Spells/");
		assignedDescriptions = new int[spellsArray.Length];
		for (int i = 0; i < spellsArray.Length; i++) assignedDescriptions[i] = -1;
		// Create a list of dropdown options
		List<string> dropdownOptions = new List<string>();
		dropdownOptions.Add(string.Empty);
		for (int i = 0; i < spellsArray.Length; i++) dropdownOptions.Add(((char)('A' + i)).ToString());
		// Create a list of descriptions (in random order) and instantiate them
		englishDescriptions = new List<string>();
		czechDescriptions = new List<string>();
		descriptionObjects = new List<SpellDescriptionOption>();
		List<SpellDescription> spellsList = new List<SpellDescription>(spellsArray); // create a list so we can remove items for random selection
		int j = 0;
		while (spellsList.Count > 0) {
			int spellIndex = Random.Range(0, spellsList.Count);
			englishDescriptions.Add(spellsList[spellIndex].spellDescriptionEnglish);
			czechDescriptions.Add(spellsList[spellIndex].spellDescriptionCzech);
			SpellDescriptionOption spellDescriptionOption = Instantiate<SpellDescriptionOption>(spellDescriptionOptionPrefab, descriptionsGridLayout);
			spellDescriptionOption.Initialize(dropdownOptions[j + 1], englishDescriptions[j], czechDescriptions[j]);
			descriptionObjects.Add(spellDescriptionOption);
			spellsList.RemoveAt(spellIndex);
			j++;
		}
		// Instantiate all icons with dropdowns
		spells = new List<SpellDescription>();
		spellsList = new List<SpellDescription>(spellsArray); // re-create the list so we can remove items for random selection
		while (spellsList.Count > 0) {
			int spellIndex = Random.Range(0, spellsList.Count);
			IconDescriptionPair iconDescriptionPair = Instantiate<IconDescriptionPair>(iconDescriptionPairPrefab, pairsGridLayout);
			iconDescriptionPair.Initialize(spells.Count, spellsList[spellIndex], dropdownOptions, this);
			spells.Add(spellsList[spellIndex]);
			spellsList.RemoveAt(spellIndex);
		}
	}

	private void Start() {
		InitializeGridItems();
		InitializeStep();
	}

}

internal enum ConnectingPairsStep {
	Directions,
	Experiment,
	Finished
}
