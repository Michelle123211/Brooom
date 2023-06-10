using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackTerrainHeightPostprocessing : LevelGeneratorModule {

	[Tooltip("Each hoop's Y coordinate is adjusted according to the maximum terran height in a close neighbourhood of the given radius.")]
	public int hoopHeightAreaRadius = 10;

	// TODO: Limit the Y coordinate according to the maximum altitude of the broom
	// TODO: Adjust the terrain height if the track point is beneath it (affect only some neighbourhood, e.g. sphere) - may not be necessary
	public override void Generate(LevelRepresentation level) {
		AdjustPointsHeightAboveTerrain(level);
	}

	private void AdjustPointsHeightAboveTerrain(LevelRepresentation level) {
		// Set Y coordinate of the points above terrain
		foreach (var trackPoint in level.track) {
			// Get maximum height on some small neighbourhood
			float maxHeight = trackPoint.position.y;
			int x, y;
			for (int i = -hoopHeightAreaRadius; i <= hoopHeightAreaRadius; i++) {
				x = trackPoint.gridCoords.x + i;
				if (x < 0 || x >= level.pointCount.x) continue;
				for (int j = -hoopHeightAreaRadius; j <= hoopHeightAreaRadius; j++) {
					y = trackPoint.gridCoords.y + j;
					if (y < 0 || y >= level.pointCount.y) continue;
					if (level.terrain[x, y].position.y > maxHeight)
						maxHeight = level.terrain[x, y].position.y;
				}
			}
			// Use this height (+1) for the hoop
			trackPoint.position.y = maxHeight + 1;
			// TODO: Distribute any change to 2 or 3 adjacent points in both sides as well to smooth it out if hoops are too close to each other
		}
	}
}
