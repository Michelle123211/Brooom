using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpellInput : SpellInput {

	[SerializeField] SpellTargetSelection spellTargetSelection;
	[SerializeField] AISkillLevel skillLevel;

	[SerializeField] bool debugLogs = false;

	private int equippedSpellCount = 0;

	private bool[] isSpellReady;
	private int readySpellCount = 0;

	private bool isInitialized = false;

	private float nextDecisionDelay = 0f; // in how many seconds a new spell casting decision would be made

	protected override void UpdateWhenGameIsRunning() {
		if (!isInitialized) Initialize(); // postponed initialization to make sure SpellController has been already initialized

		// Make sure it is already time for another iteration
		if (nextDecisionDelay > 0f) {
			nextDecisionDelay -= Time.deltaTime;
			return;
		}

		// Compute parameters dependent on Magic statistic
		float magicMistakeProbability = 0f;
		if (skillLevel != null) magicMistakeProbability = skillLevel.GetMagicMistakeProbability();
		float spellCastProbability = GetSpellCastProbability(magicMistakeProbability); // spell is casted with a certain probability
		int allowedSpellCount = GetNumberOfEquippedSpellsToUse(magicMistakeProbability); // only first few equipped spells may be used

		if (debugLogs)
			Debug.Log($"Making a spell casting decision. Magic mistake probability is {magicMistakeProbability}, probability of casting a spell is {spellCastProbability} and number of allowed spells is {allowedSpellCount}.");

		// Try to cast a spell
		UpdateSpellsReadiness(allowedSpellCount);
		SelectAndCastSpell(allowedSpellCount, spellCastProbability);

		// Update delay until next decision
		nextDecisionDelay = GetNextDecisionDelay(magicMistakeProbability);
		if (debugLogs) Debug.Log($"Next decision in approximately {Mathf.RoundToInt(nextDecisionDelay)} s.");
	}

	// Updates readySpellCount and isSpellReady according to which spells are ready (there is enough mana, they are charged and a suitable target exists)
	private void UpdateSpellsReadiness(int allowedSpellCount) {
		readySpellCount = 0;
		for (int i = 0; i < spellController.spellSlots.Length; i++) {
			// Check if the spell is ready to use (is charged and there is enough mana)
			if (spellController.IsSpellInSlotReady(i)) {
				// Check if a suitable target exists
				SwitchToSpell(i);
				if (spellTargetSelection.GetCurrentTarget().HasTargetAssigned) {
					isSpellReady[i] = true;
					if (i < allowedSpellCount) readySpellCount++; // racer may be allowed to use only X first spells, so only these are tracked (this assumes spells are equipped without gaps)
					continue;
				}
			}
			isSpellReady[i] = false;
		}
	}

	// Casts a random spell from the ones which are ready while considering only first maxSpellToUseCount equipped spells
	private void SelectAndCastSpell(int allowedSpellCount, float spellCastProbability) {
		if (allowedSpellCount > 0 && readySpellCount > 0 && Random.value < spellCastProbability) { // spell is casted only with a certain probability
			// Select a random spell from the ones which are ready and allowed
			int randomIndex = Random.Range(0, readySpellCount);
			// Find this spell and cast it
			for (int i = 0; i < isSpellReady.Length; i++) {
				if (isSpellReady[i] && randomIndex == 0) { // this is the spell to be casted
					SwitchToSpell(i);
					spellController.CastCurrentlySelectedSpell();
					return;
				}
				if (isSpellReady[i]) randomIndex--;
			}
		} else {
			if (debugLogs) Debug.Log($"Spell is not casted because conditions were not met.");
		}
	}

	private void SwitchToSpell(int spellSlotIndex) {
		while (spellController.selectedSpell < spellSlotIndex) spellController.ChangeSelectedSpell(IterationDirection.Increasing);
		while (spellController.selectedSpell > spellSlotIndex) spellController.ChangeSelectedSpell(IterationDirection.Decreasing);
	}

	// Returns time duration until another spell cast decision is made - computed based on racer statistics
	private float GetNextDecisionDelay(float magicMistakeProbability) {
		return skillLevel.mistakesParameters.SpellDecisionIntervalCurve.Evaluate(magicMistakeProbability);
	}

	// Returns probability of casting a spell - computed based on racer statistics
	private float GetSpellCastProbability(float magicMistakeProbability) {
		return skillLevel.mistakesParameters.SpellCastCurve.Evaluate(magicMistakeProbability);
	}

	// Returns number of equipped spells which could be used - computed based on racer statistics
	private int GetNumberOfEquippedSpellsToUse(float magicMistakeProbability) {
		// AI-driven racers always have spells equipped in a continuous block starting at index 0
		float percentage = skillLevel.mistakesParameters.SpellUsedCountCurve.Evaluate(magicMistakeProbability); // percentage of equipped spells which could be used
		int count = Mathf.FloorToInt(percentage * (equippedSpellCount + 1));
		if (count > equippedSpellCount) count = equippedSpellCount; // could happen for percentage == 1
		return count;
	}

	private void Initialize() {
		isSpellReady = new bool[spellController.spellSlots.Length];
		equippedSpellCount = 0;
		foreach (var slot in spellController.spellSlots) {
			if (slot != null && !slot.IsEmpty()) equippedSpellCount++;
		}
		isInitialized = true;
	}

}
