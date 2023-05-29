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
	[Tooltip("Regions all regions available in the game.")]
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
	public void GenerateLevel() {
		if (modules == null) return;
		level.ResetLevel();
		foreach (var module in modules) {
			module.Generate(level);
		}
		CreateMeshData();
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

	private void Start() {
		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;

		// Initialize level representation
		level = new LevelRepresentation(dimensions, heightRange, pointOffset);
		// Initialize regions dictionary
		regions = new Dictionary<MapRegionType, MapRegion>();
		if (allRegions == null)
			allRegions = new List<MapRegion>();
		foreach (var region in allRegions) {
			regions.Add(region.regionType, region);
		}

		GenerateLevel();
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
		for (int i = 0; i < vertices.Length; i++) {
			// For now determined just by the height (fixed intervals, not adapting to the minimum and maximum heights)
			if (vertices[i].y < 0) { // water
				colors[i] = Color.blue;
			} else if (vertices[i].y < 0.5) { // shore
				colors[i] = Color.yellow;
			} else { // grass
				colors[i] = Color.green;
			}
		}
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
	public List<MapRegionType> allowedRegions;


	public LevelRepresentation(Vector2 dimensions, Vector2 heightRange, float pointOffset) {
		this.dimensions = dimensions;
		this.heightRange = heightRange;
		this.pointOffset = pointOffset;
		// Compute other parameters
		this.pointCount = new Vector2Int(Mathf.CeilToInt(dimensions.x / pointOffset) + 1, Mathf.CeilToInt(dimensions.y / pointOffset) + 1); // multiple of pointOffset which is the closest larger number than the given dimensions
		this.startPosition = new Vector2(-(float)(pointCount.x - 1) * pointOffset / 2, -(float)(pointCount.y - 1) * pointOffset / 2); // centre is in zero, distance between adjacent points is pointOffset

		// TODO: track

		ResetLevel();
	}

	public void ResetLevel() {
		terrain = new TerrainPoint[pointCount.x, pointCount.y];
	}

}

public class TerrainPoint {
	public int vertexIndex;
	public Vector3 position;
	public Color color;
	public MapRegionType region;
}