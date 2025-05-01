using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class used when persistently storing data related to spells (i.e. available spells, equipped spells, cast spells).
/// </summary>
[System.Serializable]
public class SpellsSaveData {

    #region Purchased spells
    /// <summary>Spells availability stored in array (which is serializable by <c>JsonUtility</c> as opposed to <c>Dictionary</c>).
    /// Each string contains spell identifier and availability separated by |.</summary>
    public string[] spellsAvailabilityArray = null;

    /// <summary>
    /// Whether a spell (represented by its identifier) is available (i.e. has been purchased).
    /// Getter and setter convert <c>string[]</c> array from <c>spellsAvailabilityArray</c> to <c>Dictionary&lt;string, bool&gt;</c> and vice versa.
    /// </summary>
    public Dictionary<string, bool> SpellsAvailability {
        get {
            return GetDictionaryOfSpells(spellsAvailabilityArray);
        }
        set {
            spellsAvailabilityArray = GetArrayOfSpells(value);
        }
    }

    // Converts string[] to Dictionary<string, bool> (used for spells availability and spells usage)
    private Dictionary<string, bool> GetDictionaryOfSpells(string[] spellsArray) {
        Dictionary<string, bool> spellsDictionary = new();
        foreach (var spell in spellsArray) {
            string[] parts = spell.Split('|');
            if (parts.Length == 2) {
                if (parts[1] == "false")
                    spellsDictionary[parts[0]] = false;
                else if (parts[1] == "true")
                    spellsDictionary[parts[0]] = true;
            }
        }
        return spellsDictionary;
    }

    // Converts Dictionary<string, bool> to string[] (used for spells availability and spells usage)
    private string[] GetArrayOfSpells(Dictionary<string, bool> spellsDictionary) {
        string[] spellsArray = new string[spellsDictionary.Count];
        int i = 0;
        foreach (var spell in spellsDictionary) {
            spellsArray[i] = $"{spell.Key}|{(spell.Value ? "true" : "false")}";
            i++;
        }
        return spellsArray;
    }
    #endregion

    #region Equipped spells

    /// <summary>Spells equipped in slots stored in array of spell identifiers (or 'empty' for empty slots)</summary>
    public string[] equippedSpellsIdentifiers = null;

    /// <summary>
    /// Spells equipped in slots.
    /// Getter and setter convert <c>string[]</c> array from <c>equippedSpellsIdentifiers</c> to <c>Spell[]</c> and vice versa.
    /// </summary>
    public Spell[] EquippedSpells {
        get {
            return GetEquippedSpells(equippedSpellsIdentifiers);
        }
        set {
            equippedSpellsIdentifiers = GetEquippedSpellsIdentifiers(value);
        }
    }

    // Converts string[] to Spell[]
    private Spell[] GetEquippedSpells(string[] equippedSpellsIdentifiers) {
        Spell[] equippedSpells = new Spell[equippedSpellsIdentifiers.Length];
        int i = 0;
        foreach (var spell in equippedSpellsIdentifiers) {
            if (spell == "empty") equippedSpells[i] = null;
            else equippedSpells[i] = SpellManager.Instance.GetSpellFromIdentifier(spell);
            i++;
        }
        return equippedSpells;
    }

    // Converts Spell[] to string[]
    private string[] GetEquippedSpellsIdentifiers(Spell[] equippedSpells) {
        string[] equippedSpellsIdentifiers = new string[equippedSpells.Length];
        int i = 0;
        foreach (var spell in equippedSpells) {
            if (spell == null || string.IsNullOrEmpty(spell.Identifier)) equippedSpellsIdentifiers[i] = "empty";
            else equippedSpellsIdentifiers[i] = spell.Identifier;
            i++;
        }
        return equippedSpellsIdentifiers;
    }
    #endregion

    #region Used spells

    /// <summary>Spells usage stored in array (which is serializable by <c>JsonUtility</c> as opposed to <c>Dictionary</c>).
    /// Each string contains spell identifier and whether it was used already separated by |.</summary>
    public string[] spellsUsageArray = null;

    /// <summary>
    /// Whether a spell (represented by its identifier) has been used/cast already.
    /// Getter and setter convert <c>string[]</c> array from <c>spellsUsageArray</c> to <c>Dictionary&lt;string, bool&gt;</c> and vice versa.
    /// </summary>
    public Dictionary<string, bool> SpellsUsage {
        get {
            return GetDictionaryOfSpells(spellsUsageArray);
        }
        set {
            spellsUsageArray = GetArrayOfSpells(value);
        }
    }
    #endregion
}
