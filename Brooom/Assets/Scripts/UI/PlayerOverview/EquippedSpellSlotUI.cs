using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// A component representing a single equipped spell.
/// It uses <c>SpellSlotUI</c> for the basic spell slot functionality,
/// but adds behaviour for equipping a different spell into the slot.
/// </summary>
public class EquippedSpellSlotUI : MonoBehaviour {

	/// <summary>Spell equipped in the slot.</summary>
	[HideInInspector] public Spell assignedSpell;
	/// <summary>Index of this slot in an array of equipped spell slots.</summary>
	[HideInInspector] public int slotIndex;

	private SpellSelectionUI spellSelection;
	private SpellSlotUI spellSlotUI;

	/// <summary>
	/// Initializes an equipped spell slot with the given spell (i.e. sets the icon and the tooltip content),
	/// also stores reference to the <c>SpellSelectionUI</c> for later use.
	/// </summary>
	/// <param name="spell">Spell to be assigned to the slot.</param>
	/// <param name="slotIndex">Index of the slot in an array of equipped spell slots.</param>
	/// <param name="spellSelection">Spell selection which should be displayed after clicking on the slot.</param>
	public void Initialize(Spell spell, int slotIndex, SpellSelectionUI spellSelection) {
		this.slotIndex = slotIndex;
		this.spellSelection = spellSelection;
		AssignSpell(spell);
	}

	/// <summary>
	/// Assigns the given spell to the slot while initializing it (i.e. sets the icon and the tooltip content).
	/// </summary>
	/// <param name="spell">Spell to be assigned to the slot.</param>
	public void AssignSpell(Spell spell) {
		this.assignedSpell = spell;
		spellSlotUI?.Initialize(spell);
	}

	/// <summary>
	/// Shows spell selection for this equipped spell slot.
	/// </summary>
	public void ShowSpellSelection() {
		spellSelection.ShowSelectionForSlot(this);
	}

	private void Awake() {
		spellSlotUI = GetComponentInChildren<SpellSlotUI>();
	}
}
