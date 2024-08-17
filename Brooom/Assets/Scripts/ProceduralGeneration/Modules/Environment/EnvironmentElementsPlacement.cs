using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentElementsPlacement : LevelGeneratorModule {

	[Header("Basic parameters")]

	[Tooltip("The level is divided into a grid of tiles and within each tile a spot for environment element is randomly selected.")]
	public int gridTileSize = 5;

	[Tooltip("Edges of the level (of this width in number of terrain points) will be kept empty.")]
	public int levelBorderWidth = 5;

	[Tooltip("Probability of placing elements on the borders of regions. May be used to make it sparse there.")]
	public float regionBorderProbability = 0.5f;

	[Header("Start line parameters")]

	[Tooltip("Width (in the X axis) of the rectangular region around the player start position which should be considered start line and should be populated only with short enough elements.")]
	public float startLineSpanX = 20f;
	[Tooltip("Height (in the Z axis) of the rectangular region around the player start position which should be considered start line and should be populated only with short enough elements.")]
	public float startLineSpanZ = 14f;
	[Tooltip("Environment elements near start line should end at least this many units below the racers.")]
	public float minVerticalSpacing = 10f;

	[Header("Environment elements")]

	[Tooltip("An object which will be parent of all the environment objects in the hierarchy.")]
	public Transform environmentParent;

	[Tooltip("List of default environment elements which should be in every region (e.g. clouds).")]
	public RegionEnvironment defaultElements;

	[Tooltip("List of environment elements specific for each region.")]
	public List<RegionEnvironment> regionElements;

	// Size measured in number of terrain points (as opposed to gridTileSize which is in default units)
	private int tileSizeInPoints;

	public override void Generate(LevelRepresentation level) {
		// Remove any previously instantiated elements
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
				// Generate random number between 0 and 1
				float randomNumber = Random.value;
				// According to the number, select element type
				EnvironmentElement element = SelectElementType(region, ref randomNumber); // ... from the elements specific for the region
				if (element == null) element = SelectElementType(defaultElements, ref randomNumber); // ... from the default elements
				if (element != null) // the spot should not be empty
					CreateElementInstance(level, spotX, spotY, element);
				break;
			}
		}
	}

	private EnvironmentElement SelectElementType(RegionEnvironment environment, ref float randomNumber) {
		if (environment.elements != null && environment.elements.Count > 0) { // There are some options
			foreach (var element in environment.elements) {
				if (randomNumber <= element.probability) { // this is the selected element
					return element;
				} else {
					randomNumber -= element.probability;
				}
			}
		}
		return null;
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
		GameObject elementVariant = element.elementPrefabs[variantIndex];
		// If the spot is too close to the start line and the element is too tall, it cannot be spawned
		if (IsCloseToStartLine(level, spotX, spotY) && !CanBeSpawnedAtStartLine(level, elementVariant, spotX, spotY)) {
			return;
		}
		// Compute parameters
		Vector3 position = level.terrain[spotX, spotY].position;
		if (element.specificHeightRange) {
			if (position.y >= element.heightRange.y) return; // not possible to place the object above terrain
			else position.y = Random.Range(element.heightRange.x, element.heightRange.y);
		}
		Quaternion rotation = element.randomRotation ? Quaternion.Euler(0, Random.Range(0, 360), 0) : Quaternion.identity;
		float scale = Random.Range(element.scaleRange.x, element.scaleRange.y);
		// Placement on region borders is even less probable (or even impossible for some elements)
		if (level.terrain[spotX, spotY].isOnBorder) {
			if (!element.canBeOnRegionBorder || Random.value > regionBorderProbability) return;
		}
		// Instantiate with parameters
		GameObject instance = Instantiate(elementVariant, position, rotation, environmentParent);
		instance.transform.localScale *= scale;
		// Set shadows on/off
		MeshRenderer meshRenderer = instance.GetComponentInChildren<MeshRenderer>();
		if (!element.castShadows) {
			meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		} else {
			meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
		}
	}
	
	// Checks whether the given grid point is within the rectangular region of start line
	private bool IsCloseToStartLine(LevelRepresentation level, int spotX, int spotY) {
		Vector3 difference = level.playerStartPosition - level.terrain[spotX, spotY].position;
		return Mathf.Abs(difference.x) < (startLineSpanX / 2f) && Mathf.Abs(difference.z) < (startLineSpanZ / 2f);
	}

	// Checks whether the given element is short enough to be spawned on the given position assuming it is within the rectangular start line region
	private bool CanBeSpawnedAtStartLine(LevelRepresentation level, GameObject elementVariant, int spotX, int spotY) {
		MeshFilter meshFilter = elementVariant.GetComponentInChildren<MeshFilter>();
		float elementHeight = meshFilter.sharedMesh.bounds.size.y;
		float allowedHeight = (level.playerStartPosition.y - level.terrain[spotX, spotY].position.y) - minVerticalSpacing; // at least several units below the racers
		return elementHeight < allowedHeight;
	}
}

[System.Serializable]
public class RegionEnvironment {
	[Tooltip("Used just to display a reasonable name instead of default 'Element X'.")]
	public string name;
	public LevelRegionType region;
	[Tooltip("Elements with their variations and probabilities of them being placed on a generated spot. If probabilities don't add up to 1, the rest is probability of empty spot.")]
	public List<EnvironmentElement> elements;
}

[System.Serializable]
public class EnvironmentElement {
	[Tooltip("Used just to display a reasonable name instead of default 'Element X'.")]
	public string name;
	[Tooltip("From these elements one will be chosen randomly whenever an element is placed.")]
	public List<GameObject> elementPrefabs;
	[Tooltip("Number between 0 and 1 determining probability of placing any variant of this element on a generated spot.")]
	public float probability;
	[Tooltip("Whether the objects should use random rotation around Y axis when instantiated.")]
	public bool randomRotation = true;
	[Tooltip("The instance will be scaled according to a random number from the interval (in all 3 axes).")]
	public Vector2 scaleRange = Vector2.one;
	[Tooltip("Some objects may not be fitting very well on region borders.")]
	public bool canBeOnRegionBorder = true;
	[Tooltip("If false, the objects will be placed on the ground. If true, objects will be placed at random height from the range given below.")]
	public bool specificHeightRange = false;
	[Tooltip("Objects will be placed to a random height from this interval if the 'specificHeightRange' is set to true.")]
	public Vector2 heightRange = Vector2.zero;
	[Tooltip("Whether these objects should cast shadows.")]
	public bool castShadows = true;
}