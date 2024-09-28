using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpellInput : SpellInput {

	[SerializeField] SpellTargetSelection spellTargetSelection;

	private bool spellCasted = false;

	protected override void UpdateWhenGameIsRunning() {
		// TODO: Select random spell from the ones which are ready to use, select random target
		//		- must have anough mana
		//		- spell must be charged and have at least one target

		// DEBUG: Congelatio is selected, cast it immediately at player
		if (!spellCasted) {
			spellController.ChangeManaAmount(spellController.MaxMana);
			if (spellController.IsCurrentlySelectedSpellReady() && spellTargetSelection.GetCurrentTarget().HasTargetAssigned) {
				spellController.CastCurrentlySelectedSpell();
				spellCasted = true;
			}
		}
	}

}
