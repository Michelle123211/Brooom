using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackTerrainHeightPostprocessing : LevelGeneratorModule {

	[Tooltip("Each hoop's Y coordinate is adjusted according to the maximum terrain height in a close neighbourhood of the given radius.")]
	public int hoopHeightAreaRadius = 10;
	[Tooltip("Hoops and checkpoints should be placed in this minimum height above ground..")]
	public float defaultMinimumOffsetAboveGround = 1;
	[Tooltip("Some regions may prefer different minimum height above ground than the default one (e.g. larger for forest).")]
	public List<TrackInRegionHeight> minHeightOffsetOverrides;


	private Dictionary<MapRegionType, float> minimumOffsetAboveGround;

	public override void Generate(LevelRepresentation level) {
		PrepareRegionHeightDictionary(level);

		// Change Y coordinate of each track point according to the terrain height in a close neighbourhood, ensure minimum height above ground
		foreach (var trackPoint in level.track) {
			float height = FindMaximumHeightInNeighbourhood(level, trackPoint);
			if (trackPoint.position.y < height)
				trackPoint.position.y = height;
			// Limit the Y coordinate according to the maximum altitude of the broom
			trackPoint.position.y = Mathf.Clamp(trackPoint.position.y, 0, level.maxAltitude);
			// TODO: Distribute any change to 2 or 3 adjacent points in both sides as well to smooth it out - if hoops are too close to each other
		}
	}

	private void PrepareRegionHeightDictionary(LevelRepresentation level) {
		// Prepare Dictionary of minimum height for each region type
		minimumOffsetAboveGround = new Dictionary<MapRegionType, float>();
		foreach (var region in level.regions) {
			minimumOffsetAboveGround.Add(region.Key, defaultMinimumOffsetAboveGround);
		}
		// Override minimum height for specific regions
		if (minHeightOffsetOverrides != null) {
			foreach (var heightOverride in minHeightOffsetOverrides) {
				minimumOffsetAboveGround[heightOverride.region] = heightOverride.minimumHeightAboveGround;
			}
		}
	}

	private float FindMaximumHeightInNeighbourhood(LevelRepresentation level, TrackPoint trackPoint) {
		// Get maximum height in some small neighbourhood
		float maxHeight = float.MinValue;
		float height;
		int x, y;
		for (int i = -hoopHeightAreaRadius; i <= hoopHeightAreaRadius; i++) {
			x = trackPoint.gridCoords.x + i;
			if (x < 0 || x >= level.pointCount.x) continue; // out of bounds check
			for (int j = -hoopHeightAreaRadius; j <= hoopHeightAreaRadius; j++) {
				y = trackPoint.gridCoords.y + j;
				if (y < 0 || y >= level.pointCount.y) continue; // out of bounds check
				// Get terrain height + minimum offset above ground
				height = GetMinimumHeightAboveGround(level.terrain[x, y]);
				if (height > maxHeight)
					maxHeight = height;
			}
		}
		return maxHeight;
	}

	// Returns the minimum height a track point can be at (considering the terrain heigth and the minimum offset above ground)
	private float GetMinimumHeightAboveGround(TerrainPoint terrainPoint) {
		// Get the terrain height
		float minHeight = terrainPoint.position.y;
		// Add minimum offset above ground
		if (minimumOffsetAboveGround.TryGetValue(terrainPoint.region, out float minHeightOffset))
			minHeight += minHeightOffset;
		else
			minHeight += defaultMinimumOffsetAboveGround;
		return minHeight;
	}
}

[System.Serializable]
public class TrackInRegionHeight {
	public MapRegionType region = MapRegionType.NONE;
	public float minimumHeightAboveGround = 1;
}