using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


// A class representing state during the race
public class RaceState {
    // Race position
    public int followingTrackPoint = 0; // position of the player within the track (they are before the hoop with this index)
    public int trackPointToPassNext = 0; // index of the following hoop the player should fly through
    public int place;
    public int hoopsPassed;
    public int hoopsMissed;
    public int checkpointsPassed;
    // Regions
    public Dictionary<LevelRegionType, bool> regionsAvailability = new Dictionary<LevelRegionType, bool>();
    // Mana
    public int currentMana;
    public int maxMana;
    // Spells
    public SpellInRace[] spellSlots;
    public int selectedSpell; // index of currently selected spell

    // Callbacks
    public Action<int> onPlayerPlaceChanged; // parameter: new place
    public Action<int, int> onPassedHoopsChanged; // parameters: checkpoints passed, hoops passed
    public Action<int> onMissedHoopsChanged; // parameter: missed hoops
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

    public void UpdatePlayerPlace(int place) {
        bool valueChanged = this.place != place;
        this.place = place;
        if (valueChanged)
            onPlayerPlaceChanged?.Invoke(place);
    }

    public void UpdatePlayerPositionWithinRace(int checkpointsPassed, int hoopsPassed, int hoopsMissed) {
        // Hoops passed
        bool valuesChanged = (this.checkpointsPassed != checkpointsPassed || this.hoopsPassed != hoopsPassed);
        this.checkpointsPassed = checkpointsPassed;
        this.hoopsPassed = hoopsPassed;
        if (valuesChanged) onPassedHoopsChanged?.Invoke(checkpointsPassed, hoopsPassed);
        // Hoops missed
        valuesChanged = this.hoopsMissed != hoopsMissed;
        this.hoopsMissed = hoopsMissed;
        if (valuesChanged) onMissedHoopsChanged?.Invoke(hoopsMissed);
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
        followingTrackPoint = 0;
        trackPointToPassNext = 0;
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
        followingTrackPoint = 0;
        trackPointToPassNext = 0;
        this.currentMana = 0;
        // Reset all spells
        for (int i = 0; i < spellSlots.Length; i++) {
            if (spellSlots[i] != null) spellSlots[i].Reset();
        }
        selectedSpell = 0;
    }
}
