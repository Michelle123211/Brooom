using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// A components randomly instantiating cloud object on an upper hemisphere around the world origin.
/// It is used to prepare a scene from which skybox with clouds can be rendered.
/// </summary>
public class PlaceClouds : MonoBehaviour {

	[Tooltip("Clouds will be placed randomly on an upper hemisphere around (0,0,0) with this radius.")]
	[SerializeField] float radius = 150f;
	[Tooltip("This many clouds will be placed.")]
	[SerializeField] int count = 500;

	[Tooltip("Transform under which all clouds will be placed.")]
	[SerializeField] Transform cloudsParent;

	[Tooltip("Different variations of cloud models, each time one will be chosen randomly.")]
	[SerializeField] List<GameObject> cloudPrefabs;
	[Tooltip("Scale for each cloud will be chosen randomly from this interval.")]
	[SerializeField] Vector2 scaleRange = new Vector2(1, 4);

#if UNITY_EDITOR
	/// <summary>
	/// Generates clouds with random rotation and scale in random points on an upper hemisphere around the world origin.
	/// </summary>
	[ContextMenu("Regenerate clouds")]
	private void GenerateClouds() {
		// Remove any previously instantiated elements
		UtilsMonoBehaviour.RemoveAllChildren(cloudsParent);
		for (int i = 0; i < count; i++) {
			// Choose cloud prefab
			int variantIndex = Random.Range(0, cloudPrefabs.Count);
			// Choose random parameters
			Vector3 randomVector = Random.onUnitSphere;
			Vector3 position = randomVector.WithY(Mathf.Abs(randomVector.y)) * radius; // only upper hemisphere
			Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
			float scale = Random.Range(scaleRange.x, scaleRange.y);
			// Instantiate
			GameObject instance = Instantiate(cloudPrefabs[variantIndex], position, rotation, cloudsParent);
			instance.transform.localScale *= scale;
			// Set shadows off
			MeshRenderer meshRenderer = instance.GetComponentInChildren<MeshRenderer>();
			meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		}
	}
#endif


}
