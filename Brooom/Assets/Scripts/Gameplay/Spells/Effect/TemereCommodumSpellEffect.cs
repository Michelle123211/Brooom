using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A spell creating a random bonus (from those which are available) in the direction it was cast in.
/// </summary>
public class TemereCommodumSpellEffect : OneShotSpellEffect {

	[Tooltip("Prefabs of all different bonus types which can be instantiated as an effect of this spell.")]
	[SerializeField] List<BonusEffect> bonusPrefabs;

	/// <summary>
	/// Creates a random bonus (from those which are available) in the spell's target position.
	/// </summary>
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
