using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System;

/// <summary>
/// This class is derived from <c>QuickRaceGeneration</c> to inherit all parameters (with their default values) and also useful methods.
/// It is not derived from <c>RaceGeneration</c> to inherit more suitable implementation of <c>ChooseTerrainRegionsForLevel()</c>, which selects regions at random from all available ones.
/// </summary>
public class LevelGeneratorDemo : QuickRaceGeneration {

	[Header("UI sections")]
	[Tooltip("A parent object containing all UI elements. Can be shown or hidden using Space.")]
	[SerializeField] GameObject canvas;
	[Tooltip("A parent object containing all UI elements for setting generator parameters based on player stats.")]
	[SerializeField] GameObject statsParameterSection;
	[Tooltip("A parent object containing all UI elements for setting generator parameters directly.")]
	[SerializeField] GameObject generatorParameterSection;

	[Header("Stats-based UI elements")]
	[SerializeField] Slider enduranceSlider;
	[SerializeField] Slider speedSlider;
	[SerializeField] Slider dexteritySlider;
	[SerializeField] Slider precisionSlider;
	// Magic stat has no effect on level generation
	[SerializeField] Slider maxAltitudeSlider;

	// There are two options of setting generator parameters (based on stats, or directly) - we can switch between them
	private bool useStats = true;

	private LevelRepresentation level;


	public void RegenerateLevel() {
		levelGenerator.onLevelGenerated += OnLevelGenerated;
		// If level should be generated based on stats, use inherited method
		if (useStats) StartCoroutine(GenerateLevel()); // set parameters and generate level
		// Else use custom method for setting parameters directly
		else return;
	}

	public void GoBackToMainMenu() {
		SceneLoader.Instance.LoadScene(Scene.MainMenu);
	}

	public void OnParameterModeChanged(bool useStats) {
		// Show/hide corresponding UI elements
		this.useStats = useStats;
		statsParameterSection.SetActive(useStats);
		generatorParameterSection.SetActive(!useStats);
	} 

	/// <summary>
	/// This method overrides the inherited method so that the level is not generated immediately at the start.
	/// </summary>
	protected override IEnumerator InitializeAfterPreparation() {
		// Do nothing, don't generate level right at the start
		yield break;
	}

	#region Stats-based generator parameters
	protected override PlayerStats GetPlayerStats() {
		// Get values from UI elements
		return new PlayerStats {
			endurance = (int)enduranceSlider.value,
			speed = (int)speedSlider.value,
			dexterity = (int)dexteritySlider.value,
			precision = (int)precisionSlider.value
			// Magic stat has no effect on level generation
		};
	}

	protected override float GetMaxAltitude() {
		// Get value from UI element
		return maxAltitudeSlider.value;
	}

	protected override Dictionary<LevelRegionType, bool> GetRegionsAvailability() {
		// Make regions available based on endurance value and maximum altitude (from UI elements)

		Dictionary<LevelRegionType, bool> regionsAvailability = new Dictionary<LevelRegionType, bool>();
		foreach (var region in defaultRegions)
			regionsAvailability.Add(region, true); // default regions are always available

		int endurance = (int)enduranceSlider.value;
		foreach (var regionWithValue in regionsUnlockedByEndurance)
			regionsAvailability.Add(regionWithValue.region, endurance >= regionWithValue.minValue);

		float maxAltitude = maxAltitudeSlider.value;
		foreach (var regionWithValue in regionsUnlockedByAltitude)
			regionsAvailability.Add(regionWithValue.region,maxAltitude >= regionWithValue.minValue);

		return regionsAvailability;
	}

	protected override Dictionary<LevelRegionType, bool> GetRegionsVisited() {
		// Simply mark all regions in the game as visited (otherwise e.g. unvisited track region would be always added in TrackRegionGenerator)
		Dictionary<LevelRegionType, bool> visitedRegions = new Dictionary<LevelRegionType, bool>();
		foreach (var region in levelGenerator.terrainRegions) visitedRegions.Add(region.regionType, true);
		foreach (var region in levelGenerator.trackRegions) visitedRegions.Add(region.regionType, true);
		return visitedRegions;
	}
	#endregion

	#region Direct generator parameters
	#endregion

	private void ToggleUIVisibility() {
		bool show = !canvas.activeInHierarchy;
		canvas.SetActive(show);
		// Enable or disable cursor accordingly
		if (show) Utils.EnableCursor();
		else Utils.DisableCursor();
	}

	private void OnLevelGenerated(LevelRepresentation level) {
		this.level = level;
		FindObjectOfType<StartingZone>().ShowOrHideUI(false);
		levelGenerator.onLevelGenerated -= OnLevelGenerated;
	}

	protected override void UpdateAfterInitialization() {
		// Detect any input
		// ... Space - show/hide UI
		if (Input.GetKeyDown(KeyCode.Space))
			ToggleUIVisibility();
		// ... ESC - return to menu
		if (Input.GetKeyDown(KeyCode.Escape))
			GoBackToMainMenu();
		// ... Ctrl + C - capture (ans save) screenshot
		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C))
			Utils.SaveScreenshot();
	}

}
