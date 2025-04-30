using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A singleton which keeps track of all spells in the game.
/// It locates all spells in the <c>Resources/Spells/</c> directory.
/// </summary>
public class SpellManager : MonoBehaviourSingleton<SpellManager>, ISingleton {

	/// <summary>A list of all spells in the game (located in <c>Resources/Spells/</c> directory), sorted according to their category and then according to their cost.</summary>
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

	/// <summary>
	/// Gets a <c>Spell</c> representation (loaded from <c>Resources/Spells/</c> directory) with all information about the spell with the given spell identifier.
	/// </summary>
	/// <param name="identifier">Identifier of the spell to be found.</param>
	/// <returns><c>Spell</c> representation of the spell, or <c>null</c> if there is no spell with the given identifier.</returns>
	public Spell GetSpellFromIdentifier(string identifier) {
		if (spellsDictionary == null) LoadAllSpells();
		if (!spellsDictionary.TryGetValue(identifier, out Spell spell))
			return null;
		else
			return spell;
	}

	/// <summary>
	/// Checks whether a spell with the given identifier exists in the game (i.e. corresponding <c>Spell</c> representation is located in <c>Resources/Spells/</c> directory).
	/// </summary>
	/// <param name="identifier">Identifier of the spell to be found.</param>
	/// <returns><c>true</c> if there is a spell with the given identifier, <c>false</c> otherwise.</returns>
	public bool CheckIfSpellExists(string identifier) {
		return spellsDictionary.TryGetValue(identifier, out Spell _);
	}

	/// <summary>
	/// Locates all spells in <c>Resources/Spells/</c> directory and loads them into internal data structures, sorted based on their category and then their cost.
	/// </summary>
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
		// Singleton options override
		Options = SingletonOptions.CreateNewGameObject | SingletonOptions.PersistentBetweenScenes | SingletonOptions.RemoveRedundantInstances;
	}

	/// <inheritdoc/>
	public void AwakeSingleton() {
	}

	/// <inheritdoc/>
	public void InitializeSingleton() {
	}
	#endregion
}
