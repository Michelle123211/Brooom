using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Applies Gaussian kernel to blur heightmap of the level
// Depending on parameters may apply the kernel only on region borders
public class MapInterpolationGauss : LevelGeneratorModule {

    [Tooltip("If true interpolates only heights along the region borders.")]
    public bool interpolateBordersOnly = true;
    [Tooltip("How many times the interpolation is repeated.")]
    public int numberOfIterations = 1;
    [Tooltip("Size of Gaussian kernel used to interpolate/blur terrain heights.")]
    public GaussianKernelSize kernelSize = GaussianKernelSize.Kernel5x5;

    #region Gaussian kernels
    // Gaussian kernel 3x3
    readonly float[,] kernel3 = new float[,] {
        { 1f/16, 2f/16, 1f/16 },
        { 2f/16, 4f/16, 2f/16 },
        { 1f/16, 2f/16, 1f/16 }
    };

	// Gaussian kernel 5x5
	readonly float[,] kernel5 = new float[,] {
        { 1f/273,  4f/273, 7f/273,   4f/273, 1f/273 },
        { 4f/273, 16f/273, 26f/273, 16f/273, 4f/273 },
        { 7f/273, 26f/273, 41f/273, 26f/273, 7f/273 },
        { 4f/273, 16f/273, 26f/273, 16f/273, 4f/273 },
        { 1f/273,  4f/273,  7f/273,  4f/273, 1f/273 }
    };

    // Gaussian kernel 7x7
    readonly float[,] kernel7 = new float[,] {
        { 0f/1003,  0f/1003,  1f/1003,  2f/1003,   1f/1003,  0f/1003, 0f/1003 },
        { 0f/1003,  3f/1003, 13f/1003, 22f/1003,  13f/1003,  3f/1003, 0f/1003 },
        { 1f/1003, 13f/1003, 59f/1003, 97f/1003,  59f/1003, 13f/1003, 1f/1003 },
        { 2f/1003, 22f/1003, 97f/1003, 159f/1003, 97f/1003, 22f/1003, 2f/1003 },
        { 1f/1003, 13f/1003, 59f/1003, 97f/1003,  59f/1003, 13f/1003, 1f/1003 },
        { 0f/1003,  3f/1003, 13f/1003, 22f/1003,  13f/1003,  3f/1003, 0f/1003 },
        { 0f/1003,  0f/1003,  1f/1003,  2f/1003,   1f/1003,  0f/1003, 0f/1003 }
    };
    #endregion

    public override void Generate(LevelRepresentation level) {
        // Select kernel of the given size
        float[,] kernel = SelectKernel();

        // Prepare an array for the interpolated heights
        float[,] newHeights = new float[level.pointCount.x, level.pointCount.y];

        // Apply the kernel the given number of times
        for (int i = 0; i < numberOfIterations; i++) {
            ApplyKernel(level, newHeights, kernel);
            UpdateHeights(level, newHeights);
        }
    }

    private float[,] SelectKernel() {
        switch (kernelSize) {
            case GaussianKernelSize.Kernel3x3:
                return kernel3;
            case GaussianKernelSize.Kernel5x5:
                return kernel5;
            case GaussianKernelSize.Kernel7x7:
                return kernel7;
            default:
                return kernel5;
        }
    }

    private void ApplyKernel(LevelRepresentation level, float[,] newHeights, float[,] kernel) {
        // Compute new heights which are weighted averages of neighbouring points
        for (int x = 0; x < level.pointCount.x; x++) {
            for (int y = 0; y < level.pointCount.y; y++) {
                // If desired blur only the points on border
                if (interpolateBordersOnly && !level.Terrain[x, y].isOnBorder) continue;
                // Apply kernel
                int kernelHalf = kernel.GetLength(0) / 2;
                for (int i = -kernelHalf; i <= kernelHalf; i++) {
                    int otherX = x + i;
                    if (otherX < 0 || otherX >= level.pointCount.x) continue; // out of bounds check
                    for (int j = -kernelHalf; j <= kernelHalf; j++) {
                        int otherY = y + j;
                        if (otherY < 0 || otherY >= level.pointCount.y) continue; // out of bounds check
                        newHeights[x, y] += level.Terrain[otherX, otherY].position.y * kernel[i + kernelHalf, j + kernelHalf];
                    }
                }
            }
        }
    }

    private void UpdateHeights(LevelRepresentation level, float[,] newHeights) {
        // Use the interpolated heights in the level
        for (int x = 0; x < level.pointCount.x; x++) {
            for (int y = 0; y < level.pointCount.y; y++) {
                if (newHeights[x, y] != 0) {
                    level.Terrain[x, y].position.y = newHeights[x, y];
                    newHeights[x, y] = 0; // reset for the next iteration
                }
            }
        }
    }

}

public enum GaussianKernelSize { 
    Kernel3x3,
    Kernel5x5,
    Kernel7x7
}