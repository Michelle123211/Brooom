using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class LevelGenerationPipeline : MonoBehaviour
{
	[Tooltip("These generators/modules will be used in the exact order. Disabled module is not executed even if part of the pipeline.")]
	public List<LevelGeneratorModuleSlot> modules;

	[Header("Terrain parameters")]
	[Tooltip("Dimensions of the terrain in the X and Z axes. Final dimensions will be determined as the closest larger multiple of pointOffset.")]
	public Vector2 dimensions = new Vector2(50, 50);
	[Tooltip("Distance between two adjacent points in the grid.")]
	public float pointOffset = 0.5f;

	[Header("Level regions")]
	[Tooltip("All terrain regions in the game.")]
	public List<LevelRegion> terrainRegions;
	[Tooltip("All track regions in the game.")]
	public List<LevelRegion> trackRegions;
	[Tooltip("Regions allowed in the currently generated level.")]
	public List<LevelRegionType> allowedRegions; // TODO: Remove once they can be determined from PlayerState

	[Header("Debug")]
	public bool showVertices = false;
	public bool showEdges = false;
	public bool showBorders = false;


	// Object-oriented representation
	private LevelRepresentation level;
	private Dictionary<LevelRegionType, LevelRegion> terrainRegionsDict;
	private Dictionary<LevelRegionType, LevelRegion> trackRegionsDict;

	// Mesh and its data
	Mesh mesh;
	Vector3[] vertices;
	int[] triangles;
	Color[] colors;

	MeshCollider meshCollider;

#if UNITY_EDITOR
	[ContextMenu("Regenerate")]
	private void RegenerateLevel() { // Regenerates the level with previous parameters
		if (modules == null) return;
		level.ResetLevelWithDimensions(dimensions, pointOffset);
		foreach (var moduleSlot in modules) {
			if (moduleSlot.isModuleEnabled)
				moduleSlot.module.Generate(level);
		}
		CreateMeshData();
		ConvertMeshFromSmoothToFlat();
		UpdateMesh();
	}

	[ContextMenu("Save Mesh")]
	private void SaveMesh() {
		if (mesh == null) return;
		// Get path from the user
		string path = EditorUtility.SaveFilePanel("Save Mesh Asset", "Assets/Models/Terrain/", "GeneratedTerrainExample", "asset");
		if (string.IsNullOrEmpty(path)) return;
		// Convert the returned absolute path to project-relative to use with AssetDatabase
		path = FileUtil.GetProjectRelativePath(path);
		// Optimize the mesh (changes ordering of the geometry and vertices to improve vertex cache utilisation)
		MeshUtility.Optimize(mesh);
		// Save the asset
		AssetDatabase.CreateAsset(mesh, path);
		AssetDatabase.SaveAssets();
	}
#endif

	public LevelRepresentation GenerateLevel() { // Generates the level with the current parameters
		Initialize();
		if (modules != null) {
			foreach (var moduleSlot in modules) {
				if (moduleSlot.isModuleEnabled)
					moduleSlot.module.Generate(level);
			}
		}
		CreateMeshData();
		ConvertMeshFromSmoothToFlat();
		UpdateMesh();
		return level;
	}

	private void Awake() {
		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		meshCollider = GetComponent<MeshCollider>();
	}

	private void Initialize() {
		// Initialize regions dictionaries
		terrainRegionsDict = new Dictionary<LevelRegionType, LevelRegion>();
		if (terrainRegions == null)
			terrainRegions = new List<LevelRegion>();
		foreach (var region in terrainRegions) {
			terrainRegionsDict.Add(region.regionType, region);
		}
		trackRegionsDict = new Dictionary<LevelRegionType, LevelRegion>();
		foreach (var region in trackRegions) {
			trackRegionsDict.Add(region.regionType, region);
		}
		// Initialize level representation
		level = new LevelRepresentation(dimensions, pointOffset, terrainRegionsDict, trackRegionsDict, allowedRegions);
	}

	private void CreateMeshData() {
		// Vertices
		vertices = new Vector3[level.pointCount.x * level.pointCount.y];
		for (int x = 0; x < level.pointCount.x; x++) {
			for (int y = 0; y < level.pointCount.y; y++) {
				vertices[level.terrain[x, y].vertexIndex] = level.terrain[x, y].position;
			}
		}
		// Triangles
		triangles = new int[(level.pointCount.x - 1) * (level.pointCount.y - 1) * 6];
		for (int x = 0, i = 0; x < level.pointCount.x - 1; x++) {
			for (int y = 0; y < level.pointCount.y - 1; y++) {
				// For each possible lower left corner add a quad composed of two triangles
				triangles[i]	 = level.terrain[x, y].vertexIndex;
				triangles[i + 1] = level.terrain[x, y + 1].vertexIndex;
				triangles[i + 2] = level.terrain[x + 1, y].vertexIndex;
				triangles[i + 3] = level.terrain[x + 1, y].vertexIndex;
				triangles[i + 4] = level.terrain[x, y + 1].vertexIndex;
				triangles[i + 5] = level.terrain[x + 1, y + 1].vertexIndex;
				i += 6;
			}
		}
		// Colors
		colors = new Color[level.pointCount.x * level.pointCount.y];
		for (int x = 0; x < level.pointCount.x - 1; x++) {
			for (int y = 0; y < level.pointCount.y - 1; y++) {
				// Assign the color of the region
				if (terrainRegionsDict.TryGetValue(level.terrain[x, y].region, out LevelRegion region)) {
					colors[level.terrain[x, y].vertexIndex] = region.color;
				} else {
					Debug.Log($"Unknown region: {(int)level.terrain[x, y].region}.");
					colors[level.terrain[x, y].vertexIndex] = Color.black;
				}
			}
		}
	}

	private void ConvertMeshFromSmoothToFlat() { 
		Vector3[] newVertices = new Vector3[triangles.Length];
		int[] newTriangles = new int[triangles.Length];
		Color[] newColors = new Color[triangles.Length];
		// Duplicate vertices so that they are not shared between triangles and the normals are not smoothed out
		for (int i = 0; i < triangles.Length; i++) {
			newVertices[i] = vertices[triangles[i]];
			newColors[i] = colors[triangles[i]];
			newTriangles[i] = i;
		}
		// Replace the previous arrays with the new ones
		vertices = newVertices;
		triangles = newTriangles;
		colors = newColors;
	}

	private void UpdateMesh() {
		mesh.Clear();

		// Choose suitable size of index
		mesh.indexFormat = triangles.Length > 65535 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.colors = colors;

		mesh.RecalculateBounds();
		mesh.RecalculateNormals();

		meshCollider.sharedMesh = mesh; // the Mesh needs to be assigned every time again
	}

	private void OnDrawGizmosSelected() {
		// Vertices
		if (showVertices) {
			Gizmos.color = Color.black;
			if (vertices != null) {
				for (int i = 0; i < vertices.Length; i++) {
					Gizmos.DrawSphere(vertices[i], 0.05f);
				}
			}
		}

		// Edges
		if (showEdges) {
			Gizmos.color = Color.red;
			if (triangles != null) {
				for (int i = 0; i < triangles.Length; i += 3) {
					Gizmos.DrawLine(vertices[triangles[i]], vertices[triangles[i + 1]]);
					Gizmos.DrawLine(vertices[triangles[i + 1]], vertices[triangles[i + 2]]);
					Gizmos.DrawLine(vertices[triangles[i + 2]], vertices[triangles[i]]);
				}
			}
		}

		// Borders
		if (showBorders) {
			Gizmos.color = Color.blue;
			if (level != null) {
				for (int x = 0; x < level.pointCount.x; x++) {
					for (int y = 0; y < level.pointCount.y; y++) {
						if (level.terrain[x, y].isOnBorder) {
							Gizmos.DrawSphere(level.terrain[x, y].position, 0.1f);
						}
					}
				}
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
	public TerrainPoint[,] terrain;
	public List<TrackPoint> track;
	public List<BonusSpot> bonuses;
	public Vector3 playerStartPosition;

	// Dimensions and resolution
	public Vector2 dimensions = new Vector2(50, 50); // Dimensions of the terrain in the X and Z axes. Final dimensions will be determined as the closest larger multiple of pointOffset.
	public float pointOffset = 0.5f; // Distance between two adjacent points in the grid.
	public Vector2Int pointCount; // Number of points on the grid in the X and Z axes
	public Vector2 terrainStartPosition; // Position of the bottom left point of Mesh

	// Available regions
	public Dictionary<LevelRegionType, LevelRegion> terrainRegions;
	public Dictionary<LevelRegionType, LevelRegion> trackRegions;
	public List<LevelRegionType> availableRegions;
	public Dictionary<LevelRegionType, bool> regionsAvailability; // true if the region may be used in the level


	public LevelRepresentation(Vector2 dimensions, float pointOffset, Dictionary<LevelRegionType, LevelRegion> terrainRegions, Dictionary<LevelRegionType, LevelRegion> trackRegions, List<LevelRegionType> allowedRegions) {
		// Terrain
		this.dimensions = dimensions;
		this.pointOffset = pointOffset;
		ComputeDependentParameters(); // pointCount and startPosition
		this.availableRegions = allowedRegions;
		InitializeRegionDictionaries(terrainRegions, trackRegions, allowedRegions);
		InitializeTerrain();

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

	public void ResetLevelWithDimensions(Vector2 dimensions, float pointOffset) {
		this.dimensions = dimensions;
		this.pointOffset = pointOffset;
		ComputeDependentParameters(); // pointCount and startPosition
		// Initialize terrain
		InitializeTerrain();
		// Reset track
		track.Clear();
	}

	// May be used to change dimensions during generation (e.g. to adapt terrain dimensions to the track dimensions)
	public void ChangeDimensions(Vector2 dimensions) {
		// Store old parameters
		Vector2Int oldPointCount = this.pointCount;
		TerrainPoint[,] oldTerrain = this.terrain;

		// Update parameters
		this.dimensions = dimensions;
		ComputeDependentParameters();

		// Update terrain
		InitializeTerrain();
		for (int x = 0; x < Mathf.Min(oldPointCount.x, pointCount.x); x++) { // copy the upper-left corner of the old terrain
			for (int y = 0; y < Mathf.Min(oldPointCount.y, pointCount.y); y++) {
				terrain[x, y].position.y = oldTerrain[x, y].position.y;
				terrain[x, y].region = oldTerrain[x, y].region;
				terrain[x, y].isOnBorder = oldTerrain[x, y].isOnBorder;
				terrain[x, y].color = oldTerrain[x, y].color;
			}
		}
	}

	private void ComputeDependentParameters() {
		// Compute parameters which are not set from outside
		this.pointCount = new Vector2Int(Mathf.CeilToInt(dimensions.x / pointOffset) + 1, Mathf.CeilToInt(dimensions.y / pointOffset) + 1); // multiple of pointOffset which is the closest larger number than the given dimensions
		this.terrainStartPosition = new Vector2(-(float)(pointCount.x - 1) * pointOffset / 2, -(float)(pointCount.y - 1) * pointOffset / 2); // centre is in zero, distance between adjacent points is pointOffset
		// Update dimensions to the final ones
		dimensions = new Vector2((pointCount.x - 1) * pointOffset, (pointCount.y - 1) * pointOffset);
	}

	private void InitializeRegionDictionaries(Dictionary<LevelRegionType, LevelRegion> terrainRegions, Dictionary<LevelRegionType, LevelRegion> trackRegions, List<LevelRegionType> allowedRegions) {
		this.terrainRegions = terrainRegions;
		if (this.terrainRegions == null) this.terrainRegions = new Dictionary<LevelRegionType, LevelRegion>();
		this.trackRegions = trackRegions;
		if (trackRegions == null) this.trackRegions = new Dictionary<LevelRegionType, LevelRegion>();
		// Note all the allowed regions in the dictionary
		this.regionsAvailability = new Dictionary<LevelRegionType, bool>();
		if (allowedRegions != null) {
			foreach (var allowedRegion in allowedRegions) {
				this.regionsAvailability.Add(allowedRegion, true);
			}
		}
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

	private void InitializeTerrain() {
		terrain = new TerrainPoint[pointCount.x, pointCount.y];
		for (int x = 0, i = 0; x < pointCount.x; x++) {
			for (int y = 0; y < pointCount.y; y++) {
				terrain[x, y] = new TerrainPoint(new Vector3(terrainStartPosition.x + x * pointOffset, 0, terrainStartPosition.y + y * pointOffset), i);
				i++;
			}
		}
	}

}

public class TerrainPoint {
	public int vertexIndex;
	public Vector3 position;
	public Color color;
	public LevelRegionType region;
	public bool isOnBorder;

	private Vector3 origPosition;

	public TerrainPoint(Vector3 position, int index) {
		origPosition = position;
		vertexIndex = index;
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
	public GameObject assignedObject;

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

	public BonusSpot(Vector3 position, int previousHoopIndex, float distanceFraction) {
		this.position = position;
		this.previousHoopIndex = previousHoopIndex;
		this.distanceFraction = distanceFraction;
	}
}