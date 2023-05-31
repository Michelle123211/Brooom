using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Uses several octaves of Perlin noise to generate a height map
public class HeightMapGeneratorPerlin : LevelGeneratorModule
{
    public PerlinNoiseOctaveParameters octaveParams = new PerlinNoiseOctaveParameters();

    public override void Generate(LevelRepresentation level) {
        // Random offset in the Perlin noise to make it different each time
        int randOffsetX = Random.Range(0, 1000);
        int randOffsetY = Random.Range(0, 1000);
        // Grid of points with heights determined by the Perlin noise
        //    - the centre is in (0,0,0)
        //    - distance between adjacent points is level.pointOffset
        // Remember the minimum and maximum heights (for future use in remapping the range)
        float currMinHeight = float.MaxValue;
        float currMaxHeight = float.MinValue;
        for (int x = 0; x < level.pointCount.x; x++) {
            for (int y = 0; y < level.pointCount.y; y++) {
                // Determine height using Perlin noise with octaves
                float height = 0;
                float scale = 1;
                float frequency = octaveParams.initialFrequency;
                // Add contributions from each octave
                for (int octave = 0; octave < octaveParams.numberOfOctaves; octave++) {
                    height += Mathf.PerlinNoise(
                        (randOffsetX + x * level.pointOffset) * frequency, 
                        (randOffsetY + y * level.pointOffset) * frequency) * scale; // multiplied by pointOffset to make the overall shape of terrain not dependent on pointOffset
                    frequency *= octaveParams.frequencyFactor;
                    scale *= octaveParams.scaleFactor;
                }
                level.terrain[x, y].position.y = height;
                // Update minimum and maximum heights
                if (height < currMinHeight) currMinHeight = height;
                if (height > currMaxHeight) currMaxHeight = height;
            }
        }
        // Remap the range from (currMinHeight, currMaxHeight) to (level.heightRange.x, level.heightRange.y)
        for (int x = 0; x < level.pointCount.x; x++) {
            for (int y = 0; y < level.pointCount.y; y++) {
                float newHeight = level.terrain[x, y].position.y;
                // Remap from (currMinHeight, currMaxHeight) to (0,1)
                newHeight = (newHeight - currMinHeight) / (currMaxHeight - currMinHeight);
                // Remap from (0,1) to (level.heightRange.x, level.heightRange.y)
                newHeight = newHeight * (level.heightRange.y - level.heightRange.x) + level.heightRange.x;
                level.terrain[x, y].position.y = newHeight;
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