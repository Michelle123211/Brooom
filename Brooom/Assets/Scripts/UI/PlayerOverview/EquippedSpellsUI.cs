using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component displaying slots for equipping spells to a race.
/// Slots are initialized based on the current game state (i.e. spells which the player has currently equipped).
/// </summary>
public class EquippedSpellsUI : MonoBehaviour {

	[Tooltip("A Transform which is a parent of all the equipped spell slots.")]
	[SerializeField] Transform equippedSpellsParent;
	[Tooltip("A prefab of an equipped spell slot which is instantiated multiple times.")]
	[SerializeField] EquippedSpellSlotUI equippedSpellSlotPrefab;
	[Tooltip("An object containing a grid of spells from which we can select one to assign to the slot.")]
	[SerializeField] SpellSelectionUI spellSelection;

	private void OnEnable() {
		UpdateUI();
	}

	/// <summary>
	/// Instantiates and initializes equipped spell slots based on the spells the player has currently equipped.
	/// </summary>
	public void UpdateUI() {
		// Remove all existing equipped spell slots
		UtilsMonoBehaviour.RemoveAllChildren(equippedSpellsParent);
		// Instantiate new equipped spell slots
		for (int i = 0; i < PlayerState.Instance.equippedSpells.Length; i++) {
			EquippedSpellSlotUI slot = Instantiate<EquippedSpellSlotUI>(equippedSpellSlotPrefab, equippedSpellsParent);
			slot.Initialize(PlayerState.Instance.equippedSpells[i], i, spellSelection);
		}
	}
}
