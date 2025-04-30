using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component representing a selection of spells which could be assigned to a slot of equipped spells.
/// It is displayed after clicking on any equipped spells slot.
/// </summary>
public class SpellSelectionUI : MonoBehaviour {

	[Tooltip("A Transform which is a parent of all the spell selection slots.")]
	[SerializeField] Transform spellSelectionParent;
	[Tooltip("A prefab of a spell selection slot which is instantiated multiple times.")]
	[SerializeField] SpellSelectionSlotUI spellSelectionSlotPrefab;

	private EquippedSpellSlotUI lastTargetSlot; // the last slot which invoked ShowSelectionForSlot() method
	

	/// <summary>
	/// Shows selection of spells for the given equipped spell slot.
	/// Only spells which are unlocked and are not already assigned in a different slot are displayed in the selection.
	/// Called after clicking on any slot with equipped spells.
	/// </summary>
	/// <param name="slot">Equipped spell slot for which the selection is displayed.</param>
	public void ShowSelectionForSlot(EquippedSpellSlotUI slot) {
		this.lastTargetSlot = slot;
		// Delete all existing slots from the grid
		UtilsMonoBehaviour.RemoveAllChildren(spellSelectionParent);
		// Add an empty slot (initialize with null) so that it can be chosen as well
		CreateSlot(null);
		// Fill the grid with all unlocked spells except the ones which are already assigned in a different slot
		foreach (var spell in SpellManager.Instance.AllSpells) {
			if (PlayerState.Instance.IsSpellPurchased(spell.Identifier)) {
				// Make sure the spell is not assigned in any other slot
				bool isAssigned = false;
				for (int i = 0; i < PlayerState.Instance.equippedSpells.Length; i++) {
					if (i == lastTargetSlot.slotIndex) continue; // slot for which selection is displayed
					if (PlayerState.Instance.equippedSpells[i] == null) continue; // empty slot
					if (PlayerState.Instance.equippedSpells[i].Identifier == spell.Identifier) { // is assigned in a different slot
						isAssigned = true;
						break;
					}
				}
				// Add spell slot if it can be selected
				if (!isAssigned) CreateSlot(spell);
			}
		}
		gameObject.TweenAwareEnable();
	}

	/// <summary>
	/// Assigns the given spell to the equipped spell slot for which the last spell selection was displayed,
	/// then hides the spell selection.
	/// Called from a spell selection slot.
	/// </summary>
	/// <param name="spell">Spell to be assigned to the equipped spell slot.</param>
	public void AssignSpellAndHide(Spell spell) {
		if (lastTargetSlot != null) {
			lastTargetSlot.AssignSpell(spell);
			PlayerState.Instance.EquipSpell(spell, lastTargetSlot.slotIndex);
		}
		HideSelection();
	}

	/// <summary>
	/// Hides the spell selection.
	/// Called after clicking outside of the grid in spell selection.
	/// </summary>
	public void HideSelection() {
		lastTargetSlot = null;
		gameObject.TweenAwareDisable();
	}

	// Adds a slot for the given spell into the selection grid
	private void CreateSlot(Spell spell) {
		SpellSelectionSlotUI spellSlot = Instantiate<SpellSelectionSlotUI>(spellSelectionSlotPrefab, spellSelectionParent);
		spellSlot.Initialize(spell, this);
	}
}
