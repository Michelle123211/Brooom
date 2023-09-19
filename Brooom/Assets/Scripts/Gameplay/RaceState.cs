using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


// A class representing state during the race
public class RaceState {
    // Regions
    public Dictionary<LevelRegionType, bool> regionsAvailability = new Dictionary<LevelRegionType, bool>();
    // Mana
    public int currentMana;
    public int maxMana;
    // Spells
    public SpellInRace[] spellSlots;
    public int selectedSpell; // index of currently selected spell

    // Callbacks
    public Action<int> onManaAmountChanged; // parameter: new value

    public RaceState() {
        this.maxMana = PlayerState.Instance.maxManaAmount;
        this.currentMana = 0;
        this.spellSlots = new SpellInRace[PlayerState.Instance.equippedSpells.Length];
        this.selectedSpell = 0;
    }

    public void SetRegionAvailability(LevelRegionType region, bool availability) {
        if (regionsAvailability.ContainsKey(region) && availability && !regionsAvailability[region] && availability) { // a new region became available
            // Notify anyone interested that a new region has been unlocked
            Messaging.SendMessage("NewRegionAvailable");
        }
        regionsAvailability[region] = availability;
    }

    public void UpdateRaceState() {
        UpdateSpellsCharge(Time.deltaTime);
    }

    public void ChangeManaAmount(int delta) {
        currentMana = Mathf.Clamp(currentMana + delta, 0, maxMana);
        onManaAmountChanged?.Invoke(currentMana);
    }

    public void UpdateSpellsCharge(float timeDelta) {
        foreach (var spell in spellSlots) {
            if (spell != null)
                spell.UpdateCharge(timeDelta);
        }
    }

    public void RechargeAllSpells() {
        foreach (var spell in spellSlots) {
            if (spell != null)
                spell.Recharge();
        }
    }

    // The RaceState is reset at the beginning of a new level
    public void ResetAll() {
        this.currentMana = 0;
        // Initialize all spells
        for (int i = 0; i < spellSlots.Length; i++) {
            if (PlayerState.Instance.equippedSpells[i] == null || string.IsNullOrEmpty(PlayerState.Instance.equippedSpells[i].identifier)) {
                spellSlots[i] = null;
            } else {
                spellSlots[i] = new SpellInRace(PlayerState.Instance.equippedSpells[i]);
                spellSlots[i].Reset();
            }
        }
        selectedSpell = 0;
    }

    public void StartRace() {
        this.currentMana = 0;
        // Reset all spells
        for (int i = 0; i < spellSlots.Length; i++) {
            if (spellSlots[i] != null) spellSlots[i].Reset();
        }
        selectedSpell = 0;
    }
}
