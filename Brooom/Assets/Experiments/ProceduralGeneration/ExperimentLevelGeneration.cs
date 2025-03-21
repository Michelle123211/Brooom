using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentLevelGeneration : MonoBehaviour {

    LevelGenerationPipeline levelGenerator;
    bool canGenerateAnotherLevel = true;

    private void OnLevelGenerated(LevelRepresentation level) {
        canGenerateAnotherLevel = true;
        Debug.Log("Level generated.");
    }

    private void GenerateLevel() {
        canGenerateAnotherLevel = false;
        StartCoroutine(levelGenerator.GenerateLevel());
    }

	private void Awake() {
        levelGenerator = GetComponent<LevelGenerationPipeline>();
    }

	void Start() {
        levelGenerator.onLevelGenerated += OnLevelGenerated;
        // Set parameters
        Dictionary<LevelRegionType, bool> regionsAvailability = new Dictionary<LevelRegionType, bool>();
        regionsAvailability[LevelRegionType.AboveWater] = true;
        //regionsAvailability[LevelRegionType.EnchantedForest] = true;
        regionsAvailability[LevelRegionType.AridDesert] = true;
        //regionsAvailability[LevelRegionType.SnowyMountain] = true;
        //regionsAvailability[LevelRegionType.BloomingMeadow] = true;
        //regionsAvailability[LevelRegionType.StormyArea] = true;
        levelGenerator.regionsAvailability = regionsAvailability;
        levelGenerator.regionsVisited = regionsAvailability; // all available regions have been visited
        levelGenerator.terrainRegionsToInclude = new List<LevelRegionType> { LevelRegionType.AboveWater, LevelRegionType.AridDesert };
        //PlayerState.Instance.maxAltitude = 30;
        // Set module parameters
        TrackPointsGenerationRandomWalk trackGenerator = levelGenerator.GetComponent<TrackPointsGenerationRandomWalk>();
        trackGenerator.numberOfCheckpoints = 4; //20;
        trackGenerator.maxDirectionChangeAngle = new Vector2(10, 20); //new Vector2(30, 45);
        trackGenerator.distanceRange = new Vector2(40, 50); //new Vector2(80, 100);
        levelGenerator.GetComponent<TrackObjectsPlacement>().hoopScale = 1f; //0.4f;
        levelGenerator.GetComponent<MaximumAngleCorrection>().maxAngle = new Vector2(10, 20).x; //new Vector2(30, 45).x;
        // Generate level
        GenerateLevel();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space) && canGenerateAnotherLevel)
            GenerateLevel();
	}

}
