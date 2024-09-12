using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellSelectionUI : MonoBehaviour {
	[Tooltip("A Transform which is a parent of all the spell selection slots.")]
	[SerializeField] Transform spellSelectionParent;
	[Tooltip("A prefab of a spell selection slot which is instantiated multiple times.")]
	[SerializeField] SpellSelectionSlotUI spellSelectionSlotPrefab;

	private EquippedSpellSlotUI lastTargetSlot; // the last slot which invoked ShowSelectionForSlot() method
	
	// Called from individual slots with equipped spells
	public void ShowSelectionForSlot(EquippedSpellSlotUI slot) {
		this.lastTargetSlot = slot;
		// Delete all existing slots from the grid
		UtilsMonoBehaviour.RemoveAllChildren(spellSelectionParent);
		// Add an empty slot (Initialize with null) so that it can be chosen as well
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

	// Called from a spell selection slot
	public void AssignSpellAndHide(Spell spell) {
		if (lastTargetSlot != null) {
			lastTargetSlot.AssignSpell(spell);
			PlayerState.Instance.EquipSpell(spell, lastTargetSlot.slotIndex);
		}
		HideSelection();
	}

	// Invoked after clicking outside of the grid
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
