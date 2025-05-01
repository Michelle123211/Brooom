using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A level generator module responsible for generating a height map using several octaves of Perlin noise.
/// However different parameters are used for different regions (e.g., minimum and maximum height, number of octaves).
/// </summary>
public class HeightMapGenerationRegionPerlin : LevelGeneratorModule {

	[Tooltip("Different octaved Perlin noise parameters for different regions.")]
	public List<RegionHeightMapParameters> regionParameters;

	private Dictionary<LevelRegionType, OctavedPerlinNoise> regionPerlinNoise;
	private Dictionary<LevelRegionType, Vector2> regionHeightRange;
	private OctavedPerlinNoise defaultPerlinNoise; // used for regions for which parameters have not been specified
	private Vector2 defaultHeightRange = Vector2.up;


	/// <summary>
	/// Assigns height to each terrain point based on an octaved Perlin noise (with different parameters for different regions).
	/// </summary>
	/// <param name="level"><inheritdoc/></param>
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
				if (!regionPerlinNoise.TryGetValue(level.Terrain[x, y].region, out noise))
					noise = defaultPerlinNoise;
				// Determine height with the given parameters
				float height = noise.GetValue(x * level.pointOffset, y * level.pointOffset); // multiplied by pointOffset to make the overall shape of terrain not dependent on pointOffset
				level.Terrain[x, y].position.y = height;
				// Update minimum and maximum heights
				if (height < currMinHeight) currMinHeight = height;
				if (height > currMaxHeight) currMaxHeight = height;
			}
		}

		// Remap the range to the one according to the parameters and current height range
		RemapHeightsInEachRegion(level, currMinHeight, currMaxHeight);
	}

	// Initializes Dictionaries of octaved perlin noises and region heights for easier access
	private void InitializeValues() {
		// Initialize Dictionaries of octaved perlin noises and region heights for easier access
		regionPerlinNoise = new Dictionary<LevelRegionType, OctavedPerlinNoise>();
		regionHeightRange = new Dictionary<LevelRegionType, Vector2>();
		if (regionParameters != null) {
			foreach (var regionParams in regionParameters) {
				OctavedPerlinNoise noise = new OctavedPerlinNoise(Random.Range(0, 1000), Random.Range(0, 1000), regionParams.octaveParams);
				regionPerlinNoise.Add(regionParams.region, noise);
				regionHeightRange.Add(regionParams.region, regionParams.heightRange);
			}
		}
		// Select default values (used when the region assigned to the point does not have parameters specified)
		defaultPerlinNoise = new OctavedPerlinNoise(Random.Range(0, 1000), Random.Range(0, 1000), new PerlinNoiseOctaveParameters());
		defaultHeightRange = new Vector2(0, 6);
	}

	// Remaps the terrain height from the original range to a new range based on the region
	private void RemapHeightsInEachRegion(LevelRepresentation level, float currentMinHeight, float currentMaxHeight) {
		// Remap the range to the one according to the parameters and current height range
		for (int x = 0; x < level.pointCount.x; x++) {
			for (int y = 0; y < level.pointCount.y; y++) {
				// Look at the assigned region and get its parameters
				Vector2 heightRange;
				if (!regionHeightRange.TryGetValue(level.Terrain[x, y].region, out heightRange))
					heightRange = defaultHeightRange;
				float newHeight = level.Terrain[x, y].position.y;
				// Remap from (currMinHeight, currMaxHeight) to (regionParams.heightRange.x, regionParams.heightRange.y)
				level.Terrain[x, y].position.y = Utils.RemapRange(newHeight, currentMinHeight, currentMaxHeight, heightRange.x, heightRange.y);
			}
		}
	}
}


/// <summary>
/// A class containing parameters for generating a height map using an octaved Perlin noise in a specific region.
/// </summary>
[System.Serializable]
public class RegionHeightMapParameters {
	[Tooltip("A region these parameters are for.")]
	public LevelRegionType region;

	[Tooltip("Parameters of the octaved Perlin noise, e.g. number of octaves, frequency of each octave.")]
	public PerlinNoiseOctaveParameters octaveParams = new();

	[Tooltip("The minimum and maximum height in this region.")]
	public Vector2 heightRange = new Vector2(0, 6);
}