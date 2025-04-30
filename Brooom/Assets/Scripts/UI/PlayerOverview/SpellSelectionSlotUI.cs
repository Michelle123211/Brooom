using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component representing a single spell in a spell selection for equipping a spell into a slot.
/// It uses <c>SpellSlotUI</c> for the basic spell slot functionality,
/// but adds behaviour for assigning the selected spell into a slot of equipped spells.
/// </summary>
public class SpellSelectionSlotUI : MonoBehaviour {

    private SpellSlotUI spellSlotUI;

    private Spell assignedSpell;
    private SpellSelectionUI spellSelection;


    /// <summary>
    /// Initializes a spell selection slot with the given spell (i.e. sets the icon and the tooltip content), 
    /// also stores reference to the parent <c>SpellSelectionUI</c> for later use.
    /// </summary>
    /// <param name="spell">Spell to be assigned to the slot.</param>
    /// <param name="parent">Parent <c>SpellSelectionUI</c> containing all spell slots.</param>
    public void Initialize(Spell spell, SpellSelectionUI parent) {
        this.assignedSpell = spell;
        this.spellSelection = parent;
        // Update spell icon
        spellSlotUI = GetComponentInChildren<SpellSlotUI>();
        spellSlotUI.Initialize(spell);
    }

    /// <summary>
    /// Assigns the associated spell into a slot of equipped spells and hides the spell selection.
    /// Called after clicking on the spell slot.
    /// </summary>
    public void AssignSpellToSlot() {
        // After clicking on any spell, assign it to a slot (including empty slot, assignedSpell = null)
        spellSelection.AssignSpellAndHide(assignedSpell);
    }

}
