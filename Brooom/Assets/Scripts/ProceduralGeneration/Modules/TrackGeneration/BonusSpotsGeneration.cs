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
        for (int i = 0; i < level.track.Count - 1; i++) {
            for (int j = 1; j <= countBetweenHoops; j++) {
                // Uniformly divide the space between two track points
                Vector3 pointA = level.track[i].position;
                Vector3 pointB = level.track[i + 1].position;
                float fraction = (float)j / (countBetweenHoops + 1);
                level.bonuses.Add(new BonusSpot(pointA + fraction * (pointB - pointA), i, fraction));
            }
        }
    }

    private void ComputeReferenceGridPoints(LevelRepresentation level) {
        int i, j;
        Vector3 topleft = level.terrain[0, 0].position; // position of the top-left grid point
        float offset = level.pointOffset; // distance between adjacent grid points
        foreach (var bonusSpot in level.bonuses) {
            // Compute the closest terrain grid point
            i = Mathf.RoundToInt(Mathf.Abs(bonusSpot.position.x - topleft.x) / offset);
            j = Mathf.RoundToInt(Mathf.Abs(bonusSpot.position.z - topleft.z) / offset);
            bonusSpot.gridCoords = new Vector2Int(i, j);
        }
    }
}