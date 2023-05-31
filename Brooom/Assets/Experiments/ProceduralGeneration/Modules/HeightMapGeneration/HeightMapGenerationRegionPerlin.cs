using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Generates height map using octaved Perlin noise with different parameters for different regions
public class HeightMapGenerationRegionPerlin : LevelGeneratorModule {

	[Tooltip("Different Parlin noise parameters for different regions.")]
	public List<RegionHeightMapParameters> regionParameters;

	private Dictionary<MapRegionType, RegionHeightMapParameters> regionParamsDict;
	private Dictionary<MapRegionType, Vector2Int> regionRandomOffsets;
	private RegionHeightMapParameters defaultParams;
	private Vector2Int defaultrandomOffset;

	public override void Generate(LevelRepresentation level) {
		// Initialize Dictionaries of region parameters and random offsets for easier access
		// Select default values (used when the region assigned to the point does not have parameters specified)
		InitializeValues();

		// Remember the minimum and maximum heights (for future use in remapping the range)
		float currMinHeight = float.MaxValue;
		float currMaxHeight = float.MinValue;
		// Determine height of each point using octaved Perlin noise with parameters specific for the region
		for (int x = 0; x < level.pointCount.x; x++) {
			for (int y = 0; y < level.pointCount.y; y++) {
				// Look at the assigned region and get its parameters
				RegionHeightMapParameters regionParams;
				if (!regionParamsDict.TryGetValue(level.terrain[x, y].region, out regionParams))
					regionParams = defaultParams;
				Vector2Int offset;
				if (!regionRandomOffsets.TryGetValue(level.terrain[x, y].region, out offset))
					offset = defaultrandomOffset;
				// Determine height with the given parameters
				float height = 0;
				float scale = 1;
				float frequency = regionParams.octaveParams.initialFrequency;
				// Add contributions from each octave
				for (int octave = 0; octave < regionParams.octaveParams.numberOfOctaves; octave++) {
					height += Mathf.PerlinNoise(
						(offset.x + x * level.pointOffset) * frequency,
						(offset.y + y * level.pointOffset) * frequency) * scale; // multiplied by pointOffset to make the overall shape of terrain not dependent on pointOffset
					frequency *= regionParams.octaveParams.frequencyFactor;
					scale *= regionParams.octaveParams.scaleFactor;
				}
				level.terrain[x, y].position.y = height;
				// Update minimum and maximum heights
				if (height < currMinHeight) currMinHeight = height;
				if (height > currMaxHeight) currMaxHeight = height;
			}
		}

		// Remap the range to the one according to the parameters and current height range
		RemapHeightsInEachRegion(level, currMinHeight, currMaxHeight);
	}

	private void InitializeValues() {
		// Initialize Dictionaries of region parameters and random offsets for easier access
		regionParamsDict = new Dictionary<MapRegionType, RegionHeightMapParameters>();
		regionRandomOffsets = new Dictionary<MapRegionType, Vector2Int>();
		if (regionParameters != null) {
			foreach (var regionParams in regionParameters) {
				regionParamsDict.Add(regionParams.region, regionParams);
				regionRandomOffsets.Add(regionParams.region, new Vector2Int(Random.Range(0, 1000), Random.Range(0, 1000)));
			}
		}
		// Select default values (used when the region assigned to the point does not have parameters specified)
		defaultParams = new RegionHeightMapParameters();
		defaultrandomOffset = new Vector2Int(Random.Range(0, 1000), Random.Range(0, 1000));
	}

	private void RemapHeightsInEachRegion(LevelRepresentation level, float currentMinHeight, float currentMaxHeight) {
		// Remap the range to the one according to the parameters and current height range
		float currHeightRange = currentMaxHeight - currentMinHeight;
		float levelHeightRange = level.heightRange.y - level.heightRange.x;
		for (int x = 0; x < level.pointCount.x; x++) {
			for (int y = 0; y < level.pointCount.y; y++) {
				float newHeight = level.terrain[x, y].position.y;
				// Remap from (currMinHeight, currMaxHeight) to (0,1)
				newHeight = (newHeight - currentMinHeight) / currHeightRange;
				// Look at the assigned region and get its parameters
				RegionHeightMapParameters regionParams;
				if (!regionParamsDict.TryGetValue(level.terrain[x, y].region, out regionParams))
					regionParams = defaultParams;
				// Remap from (0, 1) to (regionParams.heightRange.x, regionParams.heightRange.y)
				newHeight = newHeight * (regionParams.heightRange.y - regionParams.heightRange.x) + regionParams.heightRange.x;
				// Remap from (0,1) to (minimumHeight, maximumHeight)
				newHeight = newHeight * (levelHeightRange) + level.heightRange.x;
				level.terrain[x, y].position.y = newHeight;
			}
		}
	}
}


[System.Serializable]
public class RegionHeightMapParameters {
	public MapRegionType region;

	public PerlinNoiseOctaveParameters octaveParams = new PerlinNoiseOctaveParameters();

	//[Header("Height")]
	[Tooltip("What interval of the global height range this lies in (between 0 and 1).")]
	public Vector2 heightRange = new Vector2(0, 1);
}