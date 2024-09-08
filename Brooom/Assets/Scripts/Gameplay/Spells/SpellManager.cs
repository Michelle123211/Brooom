using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviourSingleton<SpellManager>, ISingleton {

	public List<Spell> AllSpells {
		get {
			if (allSpells == null) {
				LoadAllSpells();
			}
			return allSpells;
		}
	}

	List<Spell> allSpells = null;
	Dictionary<string, Spell> spellsDictionary = null; // spell identifier => spell instance

	public Spell GetSpellFromIdentifier(string identifier) {
		if (spellsDictionary == null) LoadAllSpells();
		if (!spellsDictionary.TryGetValue(identifier, out Spell spell))
			return null;
		else
			return spell;
	}

	public bool CheckIfSpellExists(string identifier) {
		return spellsDictionary.TryGetValue(identifier, out Spell _);
	}

	private void LoadAllSpells() {
		allSpells = new List<Spell>();
		spellsDictionary = new Dictionary<string, Spell>();
		Spell[] spellsLoaded = Resources.LoadAll<Spell>("Spells/");
		foreach (var spell in spellsLoaded) {
			allSpells.Add(spell);
			spellsDictionary.Add(spell.Identifier, spell);
		}
	}

	#region Singleton initialization
	public void AwakeSingleton() {
	}

	public void InitializeSingleton() {
	}

	// Persistent singleton
	protected override void SetSingletonOptions() {
		Options = (int)SingletonOptions.CreateNewGameObject | (int)SingletonOptions.PersistentBetweenScenes | (int)SingletonOptions.RemoveRedundantInstances;
	}
	#endregion
}
