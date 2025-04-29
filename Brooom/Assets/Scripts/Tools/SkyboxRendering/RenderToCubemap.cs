using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


/// <summary>
/// A component which can render the current scene into a cubemap which may be then used as a skybox.
/// </summary>
public class RenderToCubemap : MonoBehaviour {

	[Tooltip("A camera which is used to render the cubemap.")]
	[SerializeField] Camera cameraToRenderFrom;
	[Tooltip("Name of the asset in which the rendered cubemap will be stored.")]
	[SerializeField] string cubemapAssetName = "Skybox.cubemap";

#if UNITY_EDITOR
	/// <summary>
	/// Renders the current scene into a cubemap and saves it as an asset.
	/// </summary>
	[ContextMenu("Render into Cubemap")]
	private void RenderIntoCubemap() {
		Cubemap cubemap = new Cubemap(1024, TextureFormat.RGB24, false);
		cameraToRenderFrom.RenderToCubemap(cubemap);
		AssetDatabase.CreateAsset(cubemap, $"Assets/Sprites/{cubemapAssetName}");
	}
#endif

}
