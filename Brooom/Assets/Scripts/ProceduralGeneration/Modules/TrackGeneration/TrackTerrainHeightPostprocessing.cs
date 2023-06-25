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
			float height = FindMaximumHeightInNeighbourhood(level, trackPoint.gridCoords);
			if (trackPoint.position.y < height)
				trackPoint.position.y = height;
			// Limit the Y coordinate according to the maximum altitude of the broom
			trackPoint.position.y = Mathf.Clamp(trackPoint.position.y, 0, PlayerState.Instance.maxAltitude);
			// TODO: Distribute any change to 2 or 3 adjacent points in both sides as well to smooth it out - if hoops are too close to each other
		}

		// Change Y coordinate of each bonus spot according to the terrain height and adjacent hoops height
		foreach (var bonus in level.bonuses) {
			// Get height according to the terrain
			float heightTerrain = FindMaximumHeightInNeighbourhood(level, bonus.gridCoords);
			bonus.position.y = Mathf.Max(bonus.position.y, heightTerrain);
			// Get height according to the adjacent hoops
			float heightHoop = Mathf.Lerp(level.track[bonus.previousHoopIndex].position.y, level.track[bonus.previousHoopIndex + 1].position.y, bonus.distanceFraction);
			// Take maximum and limit it according to the maximum altitude of the broom
			bonus.position.y = Mathf.Clamp(Mathf.Max(bonus.position.y, heightHoop), 0, PlayerState.Instance.maxAltitude);
		}

		// Change Y coordinate of the player's start position according to the terrain height
		Vector3 topleft = level.terrain[0, 0].position; // position of the top-left grid point
		float offset = level.pointOffset; // distance between adjacent grid points
		int x = Mathf.RoundToInt(Mathf.Abs(level.playerStartPosition.x - topleft.x) / offset); // closest terrain grid point coordinates
		int z = Mathf.RoundToInt(Mathf.Abs(level.playerStartPosition.z - topleft.z) / offset);
		float computedHeight = FindMaximumHeightInNeighbourhood(level, new Vector2Int(x, z));
		// Take maximum and limit it according to the maximum altitude of the broom
		level.playerStartPosition.y = Mathf.Clamp(Mathf.Max(level.playerStartPosition.y, computedHeight), 0, PlayerState.Instance.maxAltitude);
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

	private float FindMaximumHeightInNeighbourhood(LevelRepresentation level, Vector2Int gridCoords) {
		// Get maximum height in some small neighbourhood
		float maxHeight = float.MinValue;
		float height;
		int x, y;
		for (int i = -hoopHeightAreaRadius; i <= hoopHeightAreaRadius; i++) {
			x = gridCoords.x + i;
			if (x < 0 || x >= level.pointCount.x) continue; // out of bounds check
			for (int j = -hoopHeightAreaRadius; j <= hoopHeightAreaRadius; j++) {
				y = gridCoords.y + j;
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