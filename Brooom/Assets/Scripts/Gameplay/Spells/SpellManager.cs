using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviourSingleton<SpellManager>, ISingleton {

	public List<Spell> AllSpells {
		get {
			if (allSpells == null) {
				LoadAllSpells();
				Analytics.Instance.LogEvent(AnalyticsCategory.Spells, $"There are {AllSpells.Count} spells in this build.");
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
		// Find all spells available
		allSpells = new List<Spell>();
		spellsDictionary = new Dictionary<string, Spell>();
		Spell[] spellsLoaded = Resources.LoadAll<Spell>("Spells/");
		foreach (var spell in spellsLoaded) {
			allSpells.Add(spell);
			spellsDictionary.Add(spell.Identifier, spell);
		}
		// Sort spells first according to category, then according to their cost
		allSpells.Sort((a, b) => {
			if (a.Category != b.Category) return a.Category.CompareTo(b.Category);
			return a.CoinsCost.CompareTo(b.CoinsCost);
		});
	}

	#region Singleton initialization
	static SpellManager() { 
		Options = SingletonOptions.CreateNewGameObject | SingletonOptions.PersistentBetweenScenes | SingletonOptions.RemoveRedundantInstances;
	}

	public void AwakeSingleton() {
	}

	public void InitializeSingleton() {
	}
	#endregion
}
