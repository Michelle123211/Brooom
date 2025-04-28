using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A level generator module responsible for generating bonus spots along the track in which bonuses are spawned.
/// Between each pair of hoops there is a given number of evenly spaced bonus spots.
/// However it doesn't determine which type of bonus will be assigned to each spot, if any at all.
/// </summary>
public class BonusSpotsGeneration : LevelGeneratorModule {

    [Tooltip("How many bonus spots should be between each pair of hoops.")]
    public int countBetweenHoops = 3;

    /// <summary>
    /// Generates a particular number of bonus spots evenly spaced between each pair of hoops. These spots may be later populated by a specific type of bonus.
    /// </summary>
    /// <param name="level"><inheritdoc/></param>
    public override void Generate(LevelRepresentation level) {
        GenerateBonusSpots(level);
        ComputeReferenceGridPoints(level); // closest terrain grid point for each bonus spot
    }

    // Generates bonus spots, evenly distributed between each pair of hoops
    private void GenerateBonusSpots(LevelRepresentation level) {
        // Generate spots for the bonuses
        level.bonuses = new List<BonusSpot>();
        for (int i = 0; i < level.Track.Count - 1; i++) {
            for (int j = 1; j <= countBetweenHoops; j++) {
                // Uniformly divide the space between two track points
                Vector3 pointA = level.Track[i].position;
                Vector3 pointB = level.Track[i + 1].position;
                float fraction = (float)j / (countBetweenHoops + 1);
                level.bonuses.Add(new BonusSpot(pointA + fraction * (pointB - pointA), i, fraction));
            }
        }
    }

    // Assigns the nearest terrain grid point to each bonus spot
    private void ComputeReferenceGridPoints(LevelRepresentation level) {
        foreach (var bonusSpot in level.bonuses) {
            bonusSpot.gridCoords = level.GetNearestGridPoint(bonusSpot.position);
        }
    }
}