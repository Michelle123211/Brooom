using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Represents the terrain internally divided into blocks
public class TerrainRepresentation {

	private TerrainPoint[,][,] terrainInBlocks;// 2D array of blocks, each block is 2D array of TerrainPoints

	public float pointOffset = 0.5f; // Distance between two adjacent points in the grid.
	public Vector2Int pointCount; // Number of points on the grid in the X and Z axes
	public Vector2 terrainStartPosition; // Position of the bottom left point of Mesh

	public int blockSizePoints; // Level is divided into blocks whose width is this many points
	public Vector2Int blockCount; // Number of blocks in each axis

	public TerrainRepresentation(Vector2 dimensions, float pointOffset, int blockSizeInPoints) {
		this.pointOffset = pointOffset;
		this.blockSizePoints = blockSizeInPoints;
		ComputeDependentParameters(dimensions);
		ResetTerrain();
	}

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

	public void UpdateParameters(Vector2 dimensions, float pointOffset, int blockSizeInPoints) {
		this.pointOffset = pointOffset;
		this.blockSizePoints = blockSizeInPoints;
		ComputeDependentParameters(dimensions);
	}

	public void ChangeDimensions(Vector2 newDimensions) {
		Vector2Int oldPointCount = this.pointCount;
		TerrainPoint[,][,] oldTerrainInBlocks = terrainInBlocks;
		ComputeDependentParameters(newDimensions);
		ResetTerrain();
		// Copy the upper-left corner of the old terrain to an array of new dimensions
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

	public TerrainPoint[,] GetTerrainBlock(int blockX, int blockY) {
		return terrainInBlocks[blockX, blockY];
	}

	public IEnumerable<TerrainPoint[,]> EnumerateTerrainBlocks() {
		for (int i = 0; i < blockCount.x; i++) {
			for (int j = 0; j < blockCount.y; j++) {
				yield return terrainInBlocks[i, j];
			}
		}
	}

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

	private void ComputeDependentParameters(Vector2 dimensions) {
		// Compute parameters which are not set from outside
		this.pointCount = new Vector2Int(Mathf.CeilToInt(dimensions.x / pointOffset) + 1, Mathf.CeilToInt(dimensions.y / pointOffset) + 1); // multiple of pointOffset which is the closest larger number than the given dimensions
		this.terrainStartPosition = new Vector2(-(float)(pointCount.x - 1) * pointOffset / 2, -(float)(pointCount.y - 1) * pointOffset / 2); // centre is in zero, distance between adjacent points is pointOffset
		this.blockCount = new Vector2Int(
				Mathf.CeilToInt(pointCount.x / (float)blockSizePoints),
				Mathf.CeilToInt(pointCount.y / (float)blockSizePoints)
			);
	}

}