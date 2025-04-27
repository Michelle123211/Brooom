using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component responsible for automatic mana regeneration over time.
/// </summary>
public class ManaRegeneration : MonoBehaviour {

	/// <summary>How much mana has been regenerated in the race so far in total.</summary>
	public int TotalManaGenerated { get; private set; }

	[Tooltip("Spell Controller component managing mana amount. It is used to change the amount.")]
	[SerializeField] SpellController spellController;

	[Tooltip("After how many seconds the mana is automatically regenerated.")]
	[SerializeField] float manaRegenerationInterval = 5f;
	[Tooltip("How much mana is regenerated each time.")]
	[SerializeField] int manaRegenerationAmount = 5;

	[Tooltip("Whether debug messages should be logged.")]
	[SerializeField] bool debugLogs;

	private float timeUntilManaRegeneration;

	void Update() {
		// Mana regeneration
		timeUntilManaRegeneration -= Time.deltaTime;
		if (timeUntilManaRegeneration < 0) {
			spellController.ChangeManaAmount(manaRegenerationAmount);
			TotalManaGenerated += manaRegenerationAmount;
			if (debugLogs) Debug.Log($"Current mana increased to {spellController.CurrentMana}/{spellController.MaxMana}.");
			timeUntilManaRegeneration += manaRegenerationInterval;
		}
	}

	private void OnEnable() {
		// Reset to initial values
		timeUntilManaRegeneration = manaRegenerationInterval;
		TotalManaGenerated = 0;
	}

}
