using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpellInput : SpellInput {

	[SerializeField] SpellTargetSelection spellTargetSelection;

	private bool[] isSpellReady;
	private int readySpellCount = 0;
	private bool isInitialized = false;

	protected override void UpdateWhenGameIsRunning() {
		if (!isInitialized) Initialize(); // postponed initialization to make sure SpellController has been already initialized

		// Update which spells are ready to use (there is enough mana, they are charged and a suitable target exists)
		readySpellCount = 0;
		for (int i = 0; i < spellController.spellSlots.Length; i++) {
			// Check if the spell is ready to use (is charged and there is enough mana)
			if (spellController.IsSpellInSlotReady(i)) {
				// Check if a suitable target exists
				SwitchToSpell(i);
				if (spellTargetSelection.GetCurrentTarget().HasTargetAssigned) {
					isSpellReady[i] = true;
					readySpellCount++;
					continue;
				}
			}
			isSpellReady[i] = false;
		}
		// Cast a random spell from the ones which are ready
		if (readySpellCount > 0) {
			int randomIndex = Random.Range(0, 60) % readySpellCount; // 60 is a common multiple of 1, 2, 3, 4, 5 and 6 (and there will probably never be more slots for equipped spells)
			int chosenSpellIndex = 0;
			while (randomIndex >= 0) {
				if (isSpellReady[chosenSpellIndex]) {
					randomIndex--;
					if (randomIndex >= 0) chosenSpellIndex++;
				}  else chosenSpellIndex++;
			}
			SwitchToSpell(chosenSpellIndex);
			spellController.CastCurrentlySelectedSpell();
		}
	}

	private void SwitchToSpell(int spellSlotIndex) {
		while (spellController.selectedSpell < spellSlotIndex) spellController.ChangeSelectedSpell(IterationDirection.Increasing);
		while (spellController.selectedSpell > spellSlotIndex) spellController.ChangeSelectedSpell(IterationDirection.Decreasing);
	}

	private void Initialize() {
		isSpellReady = new bool[spellController.spellSlots.Length];
		isInitialized = true;
	}

}
