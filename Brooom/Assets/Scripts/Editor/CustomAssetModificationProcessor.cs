using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CustomAssetModificationProcessor : UnityEditor.AssetModificationProcessor {

	private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath) {
		// Check if it is ColorPalette in Resources, if so, reset ColorPalette.Instance
		InvalidateColorPaletteInstanceIfNecessary(sourcePath);
		InvalidateColorPaletteInstanceIfNecessary(destinationPath);
		return AssetMoveResult.DidNotMove;
	}

	private static void OnWillCreateAsset(string assetPath) {
		// Check if it is ColorPalette in Resources, if so, reset ColorPalette.Instance
		InvalidateColorPaletteInstanceIfNecessary(assetPath);
	}

	private static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions removeOptions) {
		// Check if it is ColorPalette in Resources, if so, reset ColorPalette.Instance
		InvalidateColorPaletteInstanceIfNecessary(assetPath);
		return AssetDeleteResult.DidNotDelete;
	}

	private static void InvalidateColorPaletteInstanceIfNecessary(string assetPath) {
		// If ColorPalette.asset in Resources/ is changed, invalidate singleton instance, so next time it is initialized again and correctly
		string[] tokens = assetPath.Split('/');
		if (tokens.Length == 3 && tokens[0] == "Assets" && tokens[1] == "Resources" && tokens[2] == "ColorPalette.asset") {
			ColorPalette.ResetInstance();
		}
	}

}
