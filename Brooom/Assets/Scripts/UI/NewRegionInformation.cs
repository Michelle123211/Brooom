using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NewRegionInformation : MonoBehaviour {

	[Tooltip("Level generation pipeline for registering callback on level generated.")]
	[SerializeField] LevelGenerationPipeline levelGenerator;

	[Tooltip("Parent object of the whole panel for displaying information about new region.")]
	[SerializeField] GameObject regionInformationPanel;

	[Tooltip("TextMesh Pro for displaying region name.")]
	[SerializeField] TextMeshProUGUI regionTitle;
	[Tooltip("TextMesh Pro for displaying region description.")]
	[SerializeField] TextMeshProUGUI regionDescription;
	[Tooltip("Image for displaying region image as a panel background.")]
	[SerializeField] Image regionImage;

	private List<LevelRegion> regionsToDisplay;
	private int currentRegionIndex = 0;

	private bool sceneLoaded = false;
	private bool regionsInitialized = false;

	public void ShowRegionInformation() {
		if (currentRegionIndex >= regionsToDisplay.Count) return;
		// Set panel content
		LevelRegion region = regionsToDisplay[currentRegionIndex];
		string regionLocalizationKey = "Region" + region.regionType.ToString();
		regionTitle.text = LocalizationManager.Instance.GetLocalizedString(regionLocalizationKey);
		regionDescription.text = LocalizationManager.Instance.GetLocalizedString(regionLocalizationKey + "Description");
		regionImage.sprite = region.regionImage;
		// Show panel and mouse cursor
		GamePause.DisableGamePause();
		Time.timeScale = 0;
		AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelOpen);
		regionInformationPanel.TweenAwareEnable();
		Utils.EnableCursor();
	}

	public void HideRegionInformation() {
		// Hide panel and mouse cursor
		AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelClose);
		regionInformationPanel.TweenAwareDisable();
		Utils.DisableCursor();
		Time.timeScale = 1;
		// Consider other new regions
		currentRegionIndex++;
		if (currentRegionIndex < regionsToDisplay.Count) Invoke(nameof(ShowRegionInformation), 0.3f);
	}

	private void OnSceneLoaded(Scene scene) {
		if (scene != Scene.Race) return;
		sceneLoaded = true;
		if (regionsInitialized)
			Invoke(nameof(ShowRegionInformation), 1f);
	}

	private void OnLevelGenerated(LevelRepresentation level) {
		regionsToDisplay = new List<LevelRegion>();
		currentRegionIndex = int.MaxValue;

		// Get list of all unvisited regions which are in the level
		foreach (var region in level.regionsInLevel) {
			if (!PlayerState.Instance.regionsVisited.TryGetValue(region, out bool isVisited) || !isVisited) {
				// New region detected, get its data
				foreach (var regionData in levelGenerator.terrainRegions) {
					if (regionData.regionType == region) regionsToDisplay.Add(regionData);
				}
				foreach (var regionData in levelGenerator.trackRegions) {
					if (regionData.regionType == region) regionsToDisplay.Add(regionData);
				}
				currentRegionIndex = 0;
			}
		}
		regionsInitialized = true;

		if (sceneLoaded)
			Invoke(nameof(ShowRegionInformation), 1f);
	}

	private void Start() {
		// Show information about new regions only outside of First Race tutorial
		if (Tutorial.Instance.CurrentStage != TutorialStage.FirstRace) {
			levelGenerator.onLevelGenerated += OnLevelGenerated;
			SceneLoader.Instance.onSceneLoaded += OnSceneLoaded;
		}
	}

	private void OnDestroy() {
		levelGenerator.onLevelGenerated -= OnLevelGenerated;
		SceneLoader.Instance.onSceneLoaded -= OnSceneLoaded;
	}
}
