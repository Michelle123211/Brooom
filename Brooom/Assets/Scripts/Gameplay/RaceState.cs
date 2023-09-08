using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


// A class representing state during the race
public class RaceState {
    // Level - to get access to track points and record player's position within the track
    public LevelRepresentation level;
    // Race position
    public int nextTrackPointIndex = 0; // position of the player within the track (index of the following hoop they should fly through)
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
    // Effects
    public List<PlayerEffect> effects = new List<PlayerEffect>();

    // Callbacks
    public Action<int> onPlayerPlaceChanged; // parameter: new place
    public Action<int, int> onPassedHoopsChanged; // parameters: checkpoints passed, hoops passed
    public Action<int> onMissedHoopsChanged; // parameter: missed hoops
    public Action<int> onManaAmountChanged; // parameter: new value
    public Action<PlayerEffect> onNewEffectAdded; // parameter: the added effect

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
        UpdateEffects(Time.deltaTime);
    }

    public void UpdatePlayerPlace(int place) {
        bool valueChanged = this.place != place;
        this.place = place;
        if (valueChanged)
            onPlayerPlaceChanged?.Invoke(place);
    }

    public void UpdatePlayerPositionWithinRace(int checkpointsPassed, int hoopsPassed, int hoopsMissed) {
        // Hoops passed
        bool valuesChanged = false;
        if (this.checkpointsPassed != checkpointsPassed || this.hoopsPassed != hoopsPassed) valuesChanged = true;
        this.checkpointsPassed = checkpointsPassed;
        this.hoopsPassed = hoopsPassed;
        if (valuesChanged) onPassedHoopsChanged?.Invoke(checkpointsPassed, hoopsPassed);
        // Hoops missed
        valuesChanged = false;
        if (this.hoopsMissed != hoopsMissed) valuesChanged = true;
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

    public void AddEffect(PlayerEffect effect) {
        // If there is already the same effect, increase only the duration
        foreach (var existingEffect in effects) {
            if (existingEffect == effect) {
                existingEffect.OverrideDuration(Mathf.Max(effect.TimeLeft, existingEffect.TimeLeft));
                return;
            }
        }
        // Otherwise add the new effect and call its start action
        effects.Add(effect);
        onNewEffectAdded?.Invoke(effect);
        effect.onEffectStart?.Invoke();
    }

    public void UpdateEffects(float timeDelta) {
        for (int i = effects.Count - 1; i >= 0; i--) {
            effects[i].Update(timeDelta);
            if (effects[i].IsFinished()) {
                effects[i].onEffectEnd?.Invoke();
                effects.RemoveAt(i);
            }
        }
    }

    // The RaceState is reset at the beginning of a new race
    public void Reset() {
        level = null;
        nextTrackPointIndex = 0;
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
        // Reset all effects
        for (int i = effects.Count - 1; i >= 0; i--) {
            effects[i].onEffectEnd?.Invoke(); // reverse the effects if any
            effects.RemoveAt(i);
        }
    }

    public void StartRace() {
        // Reset all spells
        for (int i = 0; i < spellSlots.Length; i++) {
            if (spellSlots[i] != null) spellSlots[i].Reset();
        }
    }
}
