using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentLevelGeneration : MonoBehaviour {

    LevelGenerationPipeline levelGenerator;
    bool canGenerateAnotherLevel = true;

    private void OnLevelGenerated(LevelRepresentation level) {
        canGenerateAnotherLevel = true;
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
        //regionsAvailability[LevelRegionType.AridDesert] = true;
        regionsAvailability[LevelRegionType.SnowyMountain] = true;
        //regionsAvailability[LevelRegionType.BloomingMeadow] = true;
        //regionsAvailability[LevelRegionType.StormyArea] = true;
        levelGenerator.regionsAvailability = regionsAvailability;
        levelGenerator.regionsVisited = regionsAvailability; // all available regions have been visited
        levelGenerator.terrainRegionsToInclude = new List<LevelRegionType> { LevelRegionType.AboveWater, LevelRegionType.SnowyMountain };
        PlayerState.Instance.maxAltitude = 30;
        // Generate level
        GenerateLevel();
    }

	private void Update() {
        if (Input.GetKeyDown(KeyCode.Space) && canGenerateAnotherLevel)
            GenerateLevel();
	}

}
