using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpellController : MonoBehaviour {

	// Mana
	public int CurrentMana { get; private set; }
	public int MaxMana { get; private set; }

	// Spells
	public SpellInRace[] spellSlots { get; private set; }
	public int selectedSpell { get; private set; }

	// Callbacks
	public Action<int> onManaAmountChanged; // parameter: new mana value
	public Action<int> onSelectedSpellChanged; // parameter: index of the currently selected spell
	public Action<int> onSpellCasted; // parameter: index of the spell

	[Tooltip("Component derived from SpellTargetSelection which is responsible for selecting a target for currently seelcted spell.")]
	[SerializeField] SpellTargetSelection spellTargetSelection;

	private bool isPlayer = false;

	// direction ... Decreasing = lower index, Increasing = higher index
	public int ChangeSelectedSpell(IterationDirection direction) {
		int previouslySelectedSpell = selectedSpell;
		// Choose the first non-empty slot in the given direction
		int increment = (int)direction;
		for (int i = selectedSpell + increment; i >= 0 && i < spellSlots.Length; i = i + increment) {
			if (spellSlots[i] != null) {
				selectedSpell = i;
				break;
			}
		}
		if (selectedSpell != previouslySelectedSpell) onSelectedSpellChanged?.Invoke(selectedSpell);
		return selectedSpell;
	}

	public Spell GetCurrentlySelectedSpell() {
		if (selectedSpell != -1) {
			if (spellSlots[selectedSpell] != null) return spellSlots[selectedSpell].Spell;
		}
		return null;
	}

	public bool IsCurrentlySelectedSpellReady() {
		if (selectedSpell == -1) return false;
		return IsSpellInSlotReady(selectedSpell);
	}

	public bool IsSpellInSlotReady(int spellSlotIndex) {
		if (spellSlotIndex < 0 || spellSlotIndex > spellSlots.Length - 1) throw new IndexOutOfRangeException();
		SpellInRace spell = spellSlots[spellSlotIndex];
		if (spell == null) return false;
		return spell.IsSpellAvailable(CurrentMana);
	}

	public void CastCurrentlySelectedSpell() {
		if (IsCurrentlySelectedSpellReady()) { // spell is selected, it is charged and there is enough mana
			SpellInRace currentSpell = spellSlots[selectedSpell];
			// Cast spell and pass correct parameters (source, target)
			SpellTarget spellTarget = spellTargetSelection.GetCurrentTarget();
			if (!spellTarget.HasTargetAssigned) {
				Debug.Log("No suitable spell target was found.");
				return;
			}
			currentSpell.CastSpell(
				new SpellCastParameters { 
					Spell = currentSpell.Spell, 
					SourceObject = gameObject, 
					Target = spellTarget, 
					castDirection = (spellTarget.GetTargetPosition() - transform.position).normalized
				});
			ChangeManaAmount(-currentSpell.Spell.ManaCost);
			// Notify anyone interested that a spell has been casted
			onSpellCasted?.Invoke(selectedSpell);
			if (isPlayer) {
				Messaging.SendMessage("SpellCasted", currentSpell.Spell.Identifier);
				PlayerState.Instance.MarkSpellAsUsed(currentSpell.Spell.Identifier);
			}
		}
	}

	public bool HasEquippedSpells() {
		if (spellSlots == null) return false;
		foreach (var spell in spellSlots) {
			if (spell != null && spell.Spell != null && !string.IsNullOrEmpty(spell.Spell.Identifier)) return true;
		}
		return false;
	}

	public void RandomizeEquippedSpells() {
		// TODO: Randomize equipped spells so it is similar to the player
		// TODO: Initialize selectedSpell
		//spellSlots[0] = new SpellInRace(SpellManager.Instance.GetSpellFromIdentifier("MateriaMuri"));
		spellSlots[1] = new SpellInRace(SpellManager.Instance.GetSpellFromIdentifier("Confusione"));
		//spellSlots[2] = new SpellInRace(SpellManager.Instance.GetSpellFromIdentifier("Defensio"));
		//spellSlots[3] = new SpellInRace(SpellManager.Instance.GetSpellFromIdentifier("Attractio"));
		selectedSpell = 0;
	}

	public void ChangeManaAmount(int delta) {
		CurrentMana = Mathf.Clamp(CurrentMana + delta, 0, MaxMana);
		onManaAmountChanged?.Invoke(CurrentMana);
	}

	public void RechargeAllSpells() {
		foreach (var spell in spellSlots) {
			if (spell != null) spell.Recharge();
		}
	}

	private void Update() {
		// Update spells charge and availability
		foreach (var spell in spellSlots) {
			if (spell != null) {
				spell.UpdateCharge(Time.deltaTime);
				spell.UpdateAvailability(CurrentMana);
			}
		}
	}

	private void Awake() {
		isPlayer = CompareTag("Player");
		// Initialize data fields
		MaxMana = PlayerState.Instance.MaxManaAmount;
		CurrentMana = 0;
		spellSlots = new SpellInRace[PlayerState.Instance.equippedSpells.Length];
		selectedSpell = -1;
		if (isPlayer) {
			// Initialize equipped spells from PlayerState
			for (int i = 0; i < spellSlots.Length; i++) {
				if (PlayerState.Instance.equippedSpells[i] == null || string.IsNullOrEmpty(PlayerState.Instance.equippedSpells[i].Identifier)) {
					spellSlots[i] = null;
				} else {
					spellSlots[i] = new SpellInRace(PlayerState.Instance.equippedSpells[i]);
					spellSlots[i].Reset();
					// Use the first non-empty slot as the selected one
					if (selectedSpell == -1) selectedSpell = i;
				}
			}
		}
	}
}

public struct SpellCastParameters {

	public Spell Spell { get; set; }

	private SpellCastPoint castPoint;
	private GameObject sourceObject;
	public GameObject SourceObject {
		get => sourceObject;
		set {
			sourceObject = value;
			castPoint = sourceObject.GetComponentInChildren<SpellCastPoint>();
		}
	}

	public SpellTarget Target { get; set; }

	public Vector3 castDirection; // direction in which the spell is casted

	public Vector3 GetCastPosition() {
		if (castPoint != null) return castPoint.GetAbsolutePosition();
		else return sourceObject.transform.position;
	}

	public Vector3 GetTargetPosition() {
		return Target.GetTargetPosition();
	}

}

public struct SpellTarget {

	private SpellTargetPoint targetPoint;
	private GameObject targetObject;
	public GameObject TargetObject {
		get => targetObject;
		set {
			targetObject = value;
			if (value != null) {
				targetPoint = targetObject.GetComponentInChildren<SpellTargetPoint>();
				HasTargetAssigned = true;
			} else {
				HasTargetAssigned = false;
			}
		}
	}

	private Vector3 targetPosition;
	public Vector3 TargetPosition {
		get => targetPosition;
		set {
			targetPosition = value;
			targetObject = null;
			HasTargetAssigned = true;
		}
	}

	public bool HasTargetAssigned { get; private set; }

	public Vector3 GetTargetPosition() {
		if (targetObject != null) {
			if (targetPoint != null) return targetPoint.GetAbsolutePosition();
			else return targetObject.transform.position;
		} else return TargetPosition;
	}

}
