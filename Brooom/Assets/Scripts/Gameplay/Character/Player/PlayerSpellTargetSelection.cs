using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpellTargetSelection : SpellTargetSelection {

	private bool shouldHighlightObjects = false;

	private GameObject lastTarget = null;
	private SpellTargetPoint lastTargetPoint = null;

	private List<Renderer> outlinedRenderers = new List<Renderer>(); // all renderers of current target object
	private List<int> originalRendererLayers = new List<int>(); // original layers assigned to renderer objects before changing it to Outline

	private Spell selectedSpell;

	[Tooltip("A UI element representing a crosshair which is to appear on screen whenever a spell with direction as its target type is selected.")]
	[SerializeField] GameObject staticCrosshair;
	[Tooltip("A UI element representing a crosshair which is used to highlight a target object whenever a spell with opponent/object as its target is selected.")]
	[SerializeField] RectTransform dynamicCrosshair;


	protected override GameObject GetCurrentTargetObject() {
		// The target object is always up-to-date thanks to Update method
		return lastTarget;
	}

	protected override Vector3 GetCurrentTargetDirection() {
		// Target direction is the current view direction
		return Camera.main.transform.forward;
	}

	private void ShowCrosshair() {
		staticCrosshair.TweenAwareEnable();
	}
	private void HideCrosshair() {
		staticCrosshair.TweenAwareDisable();
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
		if (lastTarget == null) StopHighlightingTarget();
		if (currentTarget != lastTarget) {
			StopHighlightingTarget();
			lastTarget = currentTarget;
			if (lastTarget != null) {
				lastTargetPoint = lastTarget.GetComponentInChildren<SpellTargetPoint>();
				StartHighlightingTarget();
			}
		}
	}
	// Highlights the target in lastTarget using an outline
	private void StartHighlightingTarget() {
		if (lastTarget == null) return;
		// Find all renderers corresponding to the target object
		Renderer[] renderers = lastTarget.GetComponentsInChildren<MeshRenderer>();
		foreach (var renderer in renderers) outlinedRenderers.Add(renderer);
		renderers = lastTarget.GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (var renderer in renderers) outlinedRenderers.Add(renderer);
		// Assign them to Outline layer (only renderers and not whole target object so its layer is intact for other gameplay mechanics)
		SetRendererLayerToOutline();
		UpdateDynamicCrosshair(true);
	}
	// Stops highlighting the target in lastTarget
	private void StopHighlightingTarget() {
		// Return renderer layers to their original values
		if (lastTarget != null) RestoreRendererLayers();
		// Clear everything
		lastTarget = null;
		lastTargetPoint = null;
		outlinedRenderers.Clear();
		originalRendererLayers.Clear();
		dynamicCrosshair.gameObject.SetActive(false);
	}
	// Updates position of dynamic crosshair highlighting current target
	private void UpdateDynamicCrosshair(bool alsoEnable = false) {
		if (!shouldHighlightObjects || lastTarget == null || lastTargetPoint == null) return;
		Vector3 worldPosition = lastTargetPoint.GetAbsolutePosition();
		Vector3 viewportPoint = Camera.main.WorldToViewportPoint(worldPosition);
		dynamicCrosshair.anchoredPosition = new Vector2(1920f * viewportPoint.x, 1080f * viewportPoint.y);
		if (alsoEnable) dynamicCrosshair.gameObject.SetActive(true);
	}
	private void SetRendererLayerToOutline() {
		foreach (var renderer in outlinedRenderers) {
			originalRendererLayers.Add(renderer.gameObject.layer);
			renderer.gameObject.layer = LayerMask.NameToLayer("Outline");
		}
	}
	private void RestoreRendererLayers() {
		for (int i = 0; i < outlinedRenderers.Count; i++) {
			if (outlinedRenderers[i] != null) // object hasn't been destroyed in the meantime
				outlinedRenderers[i].gameObject.layer = originalRendererLayers[i];
		}
	}

	private void OnSelectedSpellChanged(int selectedSpellIndex) {
		selectedSpell = spellController.GetCurrentlySelectedSpell();
		// Handle case when this may be called after initialization with no equipped spells
		if (selectedSpell == null) {
			shouldHighlightObjects = false;
			HideCrosshair();
			if (lastTarget != null) StopHighlightingTarget();
			return;
		}
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
	}

	private void LateUpdate() {
		if (!shouldHighlightObjects) return; // target highlighting is not needed
		UpdateDynamicCrosshair();
	}

	private void Start() {
		// Register callback
		spellController.onSelectedSpellChanged += OnSelectedSpellChanged;
		// Initialize
		OnSelectedSpellChanged(spellController.selectedSpell);
	}

	private void OnDestroy() {
		// Unregister callbacks
		spellController.onSelectedSpellChanged -= OnSelectedSpellChanged;
	}

}
