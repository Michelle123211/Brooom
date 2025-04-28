using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component representing a border around the track and reacting to collisions with player by highlighting the impact point.
/// </summary>
public class TrackBorder : MonoBehaviour {

	[Tooltip("MeshRenderer component rendering the border with a shader capable of highlighting an impact point on collision with the player.")]
	[SerializeField] MeshRenderer meshRenderer;

	[Tooltip("Another impact point is highlighted only after at least this amount of time after the previous highlighted impact.")]
	[SerializeField] float impactCooldown = 5f;

	[Tooltip("How long (in seconds) is highlight of the impact point.")]
	[SerializeField] float impactHighlightDuration = 1f;

	private bool showImpact = true;

	private bool isHighlighting = false;
	private float t = 0;


	// Detects collisions with the player and highlights the impact point if enough time has passed from the last time
	private void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.CompareTag("Player") && showImpact) {
			showImpact = false;
			isHighlighting = true;
			meshRenderer?.sharedMaterial.SetVector("_ImpactPoint", collision.contacts[0].point);
			meshRenderer?.sharedMaterial.SetFloat("_ImpactTime", 0);
			Invoke(nameof(EnableShowingImpact), impactCooldown);
		}
	}

	// Enables impact point highlighting
	private void EnableShowingImpact() {
		showImpact = true;
	}

	private void Update() {
		// Update impact point highlighting
		if (isHighlighting) {
			t += (Time.deltaTime / impactHighlightDuration);
			if (t > 1) {
				isHighlighting = false;
				meshRenderer?.sharedMaterial.SetFloat("_ImpactTime", 1);
				t = 0;
			} else {
				meshRenderer?.sharedMaterial.SetFloat("_ImpactTime", t);
			}
		}
	}

	private void OnDestroy() {
		// Reset material properties
		meshRenderer?.sharedMaterial.SetVector("_ImpactPoint", Vector3.zero);
		meshRenderer?.sharedMaterial.SetFloat("_ImpactTime", 0);
	}
}
