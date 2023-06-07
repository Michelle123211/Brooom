using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Identifies points on region borders (of some given width)
public class RegionBorderDetectionSimple : LevelGeneratorModule {

    [Tooltip("Points whose distance to other regions (in 8 directions) is less than or equal to this number are considered border between regions.")]
    public int defaultBorderTolerance = 5;
    [Tooltip("Some regions may prefer different border tolerance than the default one (e.g. smaller for mountains).")]
    public List<RegionBorderTolerance> borderToleranceOverrides;

    private Dictionary<MapRegionType, int> regionBorderTolerance;

	public override void Generate(LevelRepresentation level) {
        // Prepare Dictionary of border tolerances for each region type
        regionBorderTolerance = new Dictionary<MapRegionType, int>();
        foreach (var region in level.regions) {
            regionBorderTolerance.Add(region.Key, defaultBorderTolerance);
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
                level.terrain[x, y].isOnBorder = IsPointOnBorder(level, x, y);
            }
        }
    }

    private bool IsPointOnBorder(LevelRepresentation level, int x, int y) {
        // Determine whether the point is on the border based on distance to other regions
        int step = 1;
        int tolerance;
        if (!regionBorderTolerance.TryGetValue(level.terrain[x, y].region, out tolerance))
            tolerance = defaultBorderTolerance;
        while (step <= tolerance) {
            // Go through 8 directions
            for (int i = -1; i < 2; i++) {
                int otherX = x + step * i;
                if (otherX < 0 || otherX >= level.pointCount.x) continue; // out of bounds check
                for (int j = -1; j < 2; j++) {
                    if (i == 0 && j == 0) continue;
                    int otherY = y + step * j;
                    if (otherY < 0 || otherY >= level.pointCount.y) continue; // out of bounds check
                    // If there is a different region, it is border point
                    if (level.terrain[otherX, otherY].region != level.terrain[x, y].region) {
                        return true;
                    }
                }
            }
            step++;
        }
        return false;
    }
}

[System.Serializable]
public class RegionBorderTolerance {
    public MapRegionType region = MapRegionType.NONE;
    [Tooltip("Points whose distance to other regions (in 8 directions) is less than or equal to this number are considered border between regions.")]
    public int borderTolerance = 3;
}
