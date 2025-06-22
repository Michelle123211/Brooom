using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class represents a terrain in a level as a grid of points which is internally divided into blocks.
/// It holds references to all these blocks and creates a grid of points according to some parameters (e.g., dimensions, distance between points).
/// </summary>
public class TerrainRepresentation {

	/// <summary>A 2D array of terrain blocks, where each block is a 2D array of <c>TerrainPoint</c>s.</summary>
	private TerrainPoint[,][,] terrainInBlocks;

	/// <summary>Distance between two adjacent points of the grid.</summary>
	public float pointOffset = 0.5f;
	/// <summary>Number of points on the grid in the X and Z axes.</summary>
	public Vector2Int pointCount;
	/// <summary>Position of the bottom-left point of terrain mesh.</summary>
	public Vector2 terrainStartPosition;

	/// <summary>Terrain is divided into blocks, each block having this many points in each direction.</summary>
	public int blockSizePoints;
	/// <summary>Number of blocks in the X and Z axes.</summary>
	public Vector2Int blockCount;

	/// <summary>
	/// Creates a terrain representation as a grid of points divided into blocks with the given parameters.
	/// </summary>
	/// <param name="dimensions">Required terrain dimensions in the X and Z axes (final dimensions will be determined as the closest larger multiple of <c>pointOffset</c>).</param>
	/// <param name="pointOffset">Distance between two adjacent grid points.</param>
	/// <param name="blockSizeInPoints">How many points each block has in each direction.</param>
	public TerrainRepresentation(Vector2 dimensions, float pointOffset, int blockSizeInPoints) {
		UpdateParameters(dimensions, pointOffset, blockSizeInPoints);
	}

	/// <summary>
	/// Accesses a <c>TerrainPoint</c> on the given grid indices (while considering terrain divided into blocks).
	/// Can be used to alter terrain point's properties, e.g. height.
	/// </summary>
	/// <param name="i">Grid index of the point in the X axis.</param>
	/// <param name="j">Grid index of the point in the Z axis.</param>
	/// <returns></returns>
	public TerrainPoint this[int i, int j] {
		get {
			// Get point from a corresponding block
			return terrainInBlocks[Mathf.FloorToInt(i / (float)blockSizePoints), Mathf.FloorToInt(j / (float)blockSizePoints)][i % blockSizePoints, j % blockSizePoints];
		}
		set {
			// Store point to a corresponding block
			terrainInBlocks[Mathf.FloorToInt(i / (float)blockSizePoints), Mathf.FloorToInt(j / (float)blockSizePoints)][i % blockSizePoints, j % blockSizePoints] = value;
		}
	}

	/// <summary>
	/// Resets a terrain representation as a grid of points divided into blocks with the given parameters.
	/// </summary>
	/// <param name="dimensions">Required terrain dimensions in the X and Z axes (final dimensions will be determined as the closest larger multiple of <c>pointOffset</c>).</param>
	/// <param name="pointOffset">Distance between two adjacent grid points.</param>
	/// <param name="blockSizeInPoints">How many points each block has in each direction.</param>
	public void UpdateParameters(Vector2 dimensions, float pointOffset, int blockSizeInPoints) {
		this.pointOffset = pointOffset;
		this.blockSizePoints = blockSizeInPoints;
		ComputeDependentParameters(dimensions);
		ResetTerrain();
	}

	/// <summary>
	/// Changes dimensions of the terrain represented, while keeping the bottom-left corner (<c>TerrainPoint</c>s already existing in the grid with all of their properties).
	/// </summary>
	/// <param name="newDimensions">New required terrain dimensions in the X and Z axes (final dimensions will be determined as the closest larger multiple of <c>pointOffset</c>).</param>
	public void ChangeDimensions(Vector2 newDimensions) {
		Vector2Int oldPointCount = this.pointCount;
		TerrainPoint[,][,] oldTerrainInBlocks = terrainInBlocks;
		ComputeDependentParameters(newDimensions);
		ResetTerrain();
		// Copy the upper-left corner of the old terrain grid to an array of new dimensions
		for (int x = 0; x < Mathf.Min(oldPointCount.x, pointCount.x); x++) {
			for (int y = 0; y < Mathf.Min(oldPointCount.y, pointCount.y); y++) {
				TerrainPoint oldTerrainPoint = oldTerrainInBlocks[Mathf.FloorToInt(x / (float)blockSizePoints), Mathf.FloorToInt(y / (float)blockSizePoints)][x % blockSizePoints, y % blockSizePoints];
				this[x, y].position.y = oldTerrainPoint.position.y;
				this[x, y].region = oldTerrainPoint.region;
				this[x, y].isOnBorder = oldTerrainPoint.isOnBorder;
				this[x, y].color = oldTerrainPoint.color;
			}
		}
	}

	/// <summary>
	/// Gets terrain block from the given indices.
	/// </summary>
	/// <param name="blockX">Block index in the X axis.</param>
	/// <param name="blockZ">Block index in the Z axis.</param>
	/// <returns></returns>
	public TerrainPoint[,] GetTerrainBlock(int blockX, int blockZ) {
		return terrainInBlocks[blockX, blockZ];
	}

	/// <summary>
	/// Lazily enumerates all terrain blocks (i.e. 2D arrays of <c>TerrainPoint</c>s) the terrain is composed of.
	/// </summary>
	/// <returns>An enumerable of terrain blocks of the terrain.</returns>
	public IEnumerable<TerrainPoint[,]> EnumerateTerrainBlocks() {
		for (int i = 0; i < blockCount.x; i++) {
			for (int j = 0; j < blockCount.y; j++) {
				yield return terrainInBlocks[i, j];
			}
		}
	}

	/// <summary>
	/// Regenerates a grid of terrain points divided into blocks based on the currently stored parameters.
	/// All terrain points will have their initial values, i.e. it will be a completely flat terrain with height 0.
	/// </summary>
	public void ResetTerrain() {
		terrainInBlocks = new TerrainPoint[blockCount.x, blockCount.y][,];
		for (int i = 0; i < blockCount.x; i++) {
			for (int j = 0; j < blockCount.y; j++) {
				// Initialize new block
				if (i < blockCount.x - 1 && j < blockCount.y - 1)
					terrainInBlocks[i, j] = new TerrainPoint[blockSizePoints, blockSizePoints];
				else { // handle edges
					if (i == blockCount.x - 1 && j == blockCount.y - 1)
						terrainInBlocks[i, j] = new TerrainPoint[pointCount.x - i * blockSizePoints, pointCount.y - j * blockSizePoints];
					else if (i == blockCount.x - 1)
						terrainInBlocks[i, j] = new TerrainPoint[pointCount.x - i * blockSizePoints, blockSizePoints];
					else if (j == blockCount.y - 1)
						terrainInBlocks[i, j] = new TerrainPoint[blockSizePoints, pointCount.y - j * blockSizePoints];
				}
				// Fill the block with terrain points
				for (int x = 0; x < terrainInBlocks[i, j].GetLength(0); x++) {
					for (int y = 0; y < terrainInBlocks[i, j].GetLength(1); y++) {
						terrainInBlocks[i, j][x, y] = new TerrainPoint(new Vector3(terrainStartPosition.x + (i * blockSizePoints + x) * pointOffset, 0, terrainStartPosition.y + (j * blockSizePoints + y) * pointOffset));
					}
				}
			}
		}
	}

	// Computes parameters which are not set from outside but are dependent on the ones which are
	private void ComputeDependentParameters(Vector2 dimensions) {
		this.pointCount = new Vector2Int(Mathf.CeilToInt(dimensions.x / pointOffset) + 1, Mathf.CeilToInt(dimensions.y / pointOffset) + 1); // multiple of pointOffset which is the closest larger number than the given dimensions
		this.terrainStartPosition = new Vector2(-(float)(pointCount.x - 1) * pointOffset / 2, -(float)(pointCount.y - 1) * pointOffset / 2); // centre is in zero, distance between adjacent points is pointOffset
		this.blockCount = new Vector2Int(
				Mathf.CeilToInt(pointCount.x / (float)blockSizePoints),
				Mathf.CeilToInt(pointCount.y / (float)blockSizePoints)
			);
	}

}


/// <summary>
/// An object representation of a single point of terrain in a grid of points representing the whole terrain.
/// It contains information necessary for terrain generation, e.g. world position, assigned terrain region, whether it is on border between adjacent regions.
/// </summary>
public class TerrainPoint {

	/// <summary>World position this point represents.</summary>
	public Vector3 position;
	/// <summary>Terrain color assigned to this point based on a region it belongs to.</summary>
	public Color color;
	/// <summary>A terrain region this terrain point belongs to.</summary>
	public LevelRegionType region;
	/// <summary>Whether this point belongs to what is considered a border between adjacent regions.</summary>
	public bool isOnBorder;

	private Vector3 origPosition;

	public TerrainPoint(Vector3 position) {
		origPosition = position;
		Reset();
	}

	/// <summary>
	/// Resets the terrain point to initial values and initial position (set in constructor).
	/// </summary>
	public void Reset() {
		position = origPosition;
		color = Color.black;
		region = LevelRegionType.NONE;
		isOnBorder = false;
	}
}