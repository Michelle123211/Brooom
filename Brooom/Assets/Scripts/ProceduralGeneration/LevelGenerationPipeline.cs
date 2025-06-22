using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;


/// <summary>
/// A pipeline for generating a level, containing a list of individual modules which are run in this order to obtain the final generated level.
/// </summary>
public class LevelGenerationPipeline : MonoBehaviour {

	[Tooltip("These generators/modules will be used in the exact order. Disabled modules are skipped.")]
	[SerializeField] List<LevelGeneratorModuleSlot> modules;

	[Header("Terrain parameters")]
	[Tooltip("Dimensions of the terrain in the X and Z axes. Final dimensions will be determined as the closest larger multiple of pointOffset.")]
	[SerializeField] Vector2 dimensions = new Vector2(50, 50);
	[Tooltip("Distance between two adjacent points of the grid.")]
	[SerializeField] float pointOffset = 0.5f;

	[Header("Block-based parameters")]
	[Tooltip("Terrain is divided into blocks of this size (i.e. number of terrain points on one side) to allow for optimizations.")]
	[SerializeField] int blockSizeInPoints = 25;
	[Tooltip("Parent object of all terrain blocks.")]
	[SerializeField] Transform terrainParent;
	[Tooltip("Prefab containing all necessary components for terrain block")]
	[SerializeField] TerrainBlock terrainBlockPrefab;

	[Header("Level regions")]
	[Tooltip("All terrain regions in the game together with their data.")]
	public List<LevelRegion> terrainRegions;
	[Tooltip("All track regions in the game together with their data.")]
	public List<LevelRegion> trackRegions;
	/// <summary>Dictionary denoting for each region whether it is available to the player and may be used in the level.</summary>
	public Dictionary<LevelRegionType, bool> regionsAvailability;
	/// <summary>Dictionary denoting for each region whether it has already been visited by the player. Unvisited (but available) regions are prioritized when generating a level.</summary>
	public Dictionary<LevelRegionType, bool> regionsVisited;

	/// <summary>Terrain regions which can be used when generating the level (for track regions, all available ones will be used).</summary>
	[HideInInspector] public List<LevelRegionType> terrainRegionsToInclude; // Fill from outside, work with only these when generating level


	/// <summary>Called when the level generation has finished.</summary>
	public event Action<LevelRepresentation> onLevelGenerated;


	// Object-oriented representation
	/// <summary>Object representation of the whole level, including terrain and track.</summary>
	public LevelRepresentation Level { get; private set; }
	private Dictionary<LevelRegionType, LevelRegion> terrainRegionsDict; // all terrain regions in the game
	private Dictionary<LevelRegionType, LevelRegion> trackRegionsDict; // all track regions in the game

	/// <summary>
	/// Enables or disables a level generation module on the given index with the pipeline.
	/// </summary>
	/// <param name="index">Index of the module to be enabled/disabled.</param>
	/// <param name="enable"><c>true</c> if the module should be enabled, <c>false</c> if disabled.</param>
	public void EnableOrDisableModule(int index, bool enable) {
		modules[index].isModuleEnabled = enable;
	}

#if UNITY_EDITOR
	/// <summary>
	/// Regenerates the level with previously set parameters and a hardcoded set of regions. It is used mostly for debugging purposes.
	/// </summary>
	[ContextMenu("Regenerate level")]
	private void RegenerateLevel() { // Regenerates the level with previous parameters + parameters specified below
		if (Level == null) {
			if (terrainRegionsToInclude == null || terrainRegionsToInclude.Count == 0) {
				terrainRegionsToInclude = new List<LevelRegionType>() { LevelRegionType.AboveWater, LevelRegionType.EnchantedForest }; // TODO: Change if necessary
				if (regionsAvailability == null) regionsAvailability = new Dictionary<LevelRegionType, bool>();
				// TODO: Change if necessary
				regionsAvailability[LevelRegionType.AboveWater] = true;
				regionsAvailability[LevelRegionType.EnchantedForest] = true;
			}
			Initialize();
		}
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

	/// <summary>
	/// Generates the level with the current parameters (set in level generation pipeline and in individual modules).
	/// </summary>
	/// <returns>Yields control back inbetween modules to allow other code to run (e.g. animation of loading screen).</returns>
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

	// Initializes regions dictionaries for easier lookup, and level representation
	private void Initialize() {
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

	// Instantiates all terrain blocks and makes them generate a terrain mesh
	private void GenerateTerrainMesh() {
		UtilsMonoBehaviour.RemoveAllChildren(terrainParent);
		for (int x = 0; x < Level.Terrain.blockCount.x; x++) {
			for (int y = 0; y < Level.Terrain.blockCount.y; y++) {
				// Instantiate a new terrain block from a prefab as a child of Terrain object
				TerrainBlock instantiatedBlock = Instantiate<TerrainBlock>(terrainBlockPrefab, terrainParent);
				instantiatedBlock.Initialize();
				instantiatedBlock.GenerateTerrainMesh(Level.Terrain, x, y, terrainRegionsDict);
			}
		}
	}

}

/// <summary>
/// A base class for a representation of a single module responsible for generating a particular part of level.
/// </summary>
public abstract class LevelGeneratorModule : MonoBehaviour {
	/// <summary>
	/// Generates a particular part of the level for which this module is responsible.
	/// </summary>
	/// <param name="level">Initial level representation to be modified during the generation process.</param>
	public abstract void Generate(LevelRepresentation level);
}

/// <summary>
/// A class representing a slot for a module within a level generation pipeline.
/// A module may be assigned to it and then it may be enabled or disabled.
/// </summary>
[System.Serializable]
public class LevelGeneratorModuleSlot {
	[Tooltip("A name which is displayed in the Inspector when this slot is in a list of slots used in level generation pipeline.")]
	[SerializeField] string displayName;
	[Tooltip("Module assigned to the slot.")]
	public LevelGeneratorModule module;
	[Tooltip("Whether this module should be enabled in the level generation pipeline, or not (then it is ignored and skipped).")]
	public bool isModuleEnabled = true;
}


/// <summary>
/// This class represents a whole level, together with the terrain and track.
/// It holds all information necessary for generating a level.
/// </summary>
public class LevelRepresentation {

	// Terrain, track and level features
	/// <summary>Object representation of the terrain.</summary>
	public TerrainRepresentation Terrain { get; private set; }
	/// <summary>A list of track points (hoops/checkpoints) representations with all important data.</summary>
	public List<TrackPoint> Track { get; private set; }
	/// <summary>A list of bonus spots (spawn points) representations with all important data.</summary>
	public List<BonusSpot> bonuses;
	/// <summary>World position where the player is placed at the start of the race.</summary>
	public Vector3 playerStartPosition;
	/// <summary>Finish line object instantiated at the end of the track.</summary>
	public FinishLine finish;

	// Dimensions and resolution
	/// <summary>Dimensions of the terrain in the X and Z axes. Final dimensions will be determined as the closest larger multiple of <c>pointOffset</c>.</summary>
	public Vector2 dimensions = new Vector2(50, 50);
	/// <summary>Distance between two adjacent points of the grid.</summary>
	public float pointOffset = 0.5f;
	/// <summary>Number of points on the grid in the X and Z axes.</summary>
	public Vector2Int pointCount;

	// Available regions
	/// <summary>All terrain regions in the game together with their data, stored in a dictionary under the <c>LevelRegionType</c> they represent as a key.</summary>
	public Dictionary<LevelRegionType, LevelRegion> TerrainRegions { get; private set; }
	/// <summary>All track regions in the game together with their data, stored in a dictionary under the <c>LevelRegionType</c> they represent as a key.</summary>
	public Dictionary<LevelRegionType, LevelRegion> TrackRegions { get; private set; }
	/// <summary>Terrain regions which can be included in the generated level.</summary>
	public List<LevelRegionType> TerrainRegionsToInclude { get; private set; }
	/// <summary>Dictionary denoting for each region whether it is available to the player and may be used in the level.</summary>
	public Dictionary<LevelRegionType, bool> RegionsAvailability { get; private set; }
	/// <summary>Dictionary denoting for each region whether it has already been visited by the player.</summary>
	public Dictionary<LevelRegionType, bool> RegionsVisited { get; private set; }

	/// <summary>A set of regions (terrain and track) which are actually located in the generated level.</summary>
	public HashSet<LevelRegionType> RegionsInLevel { get; private set; }


	/// <summary>
	/// Creates a new representation of the level with the given parameters.
	/// </summary>
	/// <param name="dimensions">Dimensions of the terrain in the X and Z axes. Final dimensions will be determined as the closest larger multiple of <c>pointOffset</c>.</param>
	/// <param name="pointOffset">Distance between two adjacent points of the terrain grid.</param>
	/// <param name="terrainRegions">All terrain regions in the game together with their data (<c>LevelRegionType</c> they represent is a key).</param>
	/// <param name="trackRegions">All track regions in the game together with their data (<c>LevelRegionType</c> they represent is a key).</param>
	/// <param name="terrainRegionsToInclude">Terrain regions which can be included in the generated level.</param>
	/// <param name="regionsAvailability">Dictionary denoting for each region whether it is available to the player.</param>
	/// <param name="regionsVisited">Dictionary denoting for each region whether it has already been visited by the player.</param>
	/// <param name="blockSizeInPoints">Terrain grid is divided into blocks which have this many points in each direction.</param>
	public LevelRepresentation(
			Vector2 dimensions, float pointOffset, Dictionary<LevelRegionType, LevelRegion> terrainRegions, Dictionary<LevelRegionType, LevelRegion> trackRegions,
			List<LevelRegionType> terrainRegionsToInclude, Dictionary<LevelRegionType, bool> regionsAvailability, Dictionary<LevelRegionType, bool> regionsVisited,
			int blockSizeInPoints) {
		// Terrain
		this.dimensions = dimensions;
		this.pointOffset = pointOffset;
		ComputeDependentParameters(); // pointCount
		this.TerrainRegionsToInclude = terrainRegionsToInclude;
		this.RegionsAvailability = regionsAvailability;
		this.RegionsVisited = regionsVisited;
		InitializeRegionDictionaries(terrainRegions, trackRegions);
		this.Terrain = new TerrainRepresentation(dimensions, pointOffset, blockSizeInPoints);
		this.RegionsInLevel = new HashSet<LevelRegionType>();

		// Track
		Track = new List<TrackPoint>();
	}

	/// <summary>
	/// Resets all already generated terrain points to their initial values and positions, removes all track points.
	/// </summary>
	public void ResetLevel() {
		// Reset terrain points
		for (int x = 0; x < pointCount.x; x++) {
			for (int y = 0; y < pointCount.y; y++) {
				Terrain[x, y].Reset();
			}
		}
		// Reset track
		Track.Clear();
	}

	/// <summary>
	/// Resets all terrain points to their initial values and positions, and removes all track points,
	/// while changing terrain dimensions according to the given parameters.
	/// </summary>
	/// <param name="dimensions">New dimensions of the terrain in the X and Z axes. Final dimensions will be determined as the closest larger multiple of <c>pointOffset</c>.</param>
	/// <param name="pointOffset">Distance between two adjacent points of the terrain grid.</param>
	/// <param name="blockSizeInPoints">Terrain grid is divided into blocks which have this many points in each direction.</param>
	public void ResetLevelWithDimensions(Vector2 dimensions, float pointOffset, int blockSizeInPoints) {
		this.dimensions = dimensions;
		this.pointOffset = pointOffset;
		ComputeDependentParameters(); // pointCount
		// Reset terrain and track
		Terrain.UpdateParameters(dimensions, pointOffset, blockSizeInPoints);
		Track.Clear();
	}

	/// <summary>
	/// Changes dimensions of the terrain and recomputes all dependent parameters,
	/// while keeping the bottom-left corner (<c>TerrainPoint</c>s already existing in the grid  with all of their properties).
	/// It may be used to change dimensions during generation (e.g. to adapt terrain dimensions to the track dimensions).
	/// </summary>
	/// <param name="dimensions">New dimensions of the terrain in the X and Z axes. Final dimensions will be determined as the closest larger multiple of <c>pointOffset</c>.</param>
	public void ChangeDimensions(Vector2 dimensions) {
		// Store old parameters
		Vector2Int oldPointCount = this.pointCount;
		// Update parameters
		this.dimensions = dimensions;
		ComputeDependentParameters();
		// Update terrain
		Terrain.ChangeDimensions(dimensions);
	}

	/// <summary>
	/// Finds the <c>TerrainPoint</c> which is the closest one to the given world position.
	/// </summary>
	/// <param name="fromPosition">World position to which nearest terrain point should be found.</param>
	/// <returns>Terrain point which is the closest one to the given position.</returns>
	public TerrainPoint GetNearestTerrainPoint(Vector3 fromPosition) {
		// Get indices of the nearest terrain point
		Vector2Int indices = GetNearestGridPoint(fromPosition);
		// Return terrain point with the corresponding indices
		return Terrain[indices.x, indices.y];
	}

	/// <summary>
	/// Finds the point on the terrain grid which is the closest one to the given world position.
	/// </summary>
	/// <param name="fromPosition">World position to which nearest terrain grid point should be found.</param>
	/// <returns>Indices of the terrain grid point which is the closest one to the given position.</returns>
	public Vector2Int GetNearestGridPoint(Vector3 fromPosition) {
		// Make sure fromPosition is within the grid
		Vector3 bottomLeft = Terrain[0, 0].position;
		Vector3 topRight = Terrain[pointCount.x - 1, pointCount.y - 1].position;
		fromPosition = new Vector3(Mathf.Clamp(fromPosition.x, bottomLeft.x, topRight.x), fromPosition.y, Mathf.Clamp(fromPosition.z, bottomLeft.z, topRight.z));
		// Get indices of the nearest terrain point
		int i = Mathf.RoundToInt(Mathf.Abs(fromPosition.x - bottomLeft.x) / pointOffset);
		int j = Mathf.RoundToInt(Mathf.Abs(fromPosition.z - bottomLeft.z) / pointOffset);
		// Return terrain point with the corresponding indices
		return new Vector2Int(i, j);
	}

	// Computes values of the parameters which are not set from outside but are dependent on the ones which are
	private void ComputeDependentParameters() {
		this.pointCount = new Vector2Int(Mathf.CeilToInt(dimensions.x / pointOffset) + 1, Mathf.CeilToInt(dimensions.y / pointOffset) + 1); // multiple of pointOffset which is the closest larger number than the given dimensions
		// Update dimensions to the final ones
		dimensions = new Vector2((pointCount.x - 1) * pointOffset, (pointCount.y - 1) * pointOffset);
	}

	// Makes sure all regions (terrain and track) have their value in availability dictionary
	private void InitializeRegionDictionaries(Dictionary<LevelRegionType, LevelRegion> terrainRegions, Dictionary<LevelRegionType, LevelRegion> trackRegions) {
		this.TerrainRegions = terrainRegions;
		if (this.TerrainRegions == null) this.TerrainRegions = new Dictionary<LevelRegionType, LevelRegion>();
		this.TrackRegions = trackRegions;
		if (trackRegions == null) this.TrackRegions = new Dictionary<LevelRegionType, LevelRegion>();
		// Store all the regions in the dictionary
		foreach (var region in terrainRegions) {
			// If the region is not in the availability dictionary, it is not allowed
			if (!RegionsAvailability.ContainsKey(region.Key)) {
				RegionsAvailability.Add(region.Key, false);
			}
		}
		foreach (var region in trackRegions) {
			// If the region is not in the availability dictionary, it is not allowed
			if (!RegionsAvailability.ContainsKey(region.Key)) {
				RegionsAvailability.Add(region.Key, false);
			}
		}
	}

}

/// <summary>
/// An object representation of a single track point (hoop/checkpoint) in a array of points representing the whole track.
/// It contains information necessary for track generation, e.g. world position, assigned track region, whether it is hoop or checkpoint.
/// </summary>
public class TrackPoint {

	/// <summary>Coordinates of a terrain grid point above which this hoop/checkpoint is placed (track points are snapped to grid).</summary>
	public Vector2Int gridCoords;
	/// <summary>World position where the hoop/checkpoint is placed.</summary>
	public Vector3 position;
	/// <summary>Whether it is a checkpoint, or just a simple hoop.</summary>
	public bool isCheckpoint;
	/// <summary>A track region this point belongs to, if any.</summary>
	public LevelRegionType trackRegion;
	/// <summary>A <c>Hoop</c> object assigned to this track point.</summary>
	public Hoop assignedHoop;

	/// <summary>
	/// Creates a <c>TrackPoint</c> instance representing a hoop/checkpoint with initial values.
	/// </summary>
	public TrackPoint() {
		this.gridCoords = Vector2Int.zero;
		this.position = Vector3.zero;
		this.isCheckpoint = false;
	}

	/// <summary>
	/// Creates a <c>TrackPoint</c> instance representing a hoop/checkpoint with the given parameters.
	/// </summary>
	/// <param name="gridCoords">Coordinates of a terrain grid point above which this hoop/checkpoint is placed (track points are snapped to grid).</param>
	/// <param name="position">World position where the hoop/checkpoint is placed.</param>
	/// <param name="isCheckpoint">Whether it is a checkpoint, or just a simple hoop.</param>
	public TrackPoint(Vector2Int gridCoords, Vector3 position, bool isCheckpoint) {
		this.gridCoords = gridCoords;
		this.position = position;
		this.isCheckpoint = isCheckpoint;
	}
}

/// <summary>
/// An object representation of a single spot where several bonus instances are spawned.
/// It contains information necessary for track generation, e.g. world position, index of the previous hoop on track.
/// </summary>
public class BonusSpot {

	/// <summary>World position where the bonuses belonging to this spot are spawned.</summary>
	public Vector3 position;
	/// <summary>Coordinates of the closest terrain grid point above which this bonus spot is placed (it is not snapped to grid).</summary>
	public Vector2Int gridCoords;
	/// <summary>Whether this bonus spot is empty, i.e. not containing any bonus instances.</summary>
	public bool isEmpty = true;
	/// <summary>Bonus spots are always placed between a pair of hoops. This is index of the hoop which is before it.</summary>
	public int previousHoopIndex;
	/// <summary>Number between 0 and 1 denoting how far the bonus spot is between a pair of hoops (e.g. 1/4). It is used for linear interpolation of position.</summary>
	public float distanceFraction;
	/// <summary><c>BonusEffect</c> instances spawned on this bonus spot.</summary>
	public List<BonusEffect> bonusInstances;

	/// <summary>
	/// Creates an instance of an empty bonus spot.
	/// </summary>
	/// <param name="position">World position where the bonuses belonging to this spot are spawned.</param>
	/// <param name="previousHoopIndex">Bonus spots are always placed between a pair of hoops. This is index of the hoop which is before it.</param>
	/// <param name="distanceFraction">Number between 0 and 1 denoting how far the bonus spot is between a pair of hoops.</param>
	public BonusSpot(Vector3 position, int previousHoopIndex, float distanceFraction) {
		this.position = position;
		this.previousHoopIndex = previousHoopIndex;
		this.distanceFraction = distanceFraction;
		this.bonusInstances = new List<BonusEffect>();
	}

	/// <summary>
	/// Checks whether there is at least one bonus instance, spawned in this bonus spot, which is active in the scene.
	/// </summary>
	/// <returns><c>true</c> if there is at least one bonus instance active in this bonus spot, <c>false</c> otherwise.</returns>
	public bool IsBonusAvailable() {
		if (isEmpty) return false;
		// Check if at least one instance is available
		foreach (var bonus in bonusInstances) {
			if (bonus.gameObject.activeInHierarchy) return true;
		}
		return false;
	}

}