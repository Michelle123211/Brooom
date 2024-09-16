using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpellTargetSelection : SpellTargetSelection {

	private bool shouldHighlightObjects = false;

	private GameObject lastTarget = null;

	private Spell selectedSpell;

	[SerializeField] RectTransform tempCrosshair;


	protected override GameObject GetCurrentTargetObject() {
		// The target object is always up-to-date thanks to Update method
		return lastTarget;
	}

	protected override Vector3 GetCurrentTargetDirection() {
		// Target direction os the current view direction
		return Camera.main.transform.forward;
	}

	private void ShowCrosshair() {
		// TODO
	}
	private void HideCrosshair() {
		// TODO
	}

	private void HighlightBestTargetForSpell() {
		// Take a list of potential targets for the selected spell
		List<GameObject> potentialTargets = spellTargetDetection.GetPotentialTargetsForSelectedSpell();
		if (potentialTargets == null) return; // either there are no suitable targets or the spell is not used at objects/opponents		  
		GameObject currentTarget = null;
		// Find target which is visible on the screen and is the closest one to the screen centre
		float minDistance = float.MaxValue;
		foreach (var target in potentialTargets) {
			if (!IsTargetVisibleOnScreen(target)) {
				continue; // target must be visible on screen
			}
			float distance = GetTargetDistanceFromScreenCentre(target);
			if (distance < minDistance) {
				currentTarget = target;
				minDistance = distance;
			}
		}
		// Highlight the target and save it as the last target
		if (currentTarget != lastTarget) {
			StopHighlightingTarget();
			lastTarget = currentTarget;
			if (lastTarget != null)
				StartHighlightingTarget();
		}
	}
	private void StartHighlightingTarget() {
		if (lastTarget == null) return;
		// TODO: Highlight the target in lastTarget using a pulsing outline
		SpellTargetPoint targetPoint = lastTarget.GetComponentInChildren<SpellTargetPoint>(); // DEBUG: Delete when not necessary
		Vector3 worldPosition = targetPoint.GetAbsolutePosition(); // DEBUG: Delete when not necessary
		Vector3 viewportPoint = Camera.main.WorldToViewportPoint(worldPosition); // DEBUG: Delete when not necessary
		tempCrosshair.anchoredPosition = new Vector2(1920f * viewportPoint.x, 1080f * viewportPoint.y); // DEBUG: Delete when not necessary
		tempCrosshair.gameObject.SetActive(true); // DEBUG: Delete when not necessary
		Debug.Log($"Started highlighting {lastTarget.name}.");
	}
	private void StopHighlightingTarget() {
		if (lastTarget == null) return;
		// TODO: Stop highlighting the target in lastTarget
		tempCrosshair.gameObject.SetActive(false); // DEBUG: Delete when not necessary
		Debug.Log($"Stopped highlighting {lastTarget.name}.");
		lastTarget = null;
	}

	private void OnSelectedSpellChanged(int selectedSpellIndex) {
		selectedSpell = spellController.GetCurrentlySelectedSpell();
		// Show/hide crosshair
		if (selectedSpell.TargetType == SpellTargetType.Direction) ShowCrosshair();
		else HideCrosshair();
		// Note down if highlighting target is necessary
		if (selectedSpell.TargetType == SpellTargetType.Opponent || selectedSpell.TargetType == SpellTargetType.Object) {
			shouldHighlightObjects = true;
			// Stop highlighting current target if necessary
			if (lastTarget != null && !spellTargetDetection.IsPotentialTargetForGivenSpell(lastTarget, selectedSpellIndex)) {
				StopHighlightingTarget();
			}
		} else {
			shouldHighlightObjects = false;
			// Stop highlighting current target if necessary
			if (lastTarget != null) StopHighlightingTarget();
		}
	}

	// Returns true if spell target point of the given target is within the current camera frustum
	private bool IsTargetVisibleOnScreen(GameObject target) {
		if (target == null) return false;
		// Spell target point must be within the camera frustum
		SpellTargetPoint targetPoint = target.GetComponentInChildren<SpellTargetPoint>();
		Vector3 worldPosition = targetPoint.GetAbsolutePosition();
		Vector3 viewportPoint = Camera.main.WorldToViewportPoint(worldPosition);
		return
			viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
			viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
			viewportPoint.z >= 0;
	}

	// Returns normalized distance of the given target's spell target point from the centre of the screen
	private float GetTargetDistanceFromScreenCentre(GameObject target) {
		if (target == null) return float.MaxValue;
		SpellTargetPoint targetPoint = target.GetComponentInChildren<SpellTargetPoint>();
		Vector3 viewportPoint = Camera.main.WorldToViewportPoint(targetPoint.GetAbsolutePosition());
		return new Vector2(viewportPoint.x - 0.5f, viewportPoint.y - 0.5f).magnitude;
	}

	private void Update() {
		if (!shouldHighlightObjects) return; // target highlighting is not needed
		// Recompute current target and highlight it
		HighlightBestTargetForSpell();
		StartHighlightingTarget(); // DEBUG: Delete when not necessary
	}

	private void OnDestroy() {
		// Unregister callbacks
		spellController.onSelectedSpellChanged -= OnSelectedSpellChanged;
	}

	private void Start() {
		// Disable this whole component if no spell is equipped
		if (!spellController.HasEquippedSpells()) {
			this.enabled = false;
			return;
		}
		// Register callback
		spellController.onSelectedSpellChanged += OnSelectedSpellChanged;
		// Initialize
		OnSelectedSpellChanged(spellController.selectedSpell);
	}

}
