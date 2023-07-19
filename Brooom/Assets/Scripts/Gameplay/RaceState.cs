using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


// A class representing state during the race
public class RaceState {
    // Level - to get access to track points and record player's position within the track
    public LevelRepresentation level;
    public int previousTrackPointIndex = -1; // position of the player within the track (index of the last hoop they passed)
    // Race position
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
    public EquippedSpell[] spellSlots;
    public int selectedSpell; // index of currently selected spell
    // Effects
    public List<PlayerEffect> effects = new List<PlayerEffect>();

    // Callbacks
    public Action<int> onPlayerPlaceChanged; // parameter: new place
    public Action<int, int> onPlayerPositionWithinRaceChanged; // parameters: checkpoints passed, hoops passed
    public Action<int> onManaAmountChanged; // parameter: new value
    public Action<PlayerEffect> onNewEffectAdded; // parameter: the added effect

    public RaceState(int manaAmount, EquippedSpell[] equippedSpells) {
        this.maxMana = manaAmount;
        this.currentMana = 0;
        this.spellSlots = equippedSpells;
        this.selectedSpell = 0;

        // TODO: DEBUG only, remove
        spellSlots[0] = new EquippedSpell(new Spell());
        spellSlots[1] = new EquippedSpell(new Spell());
        spellSlots[2] = new EquippedSpell(new Spell());
        spellSlots[3] = new EquippedSpell(new Spell());
    }

    public void Update() {
        UpdateSpellsCharge(Time.deltaTime);
        UpdateEffects(Time.deltaTime);
    }

    public void UpdatePlayerPlace(int place) {
        bool valueChanged = this.place != place;
        this.place = place;
        if (valueChanged)
            onPlayerPlaceChanged?.Invoke(place);
    }

    public void UpdatePlayerPositionWithinRace(int checkpointsPassed, int hoopsPassed) {
        bool valuesChanged = false;
        if (this.checkpointsPassed != checkpointsPassed || this.hoopsPassed != hoopsPassed) valuesChanged = true;
        this.checkpointsPassed = checkpointsPassed;
        this.hoopsPassed = hoopsPassed;
        if (valuesChanged)
            onPlayerPositionWithinRaceChanged?.Invoke(checkpointsPassed, hoopsPassed);

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

    public void Reset() {
        level = null;
        previousTrackPointIndex = -1;
        this.currentMana = 0;
        // Reset all spells
        foreach (var spell in spellSlots) {
            if (spell != null)
                spell.Reset();
        }
        selectedSpell = 0;
        // Reset all effects
        for (int i = effects.Count - 1; i >= 0; i--) {
            effects[i].onEffectEnd?.Invoke(); // reverse the effects if any
            effects.RemoveAt(i);
        }
    }
}
