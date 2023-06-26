using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Determines locations of track regions (e.g. Above Clouds)
// These regions do not affect the terrain and are occurring in the track at most once
public class TrackRegionGenerator : LevelGeneratorModule
{
    [Tooltip("All track regions with parameters of their occurrences.")]
    public List<TrackRegionParameters> trackRegions;

    private float[] kernel = new float[] { 1f/10, 2f/10, 4f/10, 2f/10, 1f/10 };

	public override void Generate(LevelRepresentation level) {
        if (trackRegions == null) trackRegions = new List<TrackRegionParameters>();
        foreach (var region in trackRegions) {
            // Skip not available regions
            if (!level.regionsAvailability.ContainsKey(region.trackRegion) || !level.regionsAvailability[region.trackRegion])
                continue;
            // Add the given region with some probability
            if (Random.Range(0f, 1f) < region.probability) {
                int length = Random.Range(region.lengthRange.x, region.lengthRange.y); // random length
                // Choose starting index
                int startIndex = Random.Range(0, level.track.Count - length);
                // Adjust all selected track points
                for (int i = startIndex; i < startIndex + length; i++) {
                    // Select height in the range
                    if (level.track[i].position.y < region.heightRange.x || level.track[i].position.y > region.heightRange.y)
                        level.track[i].position.y = Random.Range(region.heightRange.x, region.heightRange.y);
                    // Mark the track region in the TrackPoint
                    level.track[i].trackRegion = region.trackRegion;
                }
                // Interpolate heights on edges
                int endIndex = startIndex + length - 1;
                InterpolateHeightsInRegion(level, startIndex, endIndex);
            }
        }
	}

    // Smooths out height in the region and its immediate neighbourhood
    private void InterpolateHeightsInRegion(LevelRepresentation level, int regionStartIndex, int regionEndIndex) {
        int startIndex = regionStartIndex - 2;
        if (startIndex < 0) startIndex = 0;
        int endIndex = regionEndIndex + 2;
        if (endIndex >= level.track.Count) endIndex = level.track.Count - 1;
        // Compute new heights as weighted averages
        float[] newHeights = new float[endIndex - startIndex + 1];
        for (int currentIndex = startIndex; currentIndex <= endIndex; currentIndex++) {
            for (int i = -2; i <= 2; i++) {
                int neighbourIndex = currentIndex + i;
                if (neighbourIndex < 0 || neighbourIndex >= level.track.Count) continue;
                newHeights[currentIndex - startIndex] += level.track[neighbourIndex].position.y * kernel[i + 2];
            }
        }
        // Update the heights
        for (int i = 0; i < newHeights.Length; i++) {
            level.track[startIndex + i].position.y = newHeights[i];
        }
    }
}

// Describes parameters of track region occurrence
[System.Serializable]
public class TrackRegionParameters {
    public LevelRegionType trackRegion;
    [Tooltip("Probability that the given region occurs in the track.")]
    public float probability;
    [Tooltip("Minimum and maximum possible length of the region (in number of track points).")]
    public Vector2Int lengthRange;
    [Tooltip("Minimum and maximum possible height of the points corresponding to the region.")]
    public Vector2 heightRange;
}