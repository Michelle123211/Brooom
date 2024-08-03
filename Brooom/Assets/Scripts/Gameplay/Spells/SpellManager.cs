using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviourSingleton<SpellManager>, ISingleton {

	Dictionary<string, Spell> spells; // spell identifier => spell instance

	public Spell GetSpellFromIdentifier(string identifier) {
		if (!spells.TryGetValue(identifier, out Spell spell))
			return null;
		else
			return spell;
	}

	#region Singleton initialization
	public void AwakeSingleton() {
	}

	public void InitializeSingleton() {
		spells = new Dictionary<string, Spell>();
		// Load all spells
		Spell[] spellsLoaded = Resources.LoadAll<Spell>("Spells/");
		foreach (var spell in spellsLoaded) {
			spells.Add(spell.Identifier, spell);
		}
	}

	// Persistent singleton
	protected override void SetSingletonOptions() {
		Options = (int)SingletonOptions.CreateNewGameObject | (int)SingletonOptions.PersistentBetweenScenes | (int)SingletonOptions.RemoveRedundantInstances;
	}
	#endregion
}
