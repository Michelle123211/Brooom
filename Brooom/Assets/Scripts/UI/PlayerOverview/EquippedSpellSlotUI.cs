using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquippedSpellSlotUI : MonoBehaviour {

	[HideInInspector] public Spell assignedSpell;
	[HideInInspector] public int slotIndex;
	private SpellSelectionUI spellSelection;

	private SpellSlotUI spellSlotUI;

	public void Initialize(Spell spell, int slotIndex, SpellSelectionUI spellSelection) {
		this.slotIndex = slotIndex;
		this.spellSelection = spellSelection;
		AssignSpell(spell);
	}

	public void AssignSpell(Spell spell) {
		this.assignedSpell = spell;
		spellSlotUI?.Initialize(spell);
	}

	public void ShowSpellSelection() {
		spellSelection.ShowSelectionForSlot(this);
	}

	private void Awake() {
		spellSlotUI = GetComponentInChildren<SpellSlotUI>();
	}
}
