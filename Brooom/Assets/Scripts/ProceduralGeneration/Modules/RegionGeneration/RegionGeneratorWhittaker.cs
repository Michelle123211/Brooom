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
		SelectRegionDiagram(level.regionsAvailability);
		// Assign each point of the terrain its region
		for (int x = 0; x < level.pointCount.x; x++) {
			for (int y = 0; y < level.pointCount.y; y++) {
				// Skip points with already assigned region
				if (level.terrain[x, y].region != MapRegionType.NONE) continue;
				// Get coordinates in the diagram
				float diagramX = Mathf.PerlinNoise((randOffsetX1 + x) * noiseXFrequency, (randOffsetX2 + y) * noiseXFrequency);
				float diagramY = Mathf.PerlinNoise((randOffsetY1 + x) * noiseYFrequency, (randOffsetY2 + y) * noiseYFrequency);
				// Get corresponding region type
				level.terrain[x, y].region = GetRegionTypeFromDiagram(diagramX, diagramY);
				// If the chosen region is available, use it as is, otherwise find the closest available region and use it instead
				if (!IsRegionAllowed(level.terrain[x, y].region, level.regionsAvailability))
					level.terrain[x, y].region = GetClosestAllowedRegion(diagramX, diagramY, level.regionsAvailability);
			}
		}
	}

	// Selects an available RegionDiagram which supports the most allowed regions if possible
	private void SelectRegionDiagram(Dictionary<MapRegionType, bool> regionsAvailability) {
		// Use the first one as default
		diagram = availableDiagrams[0];
		// Find the one supporting the most allowed regions
		int maxSupportedRegions = 0;
		foreach (var regionDiagram in availableDiagrams) {
			int supportedRegions = 0;
			foreach (var region in regionDiagram.diagramRegions) {
				if (regionsAvailability.TryGetValue(region.regionType, out bool isAllowed) && isAllowed)
					supportedRegions++;
			}
			if (supportedRegions > maxSupportedRegions) {
				diagram = regionDiagram;
				maxSupportedRegions = supportedRegions;
			}
		}
	}

	private MapRegionType GetRegionTypeFromDiagram(float x, float y) {
		MapRegionType regionType = MapRegionType.NONE;
		// Get the region exactly on these coordinates
		foreach (var region in diagram.diagramRegions) {
			if (x >= region.minValues.x && x <= region.maxValues.x && y >= region.minValues.y && y <= region.maxValues.y) {
				regionType = region.regionType;
				break;
			}
		}
		return regionType;
	}

	private MapRegionType GetClosestAllowedRegion(float x, float y, Dictionary<MapRegionType, bool> regionsAvailability) {
		// Get the allowed region on coordinates closest to the original ones (if none exists, return MapRegionType.NONE)
		// ... using simple Manhattan distance
		float minDistance = float.MaxValue;
		MapRegionType regionType = MapRegionType.NONE;
		foreach (var region in diagram.diagramRegions) {
			if (IsRegionAllowed(region.regionType, regionsAvailability)) {
				float distance = Mathf.Min(Mathf.Abs(x - region.minValues.x), Mathf.Abs(x - region.maxValues.x)) + Mathf.Min(Mathf.Abs(y - region.minValues.y), Mathf.Abs(y - region.maxValues.y));
				if (distance < minDistance) {
					regionType = region.regionType;
					minDistance = distance;
				}
			}
		}
		return regionType;
	}

	private bool IsRegionAllowed(MapRegionType regionType, Dictionary<MapRegionType, bool> regionsAvailability) {
		return regionsAvailability.TryGetValue(regionType, out bool isAllowed) && isAllowed;
	}
}