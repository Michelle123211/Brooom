using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class LevelGenerationPipeline : MonoBehaviour {
	[Tooltip("These generators/modules will be used in the exact order. Disabled module is not executed even if part of the pipeline.")]
	[SerializeField] List<LevelGeneratorModuleSlot> modules;

	[Header("Terrain parameters")]
	[Tooltip("Dimensions of the terrain in the X and Z axes. Final dimensions will be determined as the closest larger multiple of pointOffset.")]
	[SerializeField] Vector2 dimensions = new Vector2(50, 50);
	[Tooltip("Distance between two adjacent points in the grid.")]
	[SerializeField] float pointOffset = 0.5f;

	[Header("Block-based parameters")]
	[Tooltip("All level contents are divided into blocks of this size (i.e. number of terrain points on one side) to allow for optimizations (e.g. disabling all objects in blocks far away).")]
	[SerializeField] int blockSizeInPoints = 25;
	[Tooltip("Parent object of all terrain blocks.")]
	[SerializeField] Transform terrainParent;
	[Tooltip("Prefab containing all necessary components for terrain block")]
	[SerializeField] TerrainBlock terrainBlockPrefab;

	[Header("Level regions")]
	[Tooltip("All terrain regions in the game.")]
	public List<LevelRegion> terrainRegions;
	[Tooltip("All track regions in the game.")]
	public List<LevelRegion> trackRegions;
	[Tooltip("Regions availability in the currently generated level.")]
	public Dictionary<LevelRegionType, bool> regionsAvailability;
	[Tooltip("Regions which have already been visited (when there is an available but unvisited region, it may be prioritized in the level).")]
	public Dictionary<LevelRegionType, bool> regionsVisited;

	[Tooltip("Terrain regions which should be used when generating the level (for track regions, all available ones will be used).")]
	[HideInInspector] public List<LevelRegionType> terrainRegionsToInclude; // TODO: Fill from outside, work with only these when generating level


	// Anyone can register a callback for when a level is generated
	public event Action<LevelRepresentation> onLevelGenerated;


	// Object-oriented representation
	public LevelRepresentation Level { get; private set; }
	private Dictionary<LevelRegionType, LevelRegion> terrainRegionsDict;
	private Dictionary<LevelRegionType, LevelRegion> trackRegionsDict;


#if UNITY_EDITOR
	[ContextMenu("Regenerate level")]
	private void RegenerateLevel() { // Regenerates the level with previous parameters
		if (Level == null) Initialize();
		if (modules != null) {
			Level.ResetLevelWithDimensions(dimensions, pointOffset, blockSizeInPoints);
			foreach (var moduleSlot in modules) {
				if (moduleSlot.isModuleEnabled) {
					moduleSlot.module.Generate(Level);
				}
			}
		}
		GenerateTerrainMesh();
		onLevelGenerated?.Invoke(Level);
	}
#endif

	public IEnumerator GenerateLevel() { // Generates the level with the current parameters
		Initialize();
		if (modules != null) {
			foreach (var moduleSlot in modules) {
				if (moduleSlot.isModuleEnabled) {
					moduleSlot.module.Generate(Level);
					yield return null;
				}
			}
		}
		GenerateTerrainMesh();
		onLevelGenerated?.Invoke(Level);
	}

	private void Initialize() {
		if (terrainRegionsToInclude == null) terrainRegionsToInclude = new List<LevelRegionType>() { LevelRegionType.AboveWater, LevelRegionType.EnchantedForest };
		// Initialize regions dictionaries
		terrainRegionsDict = new Dictionary<LevelRegionType, LevelRegion>();
		if (terrainRegions == null) terrainRegions = new List<LevelRegion>();
		foreach (var region in terrainRegions) {
			terrainRegionsDict.Add(region.regionType, region);
		}
		trackRegionsDict = new Dictionary<LevelRegionType, LevelRegion>();
		if (trackRegions == null) trackRegions = new List<LevelRegion>();
		foreach (var region in trackRegions) {
			trackRegionsDict.Add(region.regionType, region);
		}
		// Initialize level representation
		Level = new LevelRepresentation(dimensions, pointOffset, terrainRegionsDict, trackRegionsDict, terrainRegionsToInclude, regionsAvailability, regionsVisited, blockSizeInPoints);
	}

	private void GenerateTerrainMesh() {
		UtilsMonoBehaviour.RemoveAllChildren(terrainParent);
		for (int x = 0; x < Level.terrain.blockCount.x; x++) {
			for (int y = 0; y < Level.terrain.blockCount.y; y++) {
				// Instantiate a new terrain block from a prefab as a child of Terrain object
				TerrainBlock instantiatedBlock = Instantiate<TerrainBlock>(terrainBlockPrefab, terrainParent);
				instantiatedBlock.Initialize();
				instantiatedBlock.GenerateTerrainMesh(Level.terrain, x, y, terrainRegionsDict);
			}
		}
	}

}

public abstract class LevelGeneratorModule : MonoBehaviour {
	public abstract void Generate(LevelRepresentation level);
}

[System.Serializable]
public class LevelGeneratorModuleSlot {
	public LevelGeneratorModule module;
	public bool isModuleEnabled = true;
}

// Holds all the important information necessary for the level generation
public class LevelRepresentation {
	// TODO: Change access modifiers to better describe use cases

	// Terrain, track and level features
	public TerrainRepresentation terrain;
	public List<TrackPoint> track;
	public List<BonusSpot> bonuses;
	public Vector3 playerStartPosition;
	public FinishLine finish;

	// Dimensions and resolution
	public Vector2 dimensions = new Vector2(50, 50); // Dimensions of the terrain in the X and Z axes. Final dimensions will be determined as the closest larger multiple of pointOffset.
	public float pointOffset = 0.5f; // Distance between two adjacent points in the grid.
	public Vector2Int pointCount; // Number of points on the grid in the X and Z axes

	// Available regions
	public Dictionary<LevelRegionType, LevelRegion> terrainRegions;
	public Dictionary<LevelRegionType, LevelRegion> trackRegions;
	public List<LevelRegionType> terrainRegionsToInclude;
	public Dictionary<LevelRegionType, bool> regionsAvailability; // true if the region may be used in the level
	public Dictionary<LevelRegionType, bool> regionsVisited; // true if the region has been visited (and so is not prioritized when generating a level)

	// Regions actually in the generated level
	public HashSet<LevelRegionType> regionsInLevel;


	public LevelRepresentation(Vector2 dimensions, float pointOffset, Dictionary<LevelRegionType, LevelRegion> terrainRegions, Dictionary<LevelRegionType, LevelRegion> trackRegions, List<LevelRegionType> terrainRegionsToInclude, Dictionary<LevelRegionType, bool> regionsAvailability, Dictionary<LevelRegionType, bool> regionsVisited, int blockSizeInPoints) {
		// Terrain
		this.dimensions = dimensions;
		this.pointOffset = pointOffset;
		ComputeDependentParameters(); // pointCount
		this.terrainRegionsToInclude = terrainRegionsToInclude;
		this.regionsAvailability = regionsAvailability;
		this.regionsVisited = regionsVisited;
		InitializeRegionDictionaries(terrainRegions, trackRegions);
		this.terrain = new TerrainRepresentation(dimensions, pointOffset, blockSizeInPoints);
		this.regionsInLevel = new HashSet<LevelRegionType>();

		// Track
		track = new List<TrackPoint>();
	}

	public void ResetLevel() {
		// Reset terrain points
		for (int x = 0; x < pointCount.x; x++) {
			for (int y = 0; y < pointCount.y; y++) {
				terrain[x, y].Reset();
			}
		}
		// Reset track
		track.Clear();
	}

	public void ResetLevelWithDimensions(Vector2 dimensions, float pointOffset, int blockSizeInPoints) {
		this.dimensions = dimensions;
		this.pointOffset = pointOffset;
		ComputeDependentParameters(); // pointCount
		// Reset terrain and track
		terrain.UpdateParameters(dimensions, pointOffset, blockSizeInPoints);
		terrain.ResetTerrain();
		track.Clear();
	}

	// May be used to change dimensions during generation (e.g. to adapt terrain dimensions to the track dimensions)
	public void ChangeDimensions(Vector2 dimensions) {
		// Store old parameters
		Vector2Int oldPointCount = this.pointCount;
		// Update parameters
		this.dimensions = dimensions;
		ComputeDependentParameters();
		// Update terrain
		terrain.ChangeDimensions(dimensions);
	}

	// Returns terrain point which is the closest one to the given position
	public TerrainPoint GetNearestTerrainPoint(Vector3 fromPosition) {
		// Get indices of the nearest terrain point
		Vector2Int indices = GetNearestGridPoint(fromPosition);
		// Return terrain point with the corresponding indices
		return terrain[indices.x, indices.y];
	}

	public Vector2Int GetNearestGridPoint(Vector3 fromPosition) {
		// Make sure fromPosition is within the grid
		Vector3 bottomLeft = terrain[0, 0].position;
		Vector3 topRight = terrain[pointCount.x - 1, pointCount.y - 1].position;
		fromPosition = new Vector3(Mathf.Clamp(fromPosition.x, bottomLeft.x, topRight.x), fromPosition.y, Mathf.Clamp(fromPosition.z, bottomLeft.z, topRight.z));
		// Get indices of the nearest terrain point
		int i = Mathf.RoundToInt(Mathf.Abs(fromPosition.x - bottomLeft.x) / pointOffset);
		int j = Mathf.RoundToInt(Mathf.Abs(fromPosition.z - bottomLeft.z) / pointOffset);
		// Return terrain point with the corresponding indices
		return new Vector2Int(i, j);
	}

	private void ComputeDependentParameters() {
		// Compute parameters which are not set from outside
		this.pointCount = new Vector2Int(Mathf.CeilToInt(dimensions.x / pointOffset) + 1, Mathf.CeilToInt(dimensions.y / pointOffset) + 1); // multiple of pointOffset which is the closest larger number than the given dimensions
		// Update dimensions to the final ones
		dimensions = new Vector2((pointCount.x - 1) * pointOffset, (pointCount.y - 1) * pointOffset);
	}

	private void InitializeRegionDictionaries(Dictionary<LevelRegionType, LevelRegion> terrainRegions, Dictionary<LevelRegionType, LevelRegion> trackRegions) {
		this.terrainRegions = terrainRegions;
		if (this.terrainRegions == null) this.terrainRegions = new Dictionary<LevelRegionType, LevelRegion>();
		this.trackRegions = trackRegions;
		if (trackRegions == null) this.trackRegions = new Dictionary<LevelRegionType, LevelRegion>();
		// Store all the regions in the dictionary
		foreach (var region in terrainRegions) {
			// If the region is not in the availability dictionary, it is not allowed
			if (!regionsAvailability.ContainsKey(region.Key)) {
				regionsAvailability.Add(region.Key, false);
			}
		}
		foreach (var region in trackRegions) {
			// If the region is not in the availability dictionary, it is not allowed
			if (!regionsAvailability.ContainsKey(region.Key)) {
				regionsAvailability.Add(region.Key, false);
			}
		}
	}

}

public class TerrainPoint {
	public Vector3 position;
	public Color color;
	public LevelRegionType region;
	public bool isOnBorder;

	private Vector3 origPosition;

	public TerrainPoint(Vector3 position) {
		origPosition = position;
		Reset();
	}

	public void Reset() {
		position = origPosition;
		color = Color.black;
		region = LevelRegionType.NONE;
		isOnBorder = false;
	}
}

public class TrackPoint {
	public Vector2Int gridCoords;
	public Vector3 position;
	public bool isCheckpoint;
	public LevelRegionType trackRegion;
	public Hoop assignedHoop;

	public TrackPoint() {
		this.gridCoords = Vector2Int.zero;
		this.position = Vector3.zero;
		this.isCheckpoint = false;
	}

	public TrackPoint(Vector2Int gridCoords, Vector3 position, bool isCheckpoint) {
		this.gridCoords = gridCoords;
		this.position = position;
		this.isCheckpoint = isCheckpoint;
	}
}

public class BonusSpot {
	public Vector3 position;
	public Vector2Int gridCoords; // closest terrain grid point
	public bool isEmpty = true;
	public int previousHoopIndex;
	public float distanceFraction; // how far is the bonus spot between a pair of checkpoints (e.g. 1/4), used for linear interpolation of position
	public List<BonusEffect> bonusInstances;

	public BonusSpot(Vector3 position, int previousHoopIndex, float distanceFraction) {
		this.position = position;
		this.previousHoopIndex = previousHoopIndex;
		this.distanceFraction = distanceFraction;
		this.bonusInstances = new List<BonusEffect>();
	}

	public bool IsBonusAvailable() {
		if (isEmpty) return false;
		// Check if at least one instance is available
		foreach (var bonus in bonusInstances) {
			if (bonus.gameObject.activeInHierarchy) return true;
		}
		return false;
	}
}