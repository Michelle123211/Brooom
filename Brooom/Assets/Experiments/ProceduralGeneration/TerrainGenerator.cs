using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
public class TerrainGenerator : MonoBehaviour
{
    [Tooltip("Dimensions of the terrain in the X and Z axes. Final dimensions will be determined as the closest larger multiple of pointOffset.")]
    public Vector2 dimensions = new Vector2(100, 100);
    [Tooltip("Distance between two adjacent points in the grid.")]
    public float pointOffset = 1;
    [Tooltip("Minimum Y coordinate.")]
    public float minimumHeight = -5;
    [Tooltip("Maximum Y coordinate.")]
    public float maximumHeight = 10;

    // Object-oriented representation
    TerrainPoint[,] points;

    // Parameters
    int pointsCountX, pointsCountZ; // number of points on the grid in the X and Z axes (computed from the dimensions and pointOffset and rounded up)

    // Mesh and its data
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    Color[] colors;


    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        GenerateTerrain();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("Regenerate")]
    private void GenerateTerrain() {
        ComputeParameters();
        GenerateTerrainPoints();
        CreateMeshData();
        UpdateMesh();
    }

    private void ComputeParameters() {
        // Final dimensions - as multiple of pointOffset which is the closest larger number than the given dimensions
        pointsCountX = Mathf.CeilToInt(dimensions.x / pointOffset) + 1;
        pointsCountZ = Mathf.CeilToInt(dimensions.y / pointOffset) + 1;
    }

    private void GenerateTerrainPoints() {
        // Random offset in the Perlin noise to make it different each time
        int randOffsetX = Random.Range(0, 1000);
        int randOffsetY = Random.Range(0, 1000);
        // Grid of points with heights determined by the Perlin noise
        //    - the centre is in (0,0,0)
        //    - distance between adjacent points is pointOffset
        float startX = -(float)(pointsCountX - 1) * pointOffset / 2;
        float startZ = -(float)(pointsCountZ - 1) * pointOffset / 2;
        points = new TerrainPoint[pointsCountX, pointsCountZ];
        for (int x = 0, i = 0; x < pointsCountX; x++) {
            for (int z = 0; z < pointsCountZ; z++) {
                TerrainPoint point = new TerrainPoint();
                point.vertexIndex = i;
                // Determine height using Perlin noise
                float height = Mathf.PerlinNoise((randOffsetX + x) * 0.05f, (randOffsetY + z) * 0.05f) * (maximumHeight - minimumHeight) + minimumHeight;
                point.position = new Vector3(startX + x * pointOffset, height, startZ + z * pointOffset);
                points[x, z] = point;
                i++;
            }
        }
    }

    private void CreateMeshData() {
        // Vertices
        vertices = new Vector3[pointsCountX * pointsCountZ];
        for (int x = 0; x < points.GetLength(0); x++) {
            for (int z = 0; z < points.GetLength(1); z++) {
                TerrainPoint point = points[x, z];
                vertices[point.vertexIndex] = point.position;
            }
        }
        // Triangles
        triangles = new int[(pointsCountX - 1) * (pointsCountZ - 1) * 6];
        for (int x = 0, i = 0; x < pointsCountX - 1; x++) {
            for (int z = 0; z < pointsCountZ - 1; z++) {
                // For each possible lower left corner add a quad composed of two triangles
                triangles[i] = points[x, z].vertexIndex;
                triangles[i + 1] = points[x, z + 1].vertexIndex;
                triangles[i + 2] = points[x + 1, z].vertexIndex; ;
                triangles[i + 3] = points[x + 1, z].vertexIndex; ;
                triangles[i + 4] = points[x, z + 1].vertexIndex; ;
                triangles[i + 5] = points[x + 1, z + 1].vertexIndex; ;
                i += 6;
            }
        }
        // Colors
        colors = new Color[pointsCountX * pointsCountZ];
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


public class TerrainPoint {
    public int vertexIndex;
    public Vector3 position;
    public Color color;
    public MapRegion region;
}

public enum MapRegion { 
    // Natural
    AboveWater = 1,
    SnowyMountain = 2,
    AboveClouds = 3,
    Tunnel = 4,
    // Artificial
    EnchantedForest = 101,
    AridDesert = 102,
    BloomingMeadow = 103,
    StormyArea = 104
}