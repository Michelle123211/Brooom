using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncomingSpellsTracker : MonoBehaviour {

	[Tooltip("Parent object of all indicators of incoming spells. Applicable only for player.")]
	[SerializeField] Transform incomingSpellIndicatorParent;
	[Tooltip("Prefab of indicator of incoming spell. Applicable only for player.")]
	[SerializeField] IncomingSpellIndicator incomingSpellIndicatorPrefab;

	private bool isPlayer = false;

	private List<SpellEffectController> incomingSpells = new List<SpellEffectController>();
	private List<IncomingSpellIndicator> incomingSpellIndicators = new List<IncomingSpellIndicator>();
	private List<RectTransform> incomingSpellIndicatorTransforms = new List<RectTransform>();

	private float maxCircleRadius = 400f;
	private float minCircleRadius = 80f;

	public void AddIncomingSpell(SpellEffectController spell) {
		incomingSpells.Add(spell);
		spell.onSpellCastFinished += RemoveIncomingSpell;
		// In case of player, show an indicator
		if (isPlayer) {
			IncomingSpellIndicator indicator = Instantiate<IncomingSpellIndicator>(incomingSpellIndicatorPrefab, incomingSpellIndicatorParent);
			indicator.Initialize(spell, this);
			incomingSpellIndicators.Add(indicator);
			incomingSpellIndicatorTransforms.Add(indicator.GetComponent<RectTransform>());
		}
			
	}

	public void RemoveIncomingSpell(SpellEffectController spell) {
		incomingSpells.Remove(spell);
		// In case of player, destroy the indicator
		if (isPlayer) {
			for (int i = incomingSpellIndicators.Count - 1; i >= 0; i--) {
				if (incomingSpellIndicators[i].SpellObject == spell) {
					IncomingSpellIndicator indicator = incomingSpellIndicators[i];
					incomingSpellIndicators.RemoveAt(i);
					incomingSpellIndicatorTransforms.RemoveAt(i);
					indicator.gameObject.TweenAwareDisable();
					break;
				}
			}
		}
	}

	// Returns angle between 0 and 2*pi
	private float GetAngleFromDirection(Vector3 direction) {
		float angle = Vector3.SignedAngle(direction.WithY(0), Vector3.forward, Vector3.up);
		if (angle < 0) angle += 360;
		return angle * Mathf.Deg2Rad; // convert from degrees to radians
	}

	private void Update() {
		// In case of player, place indicators on a circle
		if (isPlayer) {
			for (int i = 0; i < incomingSpellIndicators.Count; i++) {
				float angle = GetAngleFromDirection(incomingSpellIndicators[i].SpellDirection) + (Mathf.PI / 2f); // shift so zero is up
				float radius = minCircleRadius + incomingSpellIndicators[i].SpellDistanceNormalized * (maxCircleRadius - minCircleRadius);
				incomingSpellIndicatorTransforms[i].anchoredPosition = new Vector2(
					radius * Mathf.Cos(angle),
					radius * Mathf.Sin(angle)
				);
			}
		}
	}

	private void Awake() {
		isPlayer = CompareTag("Player");
	}

	private void Start() {
		if (isPlayer) {
			RectTransform rectTransform = incomingSpellIndicatorParent.GetComponent<RectTransform>();
			maxCircleRadius = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) / 2f;
		}
	}

}
