using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Uses a diagram with a function similar to Whittaker's biome diagram to divide map into regions
public class RegionGeneratorWhittaker : LevelGeneratorModule {

	[Tooltip("Diagram describing different map regions with functionality similar to Whittaker's biome diagram")]
	public List<RegionDiagram> availableDiagrams;

	[Tooltip("Frequency of the Perlin noise used for the diagram's X axis.")]
	public float noiseXFrequency = 0.05f;
	[Tooltip("Frequency of the Perlin noise used for the diagram's Y axis.")]
	public float noiseYFrequency = 0.05f;

	private RegionDiagram diagram;

	public override void Generate(LevelRepresentation level) {
		// Random Perlin noise offsets to use to determine point in the diagram
		int randOffsetX1 = Random.Range(0, 1000); // for X axis in the diagram
		int randOffsetX2 = Random.Range(0, 1000);
		int randOffsetY1 = Random.Range(0, 1000); // for Y axis in the diagram
		int randOffsetY2 = Random.Range(0, 1000);
		// Select a diagram to use (the one supporting all allowed regions if possible)
		SelectRegionDiagram(level.TerrainRegionsToInclude);
		// Assign each point of the terrain its region
		for (int x = 0; x < level.pointCount.x; x++) {
			for (int y = 0; y < level.pointCount.y; y++) {
				// Skip points with already assigned region
				if (level.Terrain[x, y].region != LevelRegionType.NONE) continue;
				// Get coordinates in the diagram
				float diagramX = Mathf.PerlinNoise((randOffsetX1 + x) * noiseXFrequency, (randOffsetX2 + y) * noiseXFrequency);
				float diagramY = Mathf.PerlinNoise((randOffsetY1 + x) * noiseYFrequency, (randOffsetY2 + y) * noiseYFrequency);
				// Get corresponding region type
				level.Terrain[x, y].region = GetRegionTypeFromDiagram(diagramX, diagramY);
				// If the chosen region is available, use it as is, otherwise find the closest available region and use it instead
				if (!IsRegionAllowed(level.TerrainRegionsToInclude, level.Terrain[x, y].region))
					level.Terrain[x, y].region = GetClosestAllowedRegion(diagramX, diagramY, level.TerrainRegionsToInclude);
				level.RegionsInLevel.Add(level.Terrain[x, y].region); // note down this region is used
			}
		}
	}

	// Selects an available RegionDiagram which supports the most allowed regions if possible
	private void SelectRegionDiagram(List<LevelRegionType> regionsToInclude) {
		// Use the first one as default
		diagram = availableDiagrams[0];
		// Find the one supporting the most allowed regions
		int maxSupportedRegions = 0;
		foreach (var regionDiagram in availableDiagrams) {
			int supportedRegions = 0;
			foreach (var region in regionDiagram.diagramRegions) {
				if (regionsToInclude.Contains(region.regionType))
					supportedRegions++;
			}
			if (supportedRegions > maxSupportedRegions) {
				diagram = regionDiagram;
				maxSupportedRegions = supportedRegions;
			}
		}
	}

	private LevelRegionType GetRegionTypeFromDiagram(float x, float y) {
		LevelRegionType regionType = LevelRegionType.NONE;
		// Get the region exactly on these coordinates
		foreach (var region in diagram.diagramRegions) {
			if (x >= region.minValues.x && x <= region.maxValues.x && y >= region.minValues.y && y <= region.maxValues.y) {
				regionType = region.regionType;
				break;
			}
		}
		return regionType;
	}

	private LevelRegionType GetClosestAllowedRegion(float x, float y, List<LevelRegionType> terrainRegionsToInclude) {
		// Get the allowed region on coordinates closest to the original ones (if none exists, return MapRegionType.NONE)
		// ... using simple Manhattan distance
		float minDistance = float.MaxValue;
		LevelRegionType regionType = LevelRegionType.NONE;
		foreach (var region in diagram.diagramRegions) {
			if (IsRegionAllowed(terrainRegionsToInclude, region.regionType)) {
				float distance = Mathf.Min(Mathf.Abs(x - region.minValues.x), Mathf.Abs(x - region.maxValues.x)) + Mathf.Min(Mathf.Abs(y - region.minValues.y), Mathf.Abs(y - region.maxValues.y));
				if (distance < minDistance) {
					regionType = region.regionType;
					minDistance = distance;
				}
			}
		}
		return regionType;
	}

	private bool IsRegionAllowed(List<LevelRegionType> regionsToInclude, LevelRegionType regionType) {
		return regionsToInclude.Contains(regionType);
	}
}