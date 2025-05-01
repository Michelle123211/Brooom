using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for generating a level at the start of the Quick Race scene.
/// It extends the base class <c>RaceGeneration</c> to modify a way in which terrain regions for the level are selected
/// (randomly from all available ones, not prioritizing unvisited ones).
/// </summary>
public class QuickRaceGeneration : RaceGeneration {

	/// <summary>
	/// Chooses random terrain regions which should be used in the generated level from all available terrain regions.
	/// Also makes sure no more than desired number of regions are selected (denoted by <c>regionCountInLevel</c> data field's value).
	/// </summary>
	/// <returns>A list of regions chosen for the currently generated level.</returns>
	protected override List<LevelRegionType> ChooseTerrainRegionsForLevel() {
        List<LevelRegionType> availableTerrainRegions = new();
        // Prepare list of available terrain regions
        Dictionary<LevelRegionType, bool> regionsAvailability = GetRegionsAvailability();
        foreach (var terrainRegion in levelGenerator.terrainRegions) {
            if (regionsAvailability[terrainRegion.regionType])
                availableTerrainRegions.Add(terrainRegion.regionType);
        }
        // Choose regions randomly, so that a desirable number of regions is chosen at maximum
        List<LevelRegionType> chosenRegions = new();
        while (chosenRegions.Count < regionCountInLevel && availableTerrainRegions.Count > 0) {
            int randomIndex = Random.Range(0, availableTerrainRegions.Count);
            chosenRegions.Add(availableTerrainRegions[randomIndex]);
            availableTerrainRegions.RemoveAt(randomIndex);
        }
        return chosenRegions;
    }

}
