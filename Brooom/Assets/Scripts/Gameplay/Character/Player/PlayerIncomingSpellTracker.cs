using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class managing a list of spells which are being cast at the player and displaying their indicators on the screen.
/// Each indicator is placed on a circle, where the radius is determined by the spell's distance to the player,
/// and the angle is determined by the direction from which the spell is approaching the player.
/// </summary>
public class PlayerIncomingSpellTracker : IncomingSpellsTracker {

	[Tooltip("Parent object of all UI indicators of incoming spells.")]
	[SerializeField] Transform incomingSpellIndicatorParent;
	[Tooltip("Prefab of UI indicator of incoming spell.")]
	[SerializeField] IncomingSpellIndicator incomingSpellIndicatorPrefab;

	[Tooltip("For how many seconds the camera shakes when player is hit by a spell.")]
	[SerializeField] float cameraShakeDuration = 0.3f;
	[Tooltip("How much the camera shakes when player is hit by a spell.")]
	[SerializeField] float cameraShakeIntensity = 3f;

	// All currently instantiated UI indicators of incoming spells
	private List<IncomingSpellIndicator> incomingSpellIndicators = new();
	// RectTransforms of all currently instantiated UI indicator of incoming spells (used for positioning indicators on the screen)
	private List<RectTransform> incomingSpellIndicatorTransforms = new();

	private float maxCircleRadius = 400f; // radius of a circle on which indicators appear when the spell is at the maximum distance from the player
	private float minCircleRadius = 80f; // radius of a circle on which indicators appear when the spell is at the minimum distance from the player

	/// <summary>
	/// <inheritdoc/>
	/// It shows UI indicator of this newly added spell on the screen.
	/// </summary>
	/// <param name="spellInfo">Incoming spell to be added.</param>
	protected override void OnIncomingSpellAdded(IncomingSpellInfo spellInfo) {
		// Show an indicator
		spellInfo.SpellObject.onSpellHit += ShakeCamera;
		IncomingSpellIndicator indicator = Instantiate<IncomingSpellIndicator>(incomingSpellIndicatorPrefab, incomingSpellIndicatorParent);
		indicator.Initialize(spellInfo);
		incomingSpellIndicators.Add(indicator);
		incomingSpellIndicatorTransforms.Add(indicator.GetComponent<RectTransform>());
	}
	/// <summary>
	/// <inheritdoc/>
	/// It destroys UI indicator of this removed spell.
	/// </summary>
	/// <param name="spellInfo">Incoming spell to be removed.</param>
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

	/// <summary>
	/// <inheritdoc/>
	/// It places UI indicators of all incoming spells on a circle, where angle indicates spell's direction and radius indicates spell's distance.
	/// </summary>
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
