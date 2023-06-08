using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackTerrainHeightPostprocessing : LevelGeneratorModule {
	// TODO: Set Y coordinate of the points above terrain
	// TODO: Limit the Y coordinate according to the maximum altitude of the broom
	// TODO: Adjust the terrain height if the track point is beneath it (affect only some neighbourhood, e.g. sphere)
	public override void Generate(LevelRepresentation level) {
		AdjustPointsHeightAboveTerrain(level);
	}

	private void AdjustPointsHeightAboveTerrain(LevelRepresentation level) {
		// TODO: Set Y coordinate of the points above terrain
		foreach (var trackPoint in level.track) {
			if (trackPoint.position.y < level.terrain[trackPoint.gridCoords.x, trackPoint.gridCoords.y].position.y + 2) {
				trackPoint.position.y = level.terrain[trackPoint.gridCoords.x, trackPoint.gridCoords.y].position.y + 2;
				// TODO: Look to some neighbourhood, if there is higher terrain very close, move the point up

				// TODO: Distribute any change to 2 or 3 adjacent points in both sides as well to smooth it out
			}
		}
	}
}
