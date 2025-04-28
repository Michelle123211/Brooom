using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


/// <summary>
/// Terrain in a level is separated into several blocks and this components represents one of them placed in the scene.
/// It holds terrain data of a single block, i.e. a grid of terrain points, and it is also responsible for creating its mesh.
/// </summary>
public class TerrainBlock : MonoBehaviour {

	[Header("Debug")]
	[Tooltip("Whether to use gizmos to highlight terrain mesh vertices.")]
	[SerializeField] bool showVertices = false;
	[Tooltip("Whether to use gizmos to highlight terrain mesh edges.")]
	[SerializeField] bool showEdges = false;
	[Tooltip("Whether to use gizmos to highlight terrain points which are considered to be on border between adjacent regions.")]
	[SerializeField] bool showBorders = false;

	// Mesh and its data
	private Mesh mesh;
	private Vector3[] vertices;
	private int[] triangles;
	private Color[] colors;

	private MeshCollider meshCollider;

	// Terrain data
	TerrainPoint[,] terrainBlock;
	int blockX, blockZ;


	/// <summary>
	/// Generates a mesh of a single terrain block based on its object representation.
	/// The whole <c>TerrainRepresentation</c> is passed in (and not only one block), because adjacent blocks are needed to fill gaps.
	/// </summary>
	/// <param name="terrain">Object representation of the terrain in the level.</param>
	/// <param name="blockX">Block index in the X axis.</param>
	/// <param name="blockZ">Block index in the Z axis.</param>
	/// <param name="terrainRegionsDict">A dictionary of terrain regions with their data.</param>
	public void GenerateTerrainMesh(TerrainRepresentation terrain, int blockX, int blockZ, Dictionary<LevelRegionType, LevelRegion> terrainRegionsDict) {
		this.terrainBlock = terrain.GetTerrainBlock(blockX, blockZ);
		this.blockX = blockX;
		this.blockZ = blockZ;
		CreateMeshData(terrain, terrainRegionsDict);
		ConvertMeshFromSmoothToFlat();
		UpdateMesh();
	}

	/// <summary>
	/// Initializes everything necessary before being able to generate a terrain mesh.
	/// </summary>
	public void Initialize() {
		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		meshCollider = GetComponent<MeshCollider>();
	}

#if UNITY_EDITOR
	// Saves the current mesh as an asset
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

	// Prepares all data for the terrain mesh (vertices, triangles, colors) based on the terrain representation
	private void CreateMeshData(TerrainRepresentation terrain, Dictionary<LevelRegionType, LevelRegion> terrainRegionsDict) {
		int startX = blockX * terrain.blockSizePoints;
		int startY = blockZ * terrain.blockSizePoints;
		// If there is another block next to it, add one more point in that direction (to fill gaps between blocks)
		int pointCountX = this.terrainBlock.GetLength(0) + (blockX < terrain.blockCount.x - 1 ? 1 : 0);
		int pointCountY = this.terrainBlock.GetLength(1) + (blockZ < terrain.blockCount.y - 1 ? 1 : 0);
		// Vertices
		vertices = new Vector3[pointCountX * pointCountY];
		for (int x = 0, vertexIndex = 0; x < pointCountX; x++) {
			for (int y = 0; y < pointCountY; y++) {
				vertices[vertexIndex] = terrain[startX + x, startY + y].position;
				vertexIndex++;
			}
		}
		// Triangles
		triangles = new int[(pointCountX - 1) * (pointCountY - 1) * 6];
		for (int x = 0, triangleIndex = 0, vertexIndex = 0; x < pointCountX - 1; x++) {
			for (int y = 0; y < pointCountY - 1; y++) {
				// For each possible lower left corner add a quad composed of two triangles
				triangles[triangleIndex] = vertexIndex;
				triangles[triangleIndex + 1] = vertexIndex + 1;
				triangles[triangleIndex + 2] = vertexIndex + pointCountY;
				triangles[triangleIndex + 3] = vertexIndex + pointCountY;
				triangles[triangleIndex + 4] = vertexIndex + 1;
				triangles[triangleIndex + 5] = vertexIndex + pointCountY + 1;
				triangleIndex += 6;
				vertexIndex++;
			}
			vertexIndex++;
		}
		// Colors
		colors = new Color[pointCountX * pointCountY];
		for (int x = 0, vertexIndex = 0; x < pointCountX; x++) {
			for (int y = 0; y < pointCountY; y++) {
				// Assign the color of the region
				if (terrainRegionsDict.TryGetValue(terrain[startX + x, startY + y].region, out LevelRegion region)) {
					colors[vertexIndex] = region.color;
				} else {
					Debug.Log($"Unknown region: {(int)terrain[startX + x, startY + y].region}.");
					colors[vertexIndex] = Color.black;
				}
				vertexIndex++;
			}
		}
	}

	// Duplicates mesh vertices so they are not shared between triangles and the resulting mesh is flat shaded (not smoothed out)
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

	// Updates the mesh assigned to the MeshCollider component based on the current data
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

	// Highlights mesh vertices, mesh edges or terrain points belonging to borders between regions, if requested
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
			if (this.terrainBlock != null) {
				for (int x = 0; x < this.terrainBlock.GetLength(0); x++) {
					for (int y = 0; y < this.terrainBlock.GetLength(1); y++) {
						if (this.terrainBlock[x, y].isOnBorder) {
							Gizmos.DrawSphere(this.terrainBlock[x, y].position, 0.1f);
						}
					}
				}
			}
		}
	}

}
