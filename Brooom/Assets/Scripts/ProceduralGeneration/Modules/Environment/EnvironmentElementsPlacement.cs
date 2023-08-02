using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentElementsPlacement : LevelGeneratorModule {
	[Tooltip("The level is divided into a grid of tiles and within each tile a spot for environment element is randomly selected.")]
	public int gridTileSize = 5;

	[Tooltip("Edges of the level (of this width in number of terrain points) will be kept empty.")]
	public int levelBorderWidth = 5;

	[Tooltip("Probability of placing elements on the borders of regions. May be used to make it sparse there.")]
	public float regionBorderProbability = 0.5f;

	[Tooltip("An object which will be parent of all the environment objects in the hierarchy.")]
	public Transform environmentParent;

	[Tooltip("List of environment elements specific for each region.")]
	public List<RegionEnvironment> regionElements;

	// Size measured in number of terrain points (as opposed to gridTileSize which is in default units)
	private int tileSizeInPoints;

	public override void Generate(LevelRepresentation level) {
		// Remove any previously instantiated bonuses
		UtilsMonoBehaviour.RemoveAllChildren(environmentParent);
		// Compute number of tiles
		tileSizeInPoints = Mathf.RoundToInt(gridTileSize / level.pointOffset); // convert to size measured in terrain points
		int tileCountX = Mathf.CeilToInt(level.pointCount.x / (float)tileSizeInPoints);
		int tileCountY = Mathf.CeilToInt(level.pointCount.y / (float)tileSizeInPoints);
		// For each grid tile choose randomly a point within it
		for (int x = 0; x < tileCountX; x++) {
			for (int y = 0; y < tileCountY; y++) {
				// Coordinates of the top-left corner
				int startX = x * tileSizeInPoints;
				int startY = y * tileSizeInPoints;
				// Size of the grid tile (the ones on the edge may be smaller)
				int sizeX = Mathf.Min(tileSizeInPoints, level.pointCount.x - startX);
				int sizeY = Mathf.Min(tileSizeInPoints, level.pointCount.y - startY);
				// Randomly choose a point
				int randIndex = Random.Range(0, sizeX * sizeY);
				// Fill the spot
				FillEnvironmentSpot(level, startX + randIndex % sizeX, startY + randIndex / sizeX);
			}
		}
	}

	private void FillEnvironmentSpot(LevelRepresentation level, int spotX, int spotY) {
		// Decide what will be there according to its region and instantiate it
		LevelRegionType regionType = level.terrain[spotX, spotY].region;
		foreach (var region in regionElements) {
			if (region.region == regionType) {
				// Check that there are some options of elements
				if (region.elements == null || region.elements.Count == 0)
					break; // the spot should be empty
				// Generate random number between 0 and 1
				float randomNumber = Random.value;
				// According to the number, select element type
				foreach (var element in region.elements) {
					if (randomNumber <= element.probability) { // this is the selected element
						CreateElementInstance(level, spotX, spotY, element);
						break;
					} else {
						randomNumber -= element.probability;
					}
				}
				break;
			}
		}
	}

	private void CreateElementInstance(LevelRepresentation level, int spotX, int spotY, EnvironmentElement element) {
		// Check for available variants
		if (element.elementPrefabs == null || element.elementPrefabs.Count == 0) return;
		// Prevent placing object at the edge of the level
		if (spotX < levelBorderWidth || spotX >= level.pointCount.x - levelBorderWidth ||
			spotY < levelBorderWidth || spotY >= level.pointCount.y - levelBorderWidth)
			return;
		// Select random variant of the element type
		int variantIndex = Random.Range(0, element.elementPrefabs.Count);
		// Compute parameters
		Quaternion rotation = element.randomRotation ? Quaternion.Euler(0, Random.Range(0, 360), 0) : Quaternion.identity;
		float scale = Random.Range(element.scaleRange.x, element.scaleRange.y);
		// Placement on region borders is even less probable
		if (level.terrain[spotX, spotY].isOnBorder && Random.value > regionBorderProbability)
			return;
		// Instantiate with parameters
		GameObject instance = Instantiate(element.elementPrefabs[variantIndex], level.terrain[spotX, spotY].position, rotation, environmentParent);
		instance.transform.localScale *= scale;
	}
}

[System.Serializable]
public class RegionEnvironment {
	public LevelRegionType region;
	[Tooltip("Elements with their variations and probabilities of them being placed on a generated spot. If probabilities don't add up to 1, the rest is probability of empty spot.")]
	public List<EnvironmentElement> elements;
}

[System.Serializable]
public class EnvironmentElement {
	[Tooltip("From these elements one will be chosen randomly whenever an element is placed.")]
	public List<GameObject> elementPrefabs;
	[Tooltip("Number between 0 and 1 determining probability of placing any variant of this element on a generated spot.")]
	public float probability;
	[Tooltip("Whether the objects should use random rotation around Y axis when instantiated.")]
	public bool randomRotation = true;
	[Tooltip("The instance will be scaled according to a random number from the interval (in all 3 axes).")]
	public Vector2 scaleRange = Vector2.one;
}