using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquippedSpellSlotUI : MonoBehaviour {

	[HideInInspector] public Spell assignedSpell;
	private SpellSelectionUI spellSelection;

	private SpellSlotUI spellSlotUI;

	public void Initialize(Spell spell, SpellSelectionUI spellSelection) {
		this.spellSelection = spellSelection;
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
