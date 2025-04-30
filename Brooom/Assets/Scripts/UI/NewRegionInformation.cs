using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// A component responsible for displaying information about a new unvisited region in the level.
/// When there are multiple of these, information about them is displayed one after another.
/// </summary>
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

	/// <summary>
	/// Shows a panel containing information about a new unvisited region which is located in the generated level.
	/// </summary>
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

	/// <summary>
	/// Hides a panel containing information about a new unvisited region which is located in the generated level.
	/// If there are more regions to show, moves on to the next one and schedules displaying the panel for it.
	/// </summary>
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

	// Notes down that the Race scene has finished loading
	// If a list of unvisited regions is initialized as well, it starts showing information about them
	private void OnSceneLoaded(Scene scene) {
		if (scene != Scene.Race) return;
		sceneLoaded = true;
		if (regionsInitialized)
			Invoke(nameof(ShowRegionInformation), 1f);
	}

	// Prepares a list of unvisited regions located in the level and notes down it is initialized
	// If the Race scene has finished loading as well, it starts showing information about the unvisited regions
	private void OnLevelGenerated(LevelRepresentation level) {
		regionsToDisplay = new List<LevelRegion>();
		currentRegionIndex = int.MaxValue;

		// Get list of all unvisited regions which are in the level
		foreach (var region in level.RegionsInLevel) {
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
