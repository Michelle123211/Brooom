using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Generates height map using octaved Perlin noise with different parameters for different regions
public class HeightMapGenerationRegionPerlin : LevelGeneratorModule {

	[Header("Generator parameters")]

	[Tooltip("Different Parlin noise parameters for different regions.")]
	public List<RegionHeightMapParameters> regionParameters;

	private Dictionary<MapRegionType, OctavedPerlinNoise> regionPerlinNoise;
	private Dictionary<MapRegionType, Vector2> regionHeightRange;
	private OctavedPerlinNoise defaultPerlinNoise;
	private Vector2 defaultHeightRange = Vector2.up;

	public override void Generate(LevelRepresentation level) {
		// Initialize Dictionaries of octaved perlin noises and region heights for easier access
		// Select default values (used when the region assigned to the point does not have parameters specified)
		InitializeValues();

		// Remember the minimum and maximum heights (for future use in remapping the range)
		float currMinHeight = float.MaxValue;
		float currMaxHeight = float.MinValue;
		// Determine height of each point using octaved Perlin noise with parameters specific for the region
		for (int x = 0; x < level.pointCount.x; x++) {
			for (int y = 0; y < level.pointCount.y; y++) {
				// Look at the assigned region and get its noise function
				OctavedPerlinNoise noise;
				if (!regionPerlinNoise.TryGetValue(level.terrain[x, y].region, out noise))
					noise = defaultPerlinNoise;
				// Determine height with the given parameters
				float height = noise.GetValue(x * level.pointOffset, y * level.pointOffset); // multiplied by pointOffset to make the overall shape of terrain not dependent on pointOffset
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
		// Initialize Dictionaries of octaved perlin noises and region heights for easier access
		regionPerlinNoise = new Dictionary<MapRegionType, OctavedPerlinNoise>();
		regionHeightRange = new Dictionary<MapRegionType, Vector2>();
		if (regionParameters != null) {
			foreach (var regionParams in regionParameters) {
				OctavedPerlinNoise noise = new OctavedPerlinNoise(Random.Range(0, 1000), Random.Range(0, 1000), regionParams.octaveParams);
				regionPerlinNoise.Add(regionParams.region, noise);
				regionHeightRange.Add(regionParams.region, regionParams.heightRange);
			}
		}
		// Select default values (used when the region assigned to the point does not have parameters specified)
		defaultPerlinNoise = new OctavedPerlinNoise(Random.Range(0, 1000), Random.Range(0, 1000), new PerlinNoiseOctaveParameters());
		defaultHeightRange = Vector2.up;
	}

	private void RemapHeightsInEachRegion(LevelRepresentation level, float currentMinHeight, float currentMaxHeight) {
		// Remap the range to the one according to the parameters and current height range
		for (int x = 0; x < level.pointCount.x; x++) {
			for (int y = 0; y < level.pointCount.y; y++) {
				// Look at the assigned region and get its parameters
				Vector2 heightRange;
				if (!regionHeightRange.TryGetValue(level.terrain[x, y].region, out heightRange))
					heightRange = defaultHeightRange;
				float newHeight = level.terrain[x, y].position.y;
				// Remap from (currMinHeight, currMaxHeight) to (regionParams.heightRange.x, regionParams.heightRange.y)
				newHeight = Utils.RemapRange(newHeight, currentMinHeight, currentMaxHeight, heightRange.x, heightRange.y);
				// Remap from (0,1) to (level.heightRange.x, level.heightRange.y)
				level.terrain[x, y].position.y = Utils.RemapRange(newHeight, 0, 1, level.heightRange.x, level.heightRange.y);
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