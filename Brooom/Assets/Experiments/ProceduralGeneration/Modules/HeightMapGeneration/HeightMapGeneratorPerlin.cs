using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Uses several octaves of Perlin noise to generate a height map
public class HeightMapGeneratorPerlin : LevelGeneratorModule
{
    [Header("Octaves (from Perlin noise)")]
    [Tooltip("How many octaves (of different altitude and frequency) will be combined.")]
    public int numberOfOctaves = 3;
    [Tooltip("Frequency of the first octave.")]
    public float initialFrequency = 0.05f;
    [Tooltip("To switch to the next octave the frequency will be multiplied by this number.")]
    public float frequencyFactor = 2f;
    [Tooltip("To switch to the next octave the scale will be multiplied by this number.")]
    public float scaleFactor = 0.5f;

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
        for (int x = 0, i = 0; x < level.pointCount.x; x++) {
            for (int y = 0; y < level.pointCount.y; y++) {
                level.terrain[x, y].vertexIndex = i;
                // Determine height using Perlin noise with octaves
                float height = 0;
                float scale = 1;
                float frequency = initialFrequency;
                // Add contributions from each octave
                for (int octave = 0; octave < numberOfOctaves; octave++) {
                    height += Mathf.PerlinNoise(
                        (randOffsetX + x * level.pointOffset) * frequency, 
                        (randOffsetY + y * level.pointOffset) * frequency) * scale; // multiplied by pointOffset to make the overall shape of terrain not dependent on pointOffset
                    frequency *= frequencyFactor;
                    scale *= scaleFactor;
                }
                // Update minimum and maximum heights
                if (height < currMinHeight) currMinHeight = height;
                if (height > currMaxHeight) currMaxHeight = height;
                // Create a new point
                level.terrain[x, y].position = new Vector3(
                    level.startPosition.x + x * level.pointOffset, 
                    height, 
                    level.startPosition.y + y * level.pointOffset);
                i++;
            }
        }
        // Remap the range from (currMinHeight, currMaxHeight) to (minimumHeight, maximumHeight)
        for (int x = 0; x < level.pointCount.x; x++) {
            for (int y = 0; y < level.pointCount.y; y++) {
                float newHeight = level.terrain[x, y].position.y;
                // Remap from (currMinHeight, currMaxHeight) to (0,1)
                newHeight = (newHeight - currMinHeight) / (currMaxHeight - currMinHeight);
                // Remap from (0,1) to (minimumHeight, maximumHeight)
                newHeight = newHeight * (level.heightRange.y - level.heightRange.x) + level.heightRange.x;
                level.terrain[x, y].position.y = newHeight;
            }
        }
    }
}
