using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentLevelGeneration : MonoBehaviour {
    void Start() {
        LevelGenerationPipeline levelGenerator = GetComponent<LevelGenerationPipeline>();
        // Set parameters
        Dictionary<LevelRegionType, bool> regionsAvailability = new Dictionary<LevelRegionType, bool>();
        regionsAvailability[LevelRegionType.EnchantedForest] = true;
        regionsAvailability[LevelRegionType.AboveWater] = true;
        regionsAvailability[LevelRegionType.AridDesert] = true;
        regionsAvailability[LevelRegionType.SnowyMountain] = true;
        levelGenerator.regionsAvailability = regionsAvailability;
        // Generate level
        levelGenerator.GenerateLevel();
    }
}
