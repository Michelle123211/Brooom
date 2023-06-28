using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Uses several octaves of Perlin noise to generate a height map
public class HeightMapGeneratorPerlin : LevelGeneratorModule {

    [Tooltip("What is the minimum and maximum height of the terrain.")]
    public Vector2 heightRange = new Vector2(-5, 25);

    public PerlinNoiseOctaveParameters octaveParams = new PerlinNoiseOctaveParameters();

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
                level.terrain[x, y].position.y = height;
                // Update minimum and maximum heights
                if (height < currMinHeight) currMinHeight = height;
                if (height > currMaxHeight) currMaxHeight = height;
            }
        }
        // Remap the range from (currMinHeight, currMaxHeight) to (heightRange.x, heightRange.y)
        for (int x = 0; x < level.pointCount.x; x++) {
            for (int y = 0; y < level.pointCount.y; y++) {
                level.terrain[x, y].position.y = Utils.RemapRange(level.terrain[x, y].position.y, currMinHeight, currMaxHeight, heightRange.x, heightRange.y);
            }
        }
    }
}

[System.Serializable]
public class PerlinNoiseOctaveParameters {
    [Tooltip("How many octaves (of different altitude and frequency) will be combined.")]
    public int numberOfOctaves = 3;
    [Tooltip("Frequency of the first octave.")]
    public float initialFrequency = 0.05f;
    [Tooltip("To switch to the next octave the frequency will be multiplied by this number.")]
    public float frequencyFactor = 2f;
    [Tooltip("To switch to the next octave the scale will be multiplied by this number.")]
    public float scaleFactor = 0.5f;
}

public class OctavedPerlinNoise {

    private float offsetX;
    private float offsetY;
    private PerlinNoiseOctaveParameters octaves;

    public OctavedPerlinNoise(float offsetX, float offsetY, PerlinNoiseOctaveParameters octaves) {
        this.offsetX = offsetX;
        this.offsetY = offsetY;
        this.octaves = octaves;
    }

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