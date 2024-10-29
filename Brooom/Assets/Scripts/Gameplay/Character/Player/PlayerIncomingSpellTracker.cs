using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIncomingSpellTracker : IncomingSpellsTracker {

	[Tooltip("Parent object of all indicators of incoming spells. Applicable only for player.")]
	[SerializeField] Transform incomingSpellIndicatorParent;
	[Tooltip("Prefab of indicator of incoming spell. Applicable only for player.")]
	[SerializeField] IncomingSpellIndicator incomingSpellIndicatorPrefab;

	[Tooltip("For how many seconds the camera shakes when player is hit by a spell.")]
	[SerializeField] float cameraShakeDuration = 0.3f;
	[Tooltip("How much the camera shakes when player is hit by a spell.")]
	[SerializeField] float cameraShakeIntensity = 3f;

	private List<IncomingSpellIndicator> incomingSpellIndicators = new List<IncomingSpellIndicator>();
	private List<RectTransform> incomingSpellIndicatorTransforms = new List<RectTransform>();

	private float maxCircleRadius = 400f;
	private float minCircleRadius = 80f;

	protected override void OnIncomingSpellAdded(IncomingSpellInfo spellInfo) {
		// Show an indicator
		spellInfo.SpellObject.onSpellHit += ShakeCamera;
		IncomingSpellIndicator indicator = Instantiate<IncomingSpellIndicator>(incomingSpellIndicatorPrefab, incomingSpellIndicatorParent);
		indicator.Initialize(spellInfo);
		incomingSpellIndicators.Add(indicator);
		incomingSpellIndicatorTransforms.Add(indicator.GetComponent<RectTransform>());
	}

	protected override void OnIncomingSpellRemoved(IncomingSpellInfo spellInfo) {
		// Destroy the indicator
		for (int i = incomingSpellIndicators.Count - 1; i >= 0; i--) {
			if (incomingSpellIndicators[i].IncomingSpellInfo == spellInfo) {
				IncomingSpellIndicator indicator = incomingSpellIndicators[i];
				incomingSpellIndicators.RemoveAt(i);
				incomingSpellIndicatorTransforms.RemoveAt(i);
				indicator.gameObject.TweenAwareDisable();
				break;
			}
		}
	}

	protected override void UpdateAfterParent() {
		// Place indicators on a circle (angle according to direction, radius according to distance)
		for (int i = 0; i < incomingSpellIndicators.Count; i++) {
			float angle = IncomingSpells[i].GetAngleFromDirection() + (Mathf.PI / 2f); // shift so zero is up
			float radius = minCircleRadius + IncomingSpells[i].DistanceNormalized * (maxCircleRadius - minCircleRadius);
			incomingSpellIndicatorTransforms[i].anchoredPosition = new Vector2(
				radius * Mathf.Cos(angle),
				radius * Mathf.Sin(angle)
			);
		}
	}

	// Callback on spell hit
	private void ShakeCamera(SpellEffectController _) {
		PlayerCameraController camera = transform.GetComponentInParent<PlayerCameraController>();
		camera.Shake(cameraShakeDuration, cameraShakeIntensity);
	}

	private void Start() {
		RectTransform rectTransform = incomingSpellIndicatorParent.GetComponent<RectTransform>();
		maxCircleRadius = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) / 2f;
	}

}
