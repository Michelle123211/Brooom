using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusSpotsGeneration : LevelGeneratorModule {
    [Tooltip("How many bonus spots should be between each pair of hoops.")]
    public int countBetweenHoops = 3;

    public override void Generate(LevelRepresentation level) {
        GenerateBonusSpots(level);
        ComputeReferenceGridPoints(level); // closest terrain grid point for each bonus spot
    }

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

    private void ComputeReferenceGridPoints(LevelRepresentation level) {
        foreach (var bonusSpot in level.bonuses) {
            bonusSpot.gridCoords = level.GetNearestGridPoint(bonusSpot.position);
        }
    }
}