using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpellInput : SpellInput {

	[SerializeField] SpellTargetSelection spellTargetSelection;

	protected override void UpdateWhenGameIsRunning() {
		// TODO: Select random spell from the ones which are ready to use, select random target
		//		- must have anough mana
		//		- spell must be charged and have at least one target
		for (int i = 0; i < spellController.spellSlots.Length; i++) {
			// Check if the spell is ready to use and a suitable target exists
			if (!spellController.IsSpellInSlotReady(i)) continue;
			// Switch to that spell
			while (spellController.selectedSpell < i) spellController.ChangeSelectedSpell(IterationDirection.Increasing);
			while (spellController.selectedSpell > i) spellController.ChangeSelectedSpell(IterationDirection.Decreasing);
			// Check if there a suitable target exists
			if (!spellTargetSelection.GetCurrentTarget().HasTargetAssigned) continue;
			// Cast that spell
			spellController.CastCurrentlySelectedSpell();
			break;
		}
	}

}
