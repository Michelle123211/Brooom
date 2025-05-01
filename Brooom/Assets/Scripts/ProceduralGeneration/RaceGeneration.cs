using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class is responsible for generating a level at the start of the Race scene.
/// Parameters for generation are selected based on current game state (e.g., player stats, broom upgrades).
/// Other classes may extend this one to alter its behaviour (e.g., to make it independent of game state and instead set parameters in a different way).
/// </summary>
public class RaceGeneration : MonoBehaviourLongInitialization {

    [Header("Level length (Endurance)")]
    [Tooltip("How many checkpoints should be generated when the player's Endurance stat is 0.")]
    public int initialNumberOfCheckpoints = 4;
    [Tooltip("How many checkpoints should be generated when the player's Endurance stat is 100.")]
    public int finalNumberOfCheckpoints = 10;

    [Header("Direction change (Dexterity)")]
    [Tooltip("Maximum angle between two consecutive hoops in the X (up/down) and Y (left/right) axis when the player's Dexterity stat is 0.")]
    public Vector2 initialDirectionChange = new Vector2(5, 20);
    [Tooltip("Maximum angle between two consecutive hoops in the X (up/down) and Y (left/right) axis when the player's Dexterity stat is 100.")]
    public Vector2 finalDirectionChange = new Vector2(20, 45);

    [Header("Hoop scale (Precision)")]
    [Tooltip("Scale of hoops when the player's Precision stat is 0.")]
    public float initialHoopScale = 1f;
    [Tooltip("Scale of hoops when the player's Precision stat is 100.")]
    public float finalHoopScale = 0.4f;

    [Header("Hoop distance (Speed)")]
    [Tooltip("The approximate minimum and maximum distance between two hoops when the player's Speed stat is 0.")]
    public Vector2 initialHoopDistanceRange = new Vector2(40, 50);
    [Tooltip("The approximate minimum and maximum distance between two hoops when the player's Speed stat is 100.")]
    public Vector2 finalHoopDistanceRange = new Vector2(80, 100);

    [Header("Regions")]
    [Tooltip("This many distinct regions will be used at maximum in each generated level.")]
    public int regionCountInLevel = 3;
    [Tooltip("A list of regions which are available by default, under no special conditions.")]
    public List<LevelRegionType> defaultRegions;
    [Tooltip("A list of regions which become available when the player reaches a certain point in tutorial.")]
    public List<RegionUnlockTutorialStage> regionsUnlockedByTutorial;
    [Tooltip("A list of regions which become available when the player reaches a certain value of the Endurance stat.")]
    public List<RegionUnlockValue> regionsUnlockedByEndurance;
    [Tooltip("A list of regions which become available when the player is able to reach a certain altitude (based on Altitude broom upgrade level).")]
    public List<RegionUnlockValue> regionsUnlockedByAltitude;

    [Header("Opponents")]
    [Tooltip("Number of opponents to generate.")]
    public int opponentsCount = 5;

    /// <summary>Level generation pipeline used for setting level generation parameters and generating it.</summary>
    protected LevelGenerationPipeline levelGenerator;

    /// <inheritdoc/>
    protected override void PrepareForInitialization_ReplacingAwake() {
    }

    /// <inheritdoc/>
    protected override void PrepareForInitialization_ReplacingStart() {
        levelGenerator = FindObjectOfType<LevelGenerationPipeline>();
    }

    /// <summary>
    /// Starts generating a level as a part of long initialization.
    /// </summary>
    /// <returns>Yields control back inbetween modules to allow other code to run (e.g. animation of loading screen).</returns>
    protected override IEnumerator InitializeAfterPreparation() {
        yield return GenerateLevel();
    }

    /// <inheritdoc/>
    protected override void UpdateAfterInitialization() {
    }

    /// <summary>
    /// Starts generating a level based on parameters obtained from <c>GetPlayerStats()</c>, <c>GetMaxAltitude()</c>, <c>GetRegionsAvailability()</c> and <c>GetRegionsVisited()</c>.
    /// </summary>
    /// <returns>Yields control back inbetween modules to allow other code to run (e.g. animation of loading screen).</returns>
    protected IEnumerator GenerateLevel() {
        // Generate level (terrain + track)
        SetLevelGeneratorParametersFromPlayerState(GetPlayerStats(), GetMaxAltitude(),
            GetRegionsAvailability(), GetRegionsVisited());
        yield return levelGenerator.GenerateLevel();
    }

    /// <summary>
    /// Gets current stats values which can be used to compute parameters for level generation.
    /// </summary>
    /// <returns>Current stats values.</returns>
    protected virtual PlayerStats GetPlayerStats() => PlayerState.Instance.CurrentStats;

    /// <summary>
    /// Gets current maximum altitude to which the broom can get. It can be then used to parametrize level generation.
    /// </summary>
    /// <returns>Maximum altitude possible.</returns>
    protected virtual float GetMaxAltitude() => PlayerState.Instance.maxAltitude;

    /// <summary>
    /// Gets a dictionary which contains <c>bool</c> value for each region in the game indicating whether it is currently available and can be used for level generation.
    /// </summary>
    /// <returns>Dictionary indicating regions' availability for level generation.</returns>
    protected virtual Dictionary<LevelRegionType, bool> GetRegionsAvailability() {
        // Update regions availability first
        UpdateRegionsAvailability();
        return PlayerState.Instance.regionsAvailability;
    }

    /// <summary>
    /// Gets a dictionary which contains <c>bool</c> value for each region in the game indicating whether it has already been visited by the player (i.e. player has finished a race going through that region).
    /// </summary>
    /// <returns>Dictionary indicating whether a regions has been visited or not.</returns>
    protected virtual Dictionary<LevelRegionType, bool> GetRegionsVisited() {
        return PlayerState.Instance.regionsVisited;
    }

    /// <summary>
    /// Prepares a list of terrain regions which are currently available and should be used in the generated level.
    /// Also ensures there is at most one unvisited region.
    /// </summary>
    /// <returns>List of terrain regions to be used in the generated level.</returns>
    protected virtual List<LevelRegionType> ChooseTerrainRegionsForLevel() {
        List<LevelRegionType> chosenRegions = new();
        List<LevelRegionType> unvisitedTerrainRegions = new();
        List<LevelRegionType> otherAvailableTerrainRegions = new();
        bool unvisitedAvailableTrackRegionExists = false;
        // Prepare lists of available terrain regions
        foreach (var terrainRegion in levelGenerator.terrainRegions) {
            // Default regions are chosen automatically
            if (defaultRegions.Contains(terrainRegion.regionType)) chosenRegions.Add(terrainRegion.regionType);
            // Then consider only available regions
            else if (PlayerState.Instance.regionsAvailability[terrainRegion.regionType]) {
                if (PlayerState.Instance.regionsVisited.TryGetValue(terrainRegion.regionType, out bool isVisited) && isVisited) // region has been visited
                    otherAvailableTerrainRegions.Add(terrainRegion.regionType);
                else unvisitedTerrainRegions.Add(terrainRegion.regionType);
            }
        }
        // Check if there is any track region which is available and hasn't been visited yet
        foreach (var trackRegion in levelGenerator.trackRegions) {
            if (PlayerState.Instance.regionsAvailability[trackRegion.regionType] // available
                && (!PlayerState.Instance.regionsVisited.TryGetValue(trackRegion.regionType, out bool isVisited) || !isVisited)) { // and not visited
                unvisitedAvailableTrackRegionExists = true;
                break;
            }
        }
        // Add one random unvisited terrain region (but only if there is no new track region, to make sure there is at most one new region)
        if (!unvisitedAvailableTrackRegionExists && unvisitedTerrainRegions.Count > 0) {
            chosenRegions.Add(unvisitedTerrainRegions[Random.Range(0, unvisitedTerrainRegions.Count)]);
        }
        // Add some other regions randomly, so that a desirable number of regions is chosen at maximum
        while (chosenRegions.Count < regionCountInLevel && otherAvailableTerrainRegions.Count > 0) {
            int randomIndex = Random.Range(0, otherAvailableTerrainRegions.Count);
            chosenRegions.Add(otherAvailableTerrainRegions[randomIndex]);
            otherAvailableTerrainRegions.RemoveAt(randomIndex);
        }
        return chosenRegions;
    }

    /// <summary>
    /// Sets level generator parameters, directly in <c>LevelGenerationPipeline</c> and also in individual modules.
    /// </summary>
    /// <param name="stats">Current stats values from which some parameters are computed.</param>
    /// <param name="maxAltitude">Maximum altitude to which the broom can get.</param>
    /// <param name="regionsAvailability">Dictionary indicating regions' availability for level generation.</param>
    /// <param name="regionsVisited">Dictionary indicating whether a regions has been visited or not.</param>
    protected void SetLevelGeneratorParametersFromPlayerState(PlayerStats stats, float maxAltitude, Dictionary<LevelRegionType, bool> regionsAvailability, Dictionary<LevelRegionType, bool> regionsVisited) {
        // Compute parameters based on the given player stats
        // ... number of checkpoints from Endurance
        int numOfCheckpoints = Mathf.RoundToInt(Mathf.Lerp(initialNumberOfCheckpoints, finalNumberOfCheckpoints, stats.endurance / 100f));
        // ... maximum direction change from Dexterity
        Vector2 directionChange = Vector2.Lerp(initialDirectionChange, finalDirectionChange, stats.dexterity / 100f);
        // ... hoop scale from Precision
        float hoopScale = Mathf.Lerp(initialHoopScale, finalHoopScale, stats.precision / 100f);
        // ... distance between adjacent hoops from Speed
        Vector2 distanceRange = Vector2.Lerp(initialHoopDistanceRange, finalHoopDistanceRange, stats.speed / 100f);
        // ... available regions + chosen terrain regions
        List<LevelRegionType> chosenTerrainRegions = ChooseTerrainRegionsForLevel(); // these will be used in the level
        // Set parameters (make sure to set all modules of the same type, if there are multiple) 
        // ... regions
        levelGenerator.terrainRegionsToInclude = chosenTerrainRegions;
        levelGenerator.regionsAvailability = regionsAvailability;
        levelGenerator.regionsVisited = regionsVisited;
        // ... track generation
        foreach (var module in levelGenerator.GetComponents<TrackGenerationBase>()) {
            module.maxAltitude = maxAltitude;
            module.numberOfCheckpoints = numOfCheckpoints;
            module.maxDirectionChangeAngle = directionChange;
            module.distanceRange = distanceRange;
        }
        // ... track terrain height postprocessing
        foreach (var module in levelGenerator.GetComponents<TrackTerrainHeightPostprocessing>())
            module.maxAltitude = maxAltitude;
        // ... track elements
        foreach (var module in levelGenerator.GetComponents<TrackObjectsPlacement>())
            module.hoopScale = hoopScale;
        foreach (var module in levelGenerator.GetComponents<MaximumAngleCorrection>())
            module.maxAngle = directionChange.x;
        foreach (var module in levelGenerator.GetComponents<OpponentsGeneration>())
            module.opponentsCount = opponentsCount;
    }

    // For each region in the game, checks whether it is currently available (based on its specific conditions), or not, and updates PlayerState accordingly
    private void UpdateRegionsAvailability() {
        // ... default available regions
        foreach (var region in defaultRegions)
            PlayerState.Instance.SetRegionAvailability(region, true);
        // ... available regions from tutorial
        foreach (var regionFromTutorial in regionsUnlockedByTutorial)
            PlayerState.Instance.SetRegionAvailability(regionFromTutorial.region, Tutorial.Instance.CurrentStage > regionFromTutorial.tutorialStage);
        // ... available regions from Endurance
        foreach (var regionWithValue in regionsUnlockedByEndurance)
            PlayerState.Instance.SetRegionAvailability(regionWithValue.region, PlayerState.Instance.CurrentStats.endurance >= regionWithValue.minValue);
        // ... available regions from max altitude
        foreach (var regionWithValue in regionsUnlockedByAltitude)
            PlayerState.Instance.SetRegionAvailability(regionWithValue.region, PlayerState.Instance.maxAltitude >= regionWithValue.minValue);
    }

}

/// <summary>
/// A class pairing a region with a specific minimum value (e.g. max altitude, endurance) which needs to be reached for the region to be unlocked.
/// </summary>
[System.Serializable]
public class RegionUnlockValue {
    [Tooltip("Region unlocked by a specific value (e.g. max altitude, endurance).")]
    public LevelRegionType region;
    [Tooltip("If the current value is greater then this value, the region becomes available.")]
    public int minValue;
}

/// <summary>
/// A class pairing a region with a specific tutorial stage which needs to be reached for the region to be unlocked.
/// </summary>
[System.Serializable]
public class RegionUnlockTutorialStage {
    [Tooltip("Region unlocked by a specific tutorial stage.")]
    public LevelRegionType region;
    [Tooltip("If the player gets into this tutorial stage, the region becomes available.")]
    public TutorialStage tutorialStage;
}
