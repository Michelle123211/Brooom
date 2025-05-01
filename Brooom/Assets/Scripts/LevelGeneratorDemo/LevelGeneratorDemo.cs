using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System;

/// <summary>
/// This class handles level generation based on parameters given by UI element.
/// It is derived from <c>QuickRaceGeneration</c> to inherit all parameters (with their default values), useful methods, 
/// and a suitable implementation of <c>ChooseTerrainRegionsForLevel()</c>, which selects regions at random from all available ones.
/// </summary>
public class LevelGeneratorDemo : QuickRaceGeneration {

	[Header("UI sections")]
	[Tooltip("A parent object containing all UI elements. Can be shown or hidden using Space.")]
	[SerializeField] GameObject canvas;

	// TODO: Uncomment for support of second option of setting generator parameters directly (instead of based on stats) - to toggle between them
	//[Tooltip("A parent object containing all UI elements for setting generator parameters based on player stats.")]
	//[SerializeField] GameObject statsParameterSection;
	//[Tooltip("A parent object containing all UI elements for setting generator parameters directly.")]
	//[SerializeField] GameObject generatorParameterSection;

	[Header("Stats-based UI elements")]
	[Tooltip("Slider for setting the Endurance stat.")]
	[SerializeField] Slider enduranceSlider;
	[Tooltip("Slider for setting the Speed stat.")]
	[SerializeField] Slider speedSlider;
	[Tooltip("Slider for setting the Dexterity stat.")]
	[SerializeField] Slider dexteritySlider;
	[Tooltip("Slider for setting the Precision stat.")]
	[SerializeField] Slider precisionSlider;
	// Magic stat has no effect on level generation
	[Tooltip("Slider for setting the maximum altitude the broom can get to.")]
	[SerializeField] Slider maxAltitudeSlider;

	// TODO: Uncomment for support of second option of setting generator parameters directly (instead of based on stats) - to toggle between them
	//[Header("Direct parameters UI elements")]
	//[SerializeField] TMP_InputField numOfCheckpoints;
	//[SerializeField] TMP_InputField numOfHoops;
	//[SerializeField] TMP_InputField maxDirectionChangeX;
	//[SerializeField] TMP_InputField maxDirectionChangeY;
	//[SerializeField] TMP_InputField hoopScale;
	//[SerializeField] TMP_InputField minHoopDistance;
	//[SerializeField] TMP_InputField maxHoopDistance;
	// TODO: And many more - e.g., available regions, number of opponents, maximum altitude, ...

	[Header("General settings UI elements")]
	[Tooltip("Dropdown with options to select only a subset of all modules used for level generation.")]
	[SerializeField] TMP_Dropdown moduleDropdown;
	[Tooltip("Different options for enabling only a subset of modules used for level generation. Options should be additive, i.e. one options enables additional modules on top of all modules enabled in all previous options.")]
	[SerializeField] List<ModuleListOption> modulesToEnableOptions;

	[Header("Level layers")]
	[Tooltip("A parent object containing all terrain meshes.")]
	[SerializeField] GameObject terrainParent;
	[Tooltip("A parent object containing objects of starting zone and finish line.")]
	[SerializeField] GameObject startAndFinishParent;
	[Tooltip("A parent object containing all hoops and checkpoints.")]
	[SerializeField] GameObject hoopsParent;
	[Tooltip("A parent object containing all bonuses.")]
	[SerializeField] GameObject bonusesParent;
	[Tooltip("A parent object containing all environment elements.")]
	[SerializeField] GameObject environmentElementsParent;
	[Tooltip("A parent object containing all track border objects.")]
	[SerializeField] GameObject trackBorderParent;

	// TODO: Uncomment for support of second option of setting generator parameters directly (instead of based on stats) - to toggle between them
	// There are two options of setting generator parameters (based on stats, or directly) - we can switch between them
	//private bool useStats = true;

	private LevelRepresentation level;


	/// <summary>
	/// Completely deletes everything from the previously generated level and generates a new one using current values from UI elements as parameters.
	/// </summary>
	public void RegenerateLevel() {
		levelGenerator.onLevelGenerated += OnLevelGenerated;
		DeletePreviousLevel();
		SetupGeneratorModules();
		// TODO: Uncomment for support of second option of setting generator parameters directly (instead of based on stats) - to toggle between them
		// If level should be generated based on stats, use inherited method
		//if (useStats) 
			StartCoroutine(GenerateLevel()); // set parameters and generate level
		// Else use custom method for setting parameters directly
		//else StartCoroutine(GenerateLevelWithDirectParameters());
	}

	/// <summary>
	/// Loads Main Menu scene.
	/// </summary>
	public void GoBackToMainMenu() {
		SceneLoader.Instance.LoadScene(Scene.MainMenu);
	}

	// TODO: Uncomment for support of second option of setting generator parameters directly (instead of based on stats) - to toggle between them
	//public void OnInputModeChanged(bool useStats) {
	//	// Show/hide corresponding UI elements
	//	this.useStats = useStats;
	//	statsParameterSection.SetActive(useStats);
	//	generatorParameterSection.SetActive(!useStats);
	//} 

	/// <summary>
	/// Initializes dropdown options for modules selection, but does not generate a level right-away.
	/// </summary>
	protected override IEnumerator InitializeAfterPreparation() {
		InitializeModuleDropdownOptions();
		// Do nothing, don't generate level right at the start
		yield break;
	}

	// Deletes all previously generates objects
	//	- modules usually do that themselves, but here we have an option to disable modules so then they cannot clean up
	private void DeletePreviousLevel() {
		UtilsMonoBehaviour.RemoveAllChildren(terrainParent.transform);
		UtilsMonoBehaviour.RemoveAllChildren(startAndFinishParent.transform);
		UtilsMonoBehaviour.RemoveAllChildren(hoopsParent.transform);
		UtilsMonoBehaviour.RemoveAllChildren(bonusesParent.transform);
		UtilsMonoBehaviour.RemoveAllChildren(environmentElementsParent.transform);
		UtilsMonoBehaviour.RemoveAllChildren(trackBorderParent.transform);
	}

	// Enables or disables level generation modules based on the currently selected dropdown option
	private void SetupGeneratorModules() {
		int currentOption = moduleDropdown.value;
		for (int i = 0; i < modulesToEnableOptions.Count; i++) {
			// All modules from the current option and all previous options are enabled, all modules from later options are disabled
			foreach (int moduleIndex in modulesToEnableOptions[i].addedModulesIndices)
				levelGenerator.EnableOrDisableModule(moduleIndex, i <= currentOption);
		}
	}

	#region Stats-based generator parameters

	/// <inheritdoc/>
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

	/// <inheritdoc/>
	protected override float GetMaxAltitude() {
		// Get value from UI element
		return maxAltitudeSlider.value;
	}

	/// <inheritdoc/>
	protected override Dictionary<LevelRegionType, bool> GetRegionsAvailability() {
		// Make regions available based on endurance value and maximum altitude (from UI elements)

		Dictionary<LevelRegionType, bool> regionsAvailability = new();
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

	/// <inheritdoc/>
	protected override Dictionary<LevelRegionType, bool> GetRegionsVisited() {
		// Simply mark all regions in the game as visited (otherwise e.g. unvisited track region would be always added in TrackRegionGenerator)
		Dictionary<LevelRegionType, bool> visitedRegions = new();
		foreach (var region in levelGenerator.terrainRegions) visitedRegions.Add(region.regionType, true);
		foreach (var region in levelGenerator.trackRegions) visitedRegions.Add(region.regionType, true);
		return visitedRegions;
	}
	#endregion

	#region Direct generator parameters

	// TODO: Implement support of second option of setting generator parameters directly (instead of based on stats) - to toggle between them
	private IEnumerator GenerateLevelWithDirectParameters() {
		// TODO: Set parameters directly
		// Generate level
		yield return levelGenerator.GenerateLevel();
	}

	#endregion

	#region General settings parameters

	/// <summary>Shows or hides all terrain objects. </summary>
	/// <param name="show"><c>true</c> to show the objects, <c>false</c> to hide them.</param>
	public void OnTerrainShowOrHide(bool show) => terrainParent.SetActive(show);
	/// <summary>Shows or hides starting zone and finish line objects. </summary>
	/// <param name="show"><c>true</c> to show the objects, <c>false</c> to hide them.</param>
	public void OnStartAndFinishShowOrHide(bool show) => startAndFinishParent.SetActive(show);
	/// <summary>Shows or hides all hoops and checkpoints objects. </summary>
	/// <param name="show"><c>true</c> to show the objects, <c>false</c> to hide them.</param>
	public void OnHoopsShowOrHide(bool show) => hoopsParent.SetActive(show);
	/// <summary>Shows or hides all bonuses objects. </summary>
	/// <param name="show"><c>true</c> to show the objects, <c>false</c> to hide them.</param>
	public void OnBonusesShowOrHide(bool show) => bonusesParent.SetActive(show);
	/// <summary>Shows or hides all environment element objects. </summary>
	/// <param name="show"><c>true</c> to show the objects, <c>false</c> to hide them.</param>
	public void OnEnvironmentElementsShowOrHide(bool show) => environmentElementsParent.SetActive(show);
	/// <summary>Shows or hides all track border objects. </summary>
	/// <param name="show"><c>true</c> to show the objects, <c>false</c> to hide them.</param>
	public void OnTrackBorderShowOrHide(bool show) => trackBorderParent.SetActive(show);


	// Initialized options for dropdown enabling only a subset of level generation modules (based on options listed in moduleListOption)
	private void InitializeModuleDropdownOptions() {
		moduleDropdown.ClearOptions();
		foreach (var moduleListOption in modulesToEnableOptions)
			moduleDropdown.options.Add(new TMP_Dropdown.OptionData(moduleListOption.dropdownOption));
		moduleDropdown.value = modulesToEnableOptions.Count - 1;
		moduleDropdown.GetComponentInChildren<TextMeshProUGUI>().text = modulesToEnableOptions[moduleDropdown.value].dropdownOption;
	}

	#endregion

	// Shows or hides all UI elements in the screen
	private void ToggleUIVisibility() {
		bool show = !canvas.activeInHierarchy;
		canvas.SetActive(show);
		// Enable or disable cursor accordingly
		if (show) Utils.EnableCursor();
		else Utils.DisableCursor();
	}

	private void OnLevelGenerated(LevelRepresentation level) {
		this.level = level;
		StartingZone startingZone = FindObjectOfType<StartingZone>();
		if (startingZone != null) startingZone.ShowOrHideUI(false);
		levelGenerator.onLevelGenerated -= OnLevelGenerated;
	}

	/// <inheritdoc/>
	protected override void UpdateAfterInitialization() {
		// Detect any input
		// ... Space - show/hide UI
		if (Input.GetKeyDown(KeyCode.Space))
			ToggleUIVisibility();
		// ... ESC - return to menu
		if (Input.GetKeyDown(KeyCode.Escape))
			GoBackToMainMenu();
		// ... Ctrl + C - capture (ans save) screenshot
		if (Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.C)) // no alt so screenshot is not captured when showing/hiding cheats command line
			Utils.SaveScreenshot();
	}

}


/// <summary>
/// A class describing one option of enabling only a subset of modules for level generation.
/// </summary>
[System.Serializable]
internal class ModuleListOption {
	[Tooltip("Name of this option visible in a dropdown.")]
	public string dropdownOption;
	[Tooltip("Indices of modules which should be enabled on top of modules from all previous options if this option is selected.")]
	public List<int> addedModulesIndices;
}