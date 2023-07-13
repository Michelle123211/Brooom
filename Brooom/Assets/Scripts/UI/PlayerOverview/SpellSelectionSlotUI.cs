using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellSelectionSlotUI : MonoBehaviour
{
    private SpellSlotUI spellSlotUI;

    private Spell assignedSpell;
    private SpellSelectionUI spellSelection;


    public void Initialize(Spell spell, SpellSelectionUI parent) {
        this.assignedSpell = spell;
        this.spellSelection = parent;
        // Update spell icon
        spellSlotUI?.Initialize(spell);
    }

    public void AssignSpellToSlot() {
        // After clicking on any spell, assign it to a slot (including empty slot, assignedSpell = null)
        spellSelection.AssignSpellAndHide(assignedSpell);
    }

	private void Awake() {
        spellSlotUI = GetComponentInChildren<SpellSlotUI>();
	}
}
