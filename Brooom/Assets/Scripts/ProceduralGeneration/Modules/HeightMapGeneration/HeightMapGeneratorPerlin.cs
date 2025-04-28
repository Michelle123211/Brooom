using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A level generator module responsible for generating a height map using several octaves of Perlin noise.
/// </summary>
public class HeightMapGeneratorPerlin : LevelGeneratorModule {

    [Tooltip("The minimum and maximum possible height of the terrain.")]
    public Vector2 heightRange = new Vector2(-5, 25);

    [Tooltip("Parameters of the octaved Perlin noise, e.g. number of octaves, frequency of each octave.")]
    public PerlinNoiseOctaveParameters octaveParams = new PerlinNoiseOctaveParameters();

    /// <summary>
    /// Assigns height to each terrain point based on an octaved Perlin noise.
    /// </summary>
    /// <param name="level"><inheritdoc/></param>
    public override void Generate(LevelRepresentation level) {
        // Random offset in the Perlin noise to make it different each time
        int randOffsetX = Random.Range(0, 1000);
        int randOffsetY = Random.Range(0, 1000);
        OctavedPerlinNoise perlinNoise = new OctavedPerlinNoise(randOffsetX, randOffsetY, octaveParams);
        // Grid of points with heights determined by the Perlin noise
        //    - the centre is in (0,0,0)
        //    - distance between adjacent points is level.pointOffset
        // Remember the minimum and maximum heights (for future use in remapping the range)
        float currMinHeight = float.MaxValue;
        float currMaxHeight = float.MinValue;
        for (int x = 0; x < level.pointCount.x; x++) {
            for (int y = 0; y < level.pointCount.y; y++) {
                // Determine height using Perlin noise with octaves
                float height = perlinNoise.GetValue(x * level.pointOffset, y * level.pointOffset); // multiplied by pointOffset to make the overall shape of terrain not dependent on pointOffset
                level.Terrain[x, y].position.y = height;
                // Update minimum and maximum heights
                if (height < currMinHeight) currMinHeight = height;
                if (height > currMaxHeight) currMaxHeight = height;
            }
        }
        // Remap the range from (currMinHeight, currMaxHeight) to (heightRange.x, heightRange.y)
        for (int x = 0; x < level.pointCount.x; x++) {
            for (int y = 0; y < level.pointCount.y; y++) {
                level.Terrain[x, y].position.y = Utils.RemapRange(level.Terrain[x, y].position.y, currMinHeight, currMaxHeight, heightRange.x, heightRange.y);
            }
        }
    }
}

/// <summary>
/// A class describing parameters for an octaved Perlin noise, e.g. number of octaves, frequency or each octave.
/// </summary>
[System.Serializable]
public class PerlinNoiseOctaveParameters {
    [Tooltip("How many octaves (of different altitude and frequency) will be combined.")]
    public int numberOfOctaves = 3;
    [Tooltip("Frequency of the first octave.")]
    public float initialFrequency = 0.05f;
    [Tooltip("To switch to the next octave the previous octave's frequency will be multiplied by this number.")]
    public float frequencyFactor = 2f;
    [Tooltip("To switch to the next octave the previous octave's scale/altitude will be multiplied by this number.")]
    public float scaleFactor = 0.5f;
}

/// <summary>
/// A class providing support to work with several octaves of Perlin noise at once.
/// </summary>
public class OctavedPerlinNoise {

    private float offsetX;
    private float offsetY;
    private PerlinNoiseOctaveParameters octaves; // parameters of the octaved Perlin noise, e.g. number of octaves, frequency of each octave

    /// <summary>
    /// Creates a 2D noise combining several octaves of Perlin noise.
    /// </summary>
    /// <param name="offsetX">Noise offset in the X axis (used instead of 0 in underlying Perlin noise).</param>
    /// <param name="offsetY">Noise offset in the Y axis (used instead of 0 in underlying Perlin noise).</param>
    /// <param name="octaves">Parameters of the octaved Perlin noise, e.g. number of octaves, frequency of each octave.</param>
    public OctavedPerlinNoise(float offsetX, float offsetY, PerlinNoiseOctaveParameters octaves) {
        this.offsetX = offsetX;
        this.offsetY = offsetY;
        this.octaves = octaves;
    }

    /// <summary>
    /// Computes a value of a 2D noise in the given point.
    /// </summary>
    /// <param name="x">X-coordinate of sample point.</param>
    /// <param name="y">X-coordinate of sample point.</param>
    /// <returns>Noise value on the given coordiantes.</returns>
    public float GetValue(float x, float y) {
        float value = 0;
        float scale = 1;
        float frequency = octaves.initialFrequency;
        // Add contributions from each octave
        for (int octave = 0; octave < octaves.numberOfOctaves; octave++) {
            value += Mathf.PerlinNoise((offsetX + x) * frequency, (offsetY + y) * frequency) * scale;
            frequency *= octaves.frequencyFactor;
            scale *= octaves.scaleFactor;
        }
        return value;
    }
}