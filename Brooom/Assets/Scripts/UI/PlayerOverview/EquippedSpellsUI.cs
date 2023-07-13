using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquippedSpellsUI : MonoBehaviour {
	[Tooltip("A Transform which is a parent of all the equipped spell slots.")]
	[SerializeField] Transform equippedSpellsParent;
	[Tooltip("A prefab of an equipped spell slot which is instantiated multiple times.")]
	[SerializeField] EquippedSpellSlotUI equippedSpellSlotPrefab;
	[Tooltip("An object containing a list of spells from which we can select one for a slot.")]
	[SerializeField] SpellSelectionUI spellSelection;

	private void Start() {
		// Remove all existing equipped spell slots
		for (int i = equippedSpellsParent.childCount - 1; i >= 0; i--) {
			Destroy(equippedSpellsParent.GetChild(i).gameObject);
		}
		// Instantiate new equipped spell slots
		for (int i = 0; i < PlayerState.Instance.raceState.spellSlots.Length; i++) {
			EquippedSpellSlotUI slot = Instantiate<EquippedSpellSlotUI>(equippedSpellSlotPrefab, equippedSpellsParent);
			EquippedSpell spell = PlayerState.Instance.raceState.spellSlots[i];
			slot.Initialize(spell == null ? null : spell.spell, spellSelection);
		}
	}
}
