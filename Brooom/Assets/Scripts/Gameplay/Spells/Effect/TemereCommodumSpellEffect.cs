using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Spell for creating a random bonus
public class TemereCommodumSpellEffect : OneShotSpellEffect {

	[SerializeField] List<BonusEffect> bonusPrefabs;

	protected override void ApplySpellEffect_Internal() {
		// Choose a random bonus to spawn (but only from those which are available)
		List<int> availableBonusIndices = new List<int>();
		for (int i = 0; i < bonusPrefabs.Count; i++) {
			if (bonusPrefabs[i].IsAvailable()) availableBonusIndices.Add(i);
		}
		int randomIndex = availableBonusIndices[Random.Range(0, availableBonusIndices.Count)];
		BonusEffect bonusPrefab = bonusPrefabs[randomIndex];
		// Create an instance, set its position and disable reactivation
		BonusEffect bonusInstance = Instantiate<BonusEffect>(bonusPrefab, castParameters.GetTargetPosition(), Quaternion.identity);
		bonusInstance.GetComponent<Bonus>().shouldReactivate = false;
	}

}
