using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RenderToCubemap : MonoBehaviour {

	[SerializeField] Camera cameraToRenderFrom;
	[SerializeField] string cubemapAssetName = "Skybox.cubemap";

#if UNITY_EDITOR
	[ContextMenu("Render into Cubemap")]
	private void RenderIntoCubemap() {
		Cubemap cubemap = new Cubemap(1024, TextureFormat.RGB24, false);
		cameraToRenderFrom.RenderToCubemap(cubemap);
		AssetDatabase.CreateAsset(cubemap, $"Assets/Sprites/{cubemapAssetName}");
	}
#endif

}
