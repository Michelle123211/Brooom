using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpellsSaveData {
	#region Purchased spells
	// Spells availability stored in array (which is serializable by JsonUtility as opposed to Dictionary)
	public string[] spellsAvailabilityArray = null; // each string contains spell identifier and availability separated by |

    // Property with getter/setter converting Dictionary<string, bool> to array of strings and vice versa
    public Dictionary<string, bool> SpellsAvailability {
        get {
            return GetDictionaryOfSpells(spellsAvailabilityArray);
        }
        set {
            spellsAvailabilityArray = GetArrayOfSpells(value);
        }
    }

    private Dictionary<string, bool> GetDictionaryOfSpells(string[] spellsArray) {
        Dictionary<string, bool> spellsDictionary = new Dictionary<string, bool>();
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
    // Spells in slots stored in array of spell identifiers (or 'empty' for empty slots)
    public string[] equippedSpellsIdentifiers = null;

    // Property with getter/setter converting Spell[] to array of strings and vice versa
    public Spell[] EquippedSpells {
        get {
            return GetEquippedSpells(equippedSpellsIdentifiers);
        }
        set {
            equippedSpellsIdentifiers = GetEquippedSpellsIdentifiers(value);
        }
    }

    private Spell[] GetEquippedSpells(string[] equippedSpellsIdentifiers) {
        Spell[] equippedSpells = new Spell[equippedSpellsIdentifiers.Length];
        int i = 0;
        foreach (var spell in equippedSpellsIdentifiers) {
            if (spell == "empty") equippedSpells[i] = null;
            // TODO: Get Spell instance given by the identifier from a Dictionary somewhere
            else equippedSpells[i] = new Spell();
            i++;
        }
        return equippedSpells;
    }

    private string[] GetEquippedSpellsIdentifiers(Spell[] equippedSpells) {
        string[] equippedSpellsIdentifiers = new string[equippedSpells.Length];
        int i = 0;
        foreach (var spell in equippedSpells) {
            if (spell == null || string.IsNullOrEmpty(spell.identifier)) equippedSpellsIdentifiers[i] = "empty";
            else equippedSpellsIdentifiers[i] = spell.identifier;
            i++;
        }
        return equippedSpellsIdentifiers;
    }
    #endregion

    #region Used spells
    // Spells usage stored in array (which is serializable by JsonUtility as opposed to Dictionary)
    public string[] spellsUsageArray = null; // each string contains spell identifier and whether it was used already separated by |

    // Property with getter/setter converting Dictionary<string, bool> to array of strings and vice versa
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
