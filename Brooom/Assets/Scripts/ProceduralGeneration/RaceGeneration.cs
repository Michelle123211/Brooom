using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceGeneration : MonoBehaviourLongInitialization {

    [Header("Level length (Endurance)")]
    [Tooltip("How many checkpoints should be generated when the player's Endurance stat is 0.")]
    public int initialNumberOfCheckpoints = 4;
    [Tooltip("How many checkpoints should be generated when the player's Endurance stat is 100.")]
    public int finalNumberOfCheckpoints = 20;

    [Header("Direction change (Dexterity)")]
    [Tooltip("Maximum angle between two consecutive hoops in the X (up/down) and Y (left/right) axis when the player's Dexterity stat is 0.")]
    public Vector2 initialDirectionChange = new Vector2(10, 20);
    [Tooltip("Maximum angle between two consecutive hoops in the X (up/down) and Y (left/right) axis when the player's Dexterity stat is 100.")]
    public Vector2 finalDirectionChange = new Vector2(30, 45);

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
    public List<LevelRegionType> defaultRegions;
    public List<RegionUnlockTutorialStage> regionsUnlockedByTutorial;
    public List<RegionUnlockValue> regionsUnlockedByEndurance;
    public List<RegionUnlockValue> regionsUnlockedByAltitude;


    protected LevelGenerationPipeline levelGenerator;

    protected override void PrepareForInitialization_ReplacingAwake() {
    }

    protected override void PrepareForInitialization_ReplacingStart() {
        levelGenerator = FindObjectOfType<LevelGenerationPipeline>();
    }

    protected override IEnumerator InitializeAfterPreparation() {
        yield return GenerateLevel();
    }

    protected override void UpdateAfterInitialization() {
    }

    protected IEnumerator GenerateLevel() {
        // Generate level (terrain + track)
        SetLevelGeneratorParameters();
        yield return levelGenerator.GenerateLevel();
    }

    private void SetLevelGeneratorParameters() {
        // Compute parameters based on player's stats
        // ... number of checkpoints from Endurance
        int numOfCheckpoints = Mathf.RoundToInt(Mathf.Lerp(initialNumberOfCheckpoints, finalNumberOfCheckpoints, PlayerState.Instance.CurrentStats.endurance / 100f));
        // ... maximum direction change from Dexterity
        Vector2 directionChange = Vector2.Lerp(initialDirectionChange, finalDirectionChange, PlayerState.Instance.CurrentStats.dexterity / 100f);
        // ... hoop scale from Precision
        float hoopScale = Mathf.Lerp(initialHoopScale, finalHoopScale, PlayerState.Instance.CurrentStats.precision / 100f);
        // ... distance between adjacent hoops from Speed
        Vector2 distanceRange = Vector2.Lerp(initialHoopDistanceRange, finalHoopDistanceRange, PlayerState.Instance.CurrentStats.speed / 100f);
        // ... available regions + chosen terrain regions
        UpdateRegionsAvailability();
        List<LevelRegionType> chosenTerrainRegions = ChooseTerrainRegionsForLevel(); // these will be used in the level
        // Set parameters
        levelGenerator.terrainRegionsToInclude = chosenTerrainRegions;
        levelGenerator.regionsAvailability = PlayerState.Instance.regionsAvailability;
        levelGenerator.regionsVisited = PlayerState.Instance.regionsVisited;
        TrackPointsGenerationRandomWalk trackGenerator = levelGenerator.GetComponent<TrackPointsGenerationRandomWalk>();
        trackGenerator.numberOfCheckpoints = numOfCheckpoints;
        trackGenerator.maxDirectionChangeAngle = directionChange;
        trackGenerator.distanceRange = distanceRange;
        levelGenerator.GetComponent<TrackObjectsPlacement>().hoopScale = hoopScale;
        levelGenerator.GetComponent<MaximumAngleCorrection>().maxAngle = directionChange.x;
        levelGenerator.GetComponent<OpponentsGeneration>().opponentsCount = 5; // TODO: Change this number if necessary, in the future
    }

    private void UpdateRegionsAvailability() {
        // ... default available regions
        foreach (var region in defaultRegions)
            PlayerState.Instance.SetRegionAvailability(region, true);
        // ... available regions from tutorial
        foreach (var regionFromTutorial in regionsUnlockedByTutorial)
            PlayerState.Instance.SetRegionAvailability(regionFromTutorial.region, Tutorial.Instance.CurrentStage >= regionFromTutorial.tutorialStage);
        // ... available regions from Endurance
        foreach (var regionWithValue in regionsUnlockedByEndurance)
            PlayerState.Instance.SetRegionAvailability(regionWithValue.region, PlayerState.Instance.CurrentStats.endurance >= regionWithValue.minValue);
        // ... available regions from max altitude
        foreach (var regionWithValue in regionsUnlockedByAltitude)
            PlayerState.Instance.SetRegionAvailability(regionWithValue.region, PlayerState.Instance.maxAltitude >= regionWithValue.minValue);
    }

    private List<LevelRegionType> ChooseTerrainRegionsForLevel() {
        List<LevelRegionType> chosenRegions = new List<LevelRegionType>();
        List<LevelRegionType> unvisitedTerrainRegions = new List<LevelRegionType>();
        List<LevelRegionType> otherAvailableTerrainRegions = new List<LevelRegionType>();
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

}
