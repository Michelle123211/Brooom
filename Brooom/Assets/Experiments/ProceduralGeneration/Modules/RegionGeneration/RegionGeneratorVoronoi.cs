using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Divides the map into a grid, then chooses centres of regions as random point in each grid tile
// This way the centres are distributed semi-uniformly and semi-randomly
// Then creates a Voronoi diagram to determine regions
public class RegionGeneratorVoronoi : LevelGeneratorModule {

	[Tooltip("The map is divided into a grid of squares. This number determines each square's width (how many terrain points).")]
	public int regionSize = 20;

	private int regionCountX, regionCountY; // number of regions in each axis
	private int regionSizeX, regionSizeY; // size of each grid tile

	private Vector2[,] centres; // centres if the regions (for each grid tile there is a randomly selected point)

	public override void Generate(LevelRepresentation level) {
		// Prepare list of available regions
		List<MapRegionType> allowedRegions = new List<MapRegionType>();
		foreach (var region in level.regionsAvailability) {
			if (region.Value) allowedRegions.Add(region.Key);
		}

		// Compute number of regions in each axis
		regionCountX = Mathf.FloorToInt(level.pointCount.x / (float)regionSize);
		regionCountY = Mathf.FloorToInt(level.pointCount.y / (float)regionSize);
		// Compute the actual width and height of each grid tile (in fact rectangle)
		regionSizeX = Mathf.CeilToInt(level.pointCount.x / (float)regionCountX);
		regionSizeY = Mathf.CeilToInt(level.pointCount.y / (float)regionCountY);

		// For each grid tile choose randomly a point within it as a region centre and assign it a region it represents
		centres = RandomlySelectCentres(level, allowedRegions);

		// Go through all the terrain points and assign them region from the closest centre
		AssignClosestRegionToAllPoints(level);
	}

	private Vector2[,] RandomlySelectCentres(LevelRepresentation level, List<MapRegionType> allowedRegions) {
		Vector2[,] centres = new Vector2[regionCountX, regionCountY];
		// For each grid tile choose randomly a point within it as a region centre and assign it a region it represents
		for (int x = 0; x < regionCountX; x++) {
			for (int y = 0; y < regionCountY; y++) {
				// Coordinates of the top-left corner
				int startX = x * regionSizeX;
				int startY = y * regionSizeY;
				// Size of the grid tile (the ones on the edge may be smaller)
				int sizeX = Mathf.Min(regionSizeX, level.pointCount.x - startX);
				int sizeY = Mathf.Min(regionSizeY, level.pointCount.y - startY);
				// Randomly choose a point
				int randIndex = Random.Range(0, sizeX * sizeY);
				int centerX = startX + randIndex % sizeX;
				int centerY = startY + randIndex / sizeX;
				centres[x, y] = new Vector2(centerX, centerY);
				// Then select randomly the corresponding region (from the allowed regions)
				if (level.terrain[centerX, centerY].region == MapRegionType.NONE) {
					level.terrain[centerX, centerY].region = allowedRegions[Random.Range(0, allowedRegions.Count)];
				}
			}
		}
		return centres;
	}

	private void AssignClosestRegionToAllPoints(LevelRepresentation level) {
		// Go through all the terrain points and assign them region from the closest centre
		for (int x = 0; x < level.pointCount.x; x++) {
			for (int y = 0; y < level.pointCount.y; y++) {
				// Skip points with already assigned region (e.g. centres)
				if (level.terrain[x, y].region != MapRegionType.NONE) continue;
				// Get coordinates of the tile the point belongs to
				Vector2Int currentTile = new Vector2Int(x / regionSizeX, y / regionSizeY);
				// Look to the tile and its neighbours and find centre with minimum distance
				float minDistance = float.MaxValue;
				Vector2 coords = new Vector2(x, y);
				for (int offsetX = -1; offsetX < 2; offsetX++) {
					for (int offsetY = -1; offsetY < 2; offsetY++) {
						Vector2Int otherTile = currentTile + new Vector2Int(offsetX, offsetY);
						if (otherTile.x >= 0 && otherTile.x < regionCountX && otherTile.y >= 0 && otherTile.y < regionCountY) { // out of bounds check
							Vector2 centre = centres[otherTile.x, otherTile.y];
							float dist = Vector2.Distance(coords, centre);
							if (dist < minDistance) { // closer centre was found
								minDistance = dist;
								level.terrain[x, y].region = level.terrain[(int)centre.x, (int)centre.y].region;
							}
						}
					}
				}
			}
		}
	}
}
