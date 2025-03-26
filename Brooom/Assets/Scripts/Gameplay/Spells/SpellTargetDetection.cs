using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellTargetDetection : MonoBehaviour {

    [Tooltip("The maximum distance in which potential targets are detected. Trigger zone's radius is overriden with this value.")]
    [SerializeField] float maxDetectionDistance = 15;

	[Tooltip("Root object of the racer this component belongs to.")]
	[SerializeField] GameObject racerRootObject;

	[Tooltip("SpellController component assigned to this racer.")]
    [SerializeField] SpellController spellController;

    [Tooltip("Trigger zone for detecting potential targets.")]
    [SerializeField] SphereCollider triggerZone;

	private List<GameObject>[] potentialTargets;

	private bool isSpellEquipped = true;


	// Returns list of potential targets for the currently selected spell
	public List<GameObject> GetPotentialTargetsForSelectedSpell() {
		RemoveAnyInactivePotentialTarget();
		return potentialTargets[spellController.selectedSpell];
	}

	// Returns list of potential targets for the spell on the given index
	public List<GameObject> GetPotentialTargetsForGivenSpell(int spellIndex) {
		RemoveAnyInactivePotentialTarget();
		return potentialTargets[spellIndex];
	}

	// Returns lists of potential targets for all equipped spells
	public List<GameObject>[] GetPotentialTargetsForAllSpells() {
		RemoveAnyInactivePotentialTarget();
		return potentialTargets;
	}

	// Returns true if the given object could be a target of the given spell
	public bool IsPotentialTargetForGivenSpell(GameObject target, int spellIndex) {
		if (spellController.spellSlots[spellIndex] == null) return false;
		Spell equippedSpell = spellController.spellSlots[spellIndex].Spell;
		if (equippedSpell == null) return false;
		// Spell cast at opponents and the target object is a racer different from the one this component is assigned to
		if (equippedSpell.TargetType == SpellTargetType.Opponent &&
				target.layer == LayerMask.NameToLayer("Characters") &&
				target.GetInstanceID() != racerRootObject.GetInstanceID()) {
			return true;
		}
		// Spell cast at objects and the target object is a suitable target
		if (equippedSpell.TargetType == SpellTargetType.Object &&
				target.CompareTag(equippedSpell.SpellTargetTag)) {
			return true;
		}
		return false;
	}

	private void OnTriggerEnter(Collider other) {
		if (!isSpellEquipped) return;
		if (other.gameObject.GetComponent<SpellTargetPoint>() == null) return; // ignore objects which cannot be spell targets
		GameObject rootObject = other.gameObject.GetComponent<ColliderRootObject>().GetRootObject();
		// Find out whether it could be a target for any equipped spell
		for (int i = 0; i < PlayerState.Instance.equippedSpells.Length; i++) {
			if (IsPotentialTargetForGivenSpell(rootObject, i)) {
				AddPotentialTargetIfNotAlreadyThere(rootObject, i);
			}
		}
	}

	private void OnTriggerExit(Collider other) {
		if (!isSpellEquipped) return;
		if (other.gameObject.GetComponent<SpellTargetPoint>() == null) return; // ignore objects which cannot be spell targets
		GameObject rootObject = other.gameObject.GetComponent<ColliderRootObject>().GetRootObject();
		// Find out whether it could be a target for any equipped spell
		for (int i = 0; i < PlayerState.Instance.equippedSpells.Length; i++) {
			if (IsPotentialTargetForGivenSpell(rootObject, i)) {
				RemovePotentialTargetIfThere(rootObject, i);
			}
		}
	}

	// Adds a new potential target to the given spell unless it is already there
	private void AddPotentialTargetIfNotAlreadyThere(GameObject target, int spellIndex) {
		bool isThere = false;
		foreach (var otherTarget in potentialTargets[spellIndex]) {
			if (otherTarget.GetInstanceID() == target.GetInstanceID()) {
				isThere = true;
				break;
			}
		}
		if (!isThere) potentialTargets[spellIndex].Add(target);
	}

	// Removes the given potential target from the given spell if it is there
	private void RemovePotentialTargetIfThere(GameObject target, int spellIndex) {
		bool isThere = false;
		foreach (var otherTarget in potentialTargets[spellIndex]) {
			if (otherTarget.GetInstanceID() == target.GetInstanceID()) {
				isThere = true;
				break;
			}
		}
		if (isThere) potentialTargets[spellIndex].Remove(target);
	}

	// Removes any potential target which is either inactive or has been destroyed
	private void RemoveAnyInactivePotentialTarget() {
		for (int i = 0; i < potentialTargets.Length; i++) {
			if (potentialTargets[i] == null) continue;
			for (int j = potentialTargets[i].Count - 1; j >= 0; j--) {
				if (potentialTargets[i][j] == null) potentialTargets[i].RemoveAt(j);
				else if (!potentialTargets[i][j].activeSelf) RemovePotentialTargetIfThere(potentialTargets[i][j], i);
			}
		}
	}

	private void Update() {
		if (!isSpellEquipped) return;
		// Remove any inactive objects - e.g. bonuses (OnTriggerExit is not invoked when they are picked up and become inactive)
		RemoveAnyInactivePotentialTarget();
	}

	private void Start() {
		isSpellEquipped = spellController.HasEquippedSpells();
		// Initialize lists for potential targets
		potentialTargets = new List<GameObject>[PlayerState.Instance.equippedSpells.Length];
		for (int i = 0; i < PlayerState.Instance.equippedSpells.Length; i++) {
			if (spellController.spellSlots[i] == null) continue;
			Spell equippedSpell = spellController.spellSlots[i].Spell;
			if (equippedSpell == null) continue;
			if (equippedSpell.TargetType == SpellTargetType.Opponent || equippedSpell.TargetType == SpellTargetType.Object) {
				potentialTargets[i] = new List<GameObject>();
			}
		}
	}

	private void Awake() {
        triggerZone.radius = maxDetectionDistance;
	}

}
