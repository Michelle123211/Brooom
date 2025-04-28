using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A level generator module responsible for identifying terrain points which are on region borders (of some given width).
/// A certain border tolerance is defined, then points whose distance to other regions (in 8 directions) is less than or equal to that are considered border between regions.
/// Different regions may override this border tolerance value to consider wider or narrower border.
/// Points on region borders may be treated differently further in the level generation pipeline.
/// </summary>
public class RegionBorderDetectionSimple : LevelGeneratorModule {

    [Tooltip("Points whose distance to other regions (in 8 directions) is less than or equal to this number are considered border between regions.")]
    public int defaultBorderTolerance = 7;
    [Tooltip("Some regions may prefer different border tolerance than the default one (e.g. smaller for mountains).")]
    public List<RegionBorderTolerance> borderToleranceOverrides;

    private Dictionary<LevelRegionType, int> regionBorderTolerance; // border tolerance for each region (either default or override)

    /// <summary>
    /// Identifies points which are on region borders (i.e. they are closer to a different region than some border tolerance value).
    /// </summary>
    /// <param name="level"><inheritdoc/></param>
	public override void Generate(LevelRepresentation level) {
        // Prepare Dictionary of border tolerances for each region type
        regionBorderTolerance = new Dictionary<LevelRegionType, int>();
        foreach (var region in level.TerrainRegions) {
            regionBorderTolerance.Add(region.Key, defaultBorderTolerance); // default border tolerance at first
        }
        // Override border tolerance for specific regions
        if (borderToleranceOverrides != null) {
            foreach (var borderOverride in borderToleranceOverrides) {
                regionBorderTolerance[borderOverride.region] = borderOverride.borderTolerance;
            }
        }

        // Go through all terrain points and determine, whether it lies in borders (based on distance to other regions)
        for (int x = 0; x < level.pointCount.x; x++) {
            for (int y = 0; y < level.pointCount.y; y++) {
                level.Terrain[x, y].isOnBorder = IsPointOnBorder(level, x, y);
            }
        }
    }

    // Determines whether the point is on the border based on distance to other regions
    private bool IsPointOnBorder(LevelRepresentation level, int x, int y) {
        int step = 1;
        if (!regionBorderTolerance.TryGetValue(level.Terrain[x, y].region, out int tolerance))
            tolerance = defaultBorderTolerance;
        while (step <= tolerance) { // check gradually larger neighbourhood
            // Go through 8 directions
            for (int i = -1; i < 2; i++) {
                int otherX = x + step * i;
                if (otherX < 0 || otherX >= level.pointCount.x) continue; // out of bounds check
                for (int j = -1; j < 2; j++) {
                    if (i == 0 && j == 0) continue;
                    int otherY = y + step * j;
                    if (otherY < 0 || otherY >= level.pointCount.y) continue; // out of bounds check
                    // If there is a different region, it is border point
                    if (level.Terrain[otherX, otherY].region != level.Terrain[x, y].region) {
                        return true;
                    }
                }
            }
            step++;
        }
        return false;
    }
}

/// <summary>
/// A class specifying a region border tolerance for a specific region.
/// Terrain points in this region whose distance to other regions (in 8 directions) is less than or equal to this number are considered border between regions
/// </summary>
[System.Serializable]
public class RegionBorderTolerance {
    [Tooltip("A region for which the border tolerance is specified.")]
    public LevelRegionType region = LevelRegionType.NONE;
    [Tooltip("Terrain points in this region whose distance to other regions (in 8 directions) is less than or equal to this number are considered border between regions.")]
    public int borderTolerance = 3;
}
