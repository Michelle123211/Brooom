using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Blurs points belonging to borders (.isOnBorder == true) using Gaussian filter
public class RegionInterpolationSimple : LevelGeneratorModule {
	// Gaussian kernel 5x5
	float[,] kernel1 = new float[,] {
		{ 1f/273,  4f/273, 7f/273,   4f/273, 1f/273 },
		{ 4f/273, 16f/273, 26f/273, 16f/273, 4f/273 },
		{ 7f/273, 26f/273, 41f/273, 26f/273, 7f/273 },
		{ 4f/273, 16f/273, 26f/273, 16f/273, 4f/273 },
		{ 1f/273,  4f/273,  7f/273,  4f/273, 1f/273 }
	};
	//float[,] kernel2 = new float[,] {
	//	{ 0.05f, 0.04f, 0.05f, 0.04f, 0.05f },
	//	{ 0.04f, 0.04f, 0.02f, 0.04f, 0.04f },
	//	{ 0.05f, 0.02f, 0.04f, 0.04f, 0.04f },
	//	{ 0.04f, 0.04f, 0.02f, 0.04f, 0.04f },
	//	{ 0.05f, 0.04f, 0.05f, 0.04f, 0.05f }
	//};

	public override void Generate(LevelRepresentation level) {
		float[,] kernel = kernel1;
		float[,] newHeights = new float[level.pointCount.x, level.pointCount.y];
		// Compute new heights which are weighted averages of neighbouring points
		for (int x = 0; x < level.pointCount.x; x++) {
			for (int y = 0; y < level.pointCount.y; y++) {
				// Blur only the points on border
				if (level.terrain[x, y].isOnBorder) {
					// Apply kernel
					for (int i = -2; i < 3; i++) {
						int otherX = x + i;
						if (otherX < 0 || otherX >= level.pointCount.x) continue; // out of bounds check
						for (int j = -2; j < 3; j++) {
							int otherY = y + j;
							if (otherY < 0 || otherY >= level.pointCount.y) continue; // out of bounds check
							newHeights[x, y] += level.terrain[otherX, otherY].position.y * kernel[i + 2, j + 2];
						}
					}
				}
			}
		}
		// Use the currently computed heights
		for (int x = 0; x < level.pointCount.x; x++) {
			for (int y = 0; y < level.pointCount.y; y++) {
				if (newHeights[x, y] != 0)
					level.terrain[x, y].position.y = newHeights[x, y];
			}
		}
	}
}
