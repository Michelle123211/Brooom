using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpellController : MonoBehaviour {

	// Mana
	[HideInInspector] public int currentMana;
	[HideInInspector] public int maxMana;

	// Spells
	[HideInInspector] public SpellInRace[] spellSlots;
	[HideInInspector] public int selectedSpell;

	// Callbacks
	public Action<int> onManaAmountChanged; // parameter: new mana value
	public Action<int> onSelectedSpellChanged; // parameter: index of the currently selected spell

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

	public void CastCurrentlySelectedSpell() {
		if (selectedSpell != -1) {
			SpellInRace currentSpell = spellSlots[selectedSpell];
			if (currentSpell.charge >= 1 && currentMana >= currentSpell.spell.manaCost) {
				currentSpell.CastSpell();
				ChangeManaAmount(-currentSpell.spell.manaCost);
				// Notify anyone interested that a spell has been casted
				if (isPlayer) Messaging.SendMessage("SpellCasted");
			}
		}
	}

	public bool HasEquippedSpells() {
		foreach (var spell in spellSlots) {
			if (spell != null) return true;
		}
		return false;
	}

	public void RandomizeEquippedSpells() { 
		// TODO: Randomize equipped spells so it is similar to the player
		// TODO: Initialize selectedSpell
	}

	public void ChangeManaAmount(int delta) {
		currentMana = Mathf.Clamp(currentMana + delta, 0, maxMana);
		onManaAmountChanged?.Invoke(currentMana);
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
				spell.UpdateAvailability(currentMana);
			}
		}
	}

	private void Awake() {
		isPlayer = CompareTag("Player");
		// Initialize data fields
		maxMana = PlayerState.Instance.maxManaAmount;
		currentMana = 0;
		spellSlots = new SpellInRace[PlayerState.Instance.equippedSpells.Length];
		selectedSpell = -1;
		if (isPlayer) {
			// Initialize equipped spells from PlayerState
			for (int i = 0; i < spellSlots.Length; i++) {
				if (PlayerState.Instance.equippedSpells[i] == null || string.IsNullOrEmpty(PlayerState.Instance.equippedSpells[i].identifier)) {
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
