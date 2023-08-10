using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackBorder : MonoBehaviour
{
    [SerializeField] MeshRenderer meshRenderer;

	[Tooltip("Another impact point is highlighted only after at least this amount of time after the previous highlighted impact.")]
	[SerializeField] float impactCooldown = 5f;

	[Tooltip("How long (in seconds) is highlight of the impact point.")]
	[SerializeField] float impactHighlightDuration = 1f;

	private bool showImpact = true;

	private bool isHighlighting = false;
	private float t = 0;


	private void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.CompareTag("Player") && showImpact) {
			showImpact = false;
			isHighlighting = true;
			meshRenderer?.sharedMaterial.SetVector("_ImpactPoint", collision.contacts[0].point);
			meshRenderer?.sharedMaterial.SetFloat("_ImpactTime", 0);
			Invoke(nameof(EnableShowingImpact), impactCooldown);
		}
	}

	private void EnableShowingImpact() {
		showImpact = true;
	}

	private void Update() {
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
}
