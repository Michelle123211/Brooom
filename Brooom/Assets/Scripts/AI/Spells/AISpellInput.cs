using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpellInput : SpellInput {

	[SerializeField] SpellTargetSelection spellTargetSelection;

	private bool spellCasted = false;

	private float time = 0;
	private float delay = 5;

	protected override void UpdateWhenGameIsRunning() {
		// TODO: Select random spell from the ones which are ready to use, select random target
		//		- must have anough mana
		//		- spell must be charged and have at least one target

		// DEBUG: Confusione, Congelatio and Flante are equipped - Confusione is selected, cast it immediately at player
		if (!spellCasted) {
			spellController.ChangeManaAmount(spellController.MaxMana);
			if (spellController.IsCurrentlySelectedSpellReady() && spellTargetSelection.GetCurrentTarget().HasTargetAssigned) {
				spellController.CastCurrentlySelectedSpell();
				spellCasted = true;
				return;
			}
		}
		// DEBUG: Confusione, Congelatio and Flante are equipped - every X seconds try to cast a spell with certain probability
		time += Time.deltaTime;
		if (time > delay) {
			time = 0;
			delay = Random.Range(5, 10);
			if (Random.value < 0.2) { // 10 % probability of casting a spell
				for (int i = 0; i < 3; i++) {
					spellController.ChangeManaAmount(spellController.MaxMana);
					if (spellController.IsSpellInSlotReady(i) && spellTargetSelection.GetCurrentTarget().HasTargetAssigned) {
						while (spellController.selectedSpell < i) {
							spellController.ChangeSelectedSpell(IterationDirection.Increasing);
						}
						while (spellController.selectedSpell > i) {
							spellController.ChangeSelectedSpell(IterationDirection.Decreasing);
						}
						spellController.CastCurrentlySelectedSpell();
					}
				}
			}
		}
	}

}
