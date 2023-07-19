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
		gameObject.TweenAwareEnable();
	}

	// Called from a spell selection slot
	public void AssignSpellAndHide(Spell spell) {
		if (lastTargetSlot != null) {
			lastTargetSlot.AssignSpell(spell);
			PlayerState.Instance.equippedSpells[lastTargetSlot.slotIndex] = spell;
		}
		HideSelection();
	}

	// Invoked after clicking outside of the grid
	public void HideSelection() {
		lastTargetSlot = null;
		gameObject.TweenAwareDisable();
	}

	private void OnEnable() {
		// Delete all existing slots from the grid
		for (int i = spellSelectionParent.childCount - 1; i >= 0; i--) {
			Destroy(spellSelectionParent.GetChild(i).gameObject);
		}
		// Add an empty slot (Initialize with null) so that it can be chosen as well
		SpellSelectionSlotUI slot = Instantiate<SpellSelectionSlotUI>(spellSelectionSlotPrefab, spellSelectionParent);
		slot.Initialize(null, this);
		// TODO: Fill the grid with all unlocked spells
	}
}
