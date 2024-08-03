using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviourSingleton<SpellManager>, ISingleton {

	public List<Spell> AllSpells { get; private set; }
	Dictionary<string, Spell> spellsDictionary; // spell identifier => spell instance

	public Spell GetSpellFromIdentifier(string identifier) {
		if (!spellsDictionary.TryGetValue(identifier, out Spell spell))
			return null;
		else
			return spell;
	}

	#region Singleton initialization
	public void AwakeSingleton() {
		AllSpells = new List<Spell>();
		spellsDictionary = new Dictionary<string, Spell>();
		// Load all spells
		Spell[] spellsLoaded = Resources.LoadAll<Spell>("Spells/");
		foreach (var spell in spellsLoaded) {
			AllSpells.Add(spell);
			spellsDictionary.Add(spell.Identifier, spell);
		}
	}

	public void InitializeSingleton() {
	}

	// Persistent singleton
	protected override void SetSingletonOptions() {
		Options = (int)SingletonOptions.CreateNewGameObject | (int)SingletonOptions.PersistentBetweenScenes | (int)SingletonOptions.RemoveRedundantInstances;
	}
	#endregion
}
