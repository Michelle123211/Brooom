using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickRaceGeneration : RaceGeneration {

	/// <summary>
	/// Chooses random terrain regions which should be used in the generated level from all available terrain regions.
	/// Also makes sure no more than desired number of regions are selected (denoted by <c>regionCountInLevel</c> data field's value.
	/// </summary>
	/// <returns></returns>
	protected override List<LevelRegionType> ChooseTerrainRegionsForLevel() {
        List<LevelRegionType> availableTerrainRegions = new List<LevelRegionType>();
        // Prepare list of available terrain regions
        foreach (var terrainRegion in levelGenerator.terrainRegions) {
            if (PlayerState.Instance.regionsAvailability[terrainRegion.regionType])
                availableTerrainRegions.Add(terrainRegion.regionType);
        }
        // Choose regions randomly, so that a desirable number of regions is chosen at maximum
        List<LevelRegionType> chosenRegions = new List<LevelRegionType>();
        while (chosenRegions.Count < regionCountInLevel && availableTerrainRegions.Count > 0) {
            int randomIndex = Random.Range(0, availableTerrainRegions.Count);
            chosenRegions.Add(availableTerrainRegions[randomIndex]);
            availableTerrainRegions.RemoveAt(randomIndex);
        }
        return chosenRegions;
    }

}
