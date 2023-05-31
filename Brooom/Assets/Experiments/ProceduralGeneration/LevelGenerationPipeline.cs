using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[RequireComponent(typeof(MeshFilter))]
public class LevelGenerationPipeline : MonoBehaviour
{
	[Tooltip("These generators will be used in the exact order.")]
	public List<LevelGeneratorModule> modules;

	[Header("Terrain parameters")]
	[Tooltip("Dimensions of the terrain in the X and Z axes. Final dimensions will be determined as the closest larger multiple of pointOffset.")]
	public Vector2 dimensions = new Vector2(50, 50);
	[Tooltip("Minimum and maximum Y coordinate of the terrain.")]
	public Vector2 heightRange = new Vector2(-5, 10);
	[Tooltip("Distance between two adjacent points in the grid.")]
	public float pointOffset = 0.5f;

	[Header("Map regions")]
	[Tooltip("All regions available in the game.")]
	public List<MapRegion> allRegions;
	[Tooltip("Regions allowed in the generated level.")]
	public List<MapRegionType> allowedRegions;


	// Object-oriented representation
	private LevelRepresentation level;
	private Dictionary<MapRegionType, MapRegion> regions;

	// Mesh and its data
	Mesh mesh;
	Vector3[] vertices;
	int[] triangles;
	Color[] colors;

	[ContextMenu("Regenerate")]
	private void RegenerateLevel() { // Regenerates the level with previous parameters
		if (modules == null) return;
		level.ResetLevel();
		foreach (var module in modules) {
			module.Generate(level);
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

	public void GenerateLevel() { // Generates the level with the current parameters
		if (modules == null) return;
		Initialize();
		foreach (var module in modules) {
			module.Generate(level);
		}
		CreateMeshData();
		ConvertMeshFromSmoothToFlat();
		UpdateMesh();
	}

	private void Start() {
		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;

		GenerateLevel();
	}

	private void Initialize() {
		// Initialize regions dictionary
		regions = new Dictionary<MapRegionType, MapRegion>();
		if (allRegions == null)
			allRegions = new List<MapRegion>();
		foreach (var region in allRegions) {
			regions.Add(region.regionType, region);
		}
		// Initialize level representation
		level = new LevelRepresentation(dimensions, heightRange, pointOffset, regions, allowedRegions);
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
				if (regions.TryGetValue(level.terrain[x, y].region, out MapRegion region)) {
					colors[level.terrain[x, y].vertexIndex] = region.color;
				} else {
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

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.colors = colors;

		mesh.RecalculateNormals();
	}

	private void OnDrawGizmos() {
		// Vertices
		Gizmos.color = Color.black;
		if (vertices != null) {
			for (int i = 0; i < vertices.Length; i++) {
				Gizmos.DrawSphere(vertices[i], 0.05f);
			}
		}
		// Edges
		Gizmos.color = Color.red;
		if (triangles != null) {
			for (int i = 0; i < triangles.Length; i += 3) {
				Gizmos.DrawLine(vertices[triangles[i]], vertices[triangles[i + 1]]);
				Gizmos.DrawLine(vertices[triangles[i + 1]], vertices[triangles[i + 2]]);
				Gizmos.DrawLine(vertices[triangles[i + 2]], vertices[triangles[i]]);
			}
		}
	}


}

public abstract class LevelGeneratorModule : MonoBehaviour {
	public abstract void Generate(LevelRepresentation level);
}

// Holds all the important information necessary for the level generation
public class LevelRepresentation {
	// TODO: Change access modifiers to better describe use cases

	// Terrain, track and level features
	public TerrainPoint[,] terrain;
	// TODO: track

	// Dimensions and resolution
	public Vector2 dimensions = new Vector2(50, 50); // Dimensions of the terrain in the X and Z axes. Final dimensions will be determined as the closest larger multiple of pointOffset.
	public float pointOffset = 0.5f; // Distance between two adjacent points in the grid.											 
	public Vector2 heightRange = new Vector2(-5, 10); // Minimum and maximum Y coordinate of the terrain.
	public Vector2Int pointCount; // Number of points on the grid in the X and Z axes
	public Vector2 startPosition; // Position of the bottom left point of Mesh

	// Available regions
	public Dictionary<MapRegionType, MapRegion> regions;
	public Dictionary<MapRegionType, bool> regionsAvailability; // true if the region may be used in the level


	public LevelRepresentation(Vector2 dimensions, Vector2 heightRange, float pointOffset, Dictionary<MapRegionType, MapRegion> allRegions, List<MapRegionType> allowedRegions) {
		this.dimensions = dimensions;
		this.heightRange = heightRange;
		this.pointOffset = pointOffset;
		// Compute other parameters
		this.pointCount = new Vector2Int(Mathf.CeilToInt(dimensions.x / pointOffset) + 1, Mathf.CeilToInt(dimensions.y / pointOffset) + 1); // multiple of pointOffset which is the closest larger number than the given dimensions
		this.startPosition = new Vector2(-(float)(pointCount.x - 1) * pointOffset / 2, -(float)(pointCount.y - 1) * pointOffset / 2); // centre is in zero, distance between adjacent points is pointOffset
		InitializeRegionDictionaries(allRegions, allowedRegions);
		
		// TODO: track

		InitializeTerrain();
	}

	public void ResetLevel() {
		for (int x = 0; x < pointCount.x; x++) {
			for (int y = 0; y < pointCount.y; y++) {
				terrain[x, y].Reset();
			}
		}
	}

	private void InitializeRegionDictionaries(Dictionary<MapRegionType, MapRegion> allRegions, List<MapRegionType> allowedRegions) {
		this.regions = allRegions;
		if (this.regions == null) this.regions = new Dictionary<MapRegionType, MapRegion>();
		this.regionsAvailability = new Dictionary<MapRegionType, bool>();
		// Note all the allowed regions in the dictionary
		if (allowedRegions != null) {
			foreach (var allowedRegion in allowedRegions) {
				this.regionsAvailability.Add(allowedRegion, true);
			}
		}
		// Store all the regions in the dictionary
		foreach (var region in allRegions) {
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
				terrain[x, y] = new TerrainPoint(new Vector3(startPosition.x + x * pointOffset, 0, startPosition.y + y * pointOffset), i);
				i++;
			}
		}
	}

}

public class TerrainPoint {
	public int vertexIndex;
	public Vector3 position;
	public Color color;
	public MapRegionType region;
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
		region = MapRegionType.NONE;
		isOnBorder = false;
	}
}