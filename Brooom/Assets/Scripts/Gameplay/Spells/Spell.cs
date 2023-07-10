using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[System.Serializable]
public class Spell {
    public Sprite icon;
    public int coinsCost = 250;
    public bool isUnlocked = false;
    public int manaCost = 20;
    public float cooldown = 10; // how long it takes to recharge the spell

    // TODO: Effect of the spell, ...
}

// Spell assigned to a slot and used in a race
public class EquippedSpell {
    public Spell spell;
    public float charge = 1; // percentage but between 0 and 1

    // Callbacks on changes
    public Action onBecomesAvailable;
    public Action onBecomesUnavailable;

    private bool isAvailable = false; // is fully charged and there is enough mana

    public EquippedSpell(Spell spell, float charge = 1) {
        this.spell = spell;
        this.charge = charge;
    }

    public void CastSpell() {
        if (charge >= 1 && PlayerState.Instance.raceState.currentMana >= spell.manaCost) {
            PlayerState.Instance.raceState.ChangeManaAmount(-spell.manaCost);
            // TODO: Invoke effect of the spell
            // TODO: Tween the charge value so that the decrease is animated
            Debug.Log("Spell cast.");
            charge = 0;
            UpdateAvailability();
        }
    }

    public void Recharge() {
        // TODO: Tween the value to animate the increase
        this.charge = 1;
        UpdateAvailability();
    }

    public void UpdateCharge(float timeDelta) {
        this.charge = Mathf.Clamp(this.charge + (timeDelta / spell.cooldown), 0, 1);
        UpdateAvailability();
    }

    public void Reset() {
        charge = 1; // it is fully charged at the beginning
        UpdateAvailability();
    }

    public bool IsSpellAvailable() {
        return this.charge >= 1 && PlayerState.Instance.raceState.currentMana >= spell.manaCost; // fully charged and enough mana
    }

    private void UpdateAvailability() {
        bool newAvailability = IsSpellAvailable();
        // Check if the state changed
        bool becomesAvailable = false;
        bool becomesUnavailable = false;
        if (newAvailability != isAvailable) {
            becomesAvailable = newAvailability;
            becomesUnavailable = !newAvailability;
            isAvailable = newAvailability;
        }
        // Invoke callbacks
        if (becomesAvailable) onBecomesAvailable?.Invoke();
        if (becomesUnavailable) onBecomesUnavailable?.Invoke();
    }
}
