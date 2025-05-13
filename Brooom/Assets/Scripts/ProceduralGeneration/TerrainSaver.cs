using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;


/// <summary>
/// Provides useful context menu items for saving currently generated terrain, so it can be reused e.g. in Testing Track or Tutorial scene.
/// </summary>
public class TerrainSaver : MonoBehaviour {

#if UNITY_EDITOR

	// Saves the current terrain with its environment elements as a prefab
	[ContextMenu("Save Terrain as Prefab")]
	private void SaveTerrainAsPrefab() {
		// Get path from the user
		string path = EditorUtility.SaveFilePanel("Save Level Prefab", "Assets/Prefabs/LevelGeneration/", "GeneratedLevelPrefab", "prefab");
		if (string.IsNullOrEmpty(path)) return;
		path = FileUtil.GetProjectRelativePath(path); // convert the absolute path to project-relative to use with AssetDatabase
		 // Save all terrain meshes
		SaveTerrainMeshes();
		// Create an instance and save it as a prefab
		Transform origParent = transform.parent;
		GameObject cloneParent = new GameObject();
		Instantiate<Transform>(origParent.Find("Terrain"), cloneParent.transform); // terrain blocks
		Instantiate<Transform>(origParent.Find("Environment"), cloneParent.transform); // environment elements
		Instantiate<Transform>(origParent.Find("Water"), cloneParent.transform); // water
		GameObject prefab = PrefabUtility.SaveAsPrefabAsset(cloneParent, path);
		// Remove the original template
		DestroyImmediate(cloneParent);
		// Open the newly created asset in the Project window
		ProjectWindowUtil.ShowCreatedAsset(prefab);
	}

	// Saves meshes of all currently generated terrain blocks as assets
	[ContextMenu("Save Terrain Meshes")]
	private void SaveTerrainMeshes() {
		// Get target folder
		string saveFolderPath = EditorUtility.SaveFolderPanel("Save Terrain Meshes", "Assets/Models/Terrain/", "");
		saveFolderPath = FileUtil.GetProjectRelativePath(saveFolderPath); // convert the absolute path to project-relative to use with AssetDatabase
		// Save each terrain block individually
		int blockIndex = 0;
		foreach (var terrainBlock in GetComponentsInChildren<TerrainBlock>()) {
			SaveTerrainBlock(terrainBlock, blockIndex, saveFolderPath);
			blockIndex++;
		}
	}

	// Saves mesh of the given TerrainBlock as an asset
	private void SaveTerrainBlock(TerrainBlock block, int index, string path) {
		MeshFilter meshFilter = block.GetComponent<MeshFilter>();
		if (meshFilter == null || meshFilter.mesh == null) return;
		if (string.IsNullOrEmpty(path)) return;

		path = $"{path}/TerrainBlockMesh{index:D5}.asset";
		// Check whether the mesh is already saved as an asset
		if (AssetDatabase.IsNativeAsset(meshFilter.mesh.GetInstanceID())) {
			Debug.Log($"Mesh {meshFilter.mesh.name} of a terrain block is an asset already, so {path} was not created.");
			return;
		}
		// Optimize the mesh (changes ordering of the geometry and vertices to improve vertex cache utilisation)
		MeshUtility.Optimize(meshFilter.mesh);
		// Save the asset
		AssetDatabase.CreateAsset(meshFilter.mesh, path);
		AssetDatabase.SaveAssets();
	}

#endif

}
