using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Spell {
    public Sprite icon;
    public bool isUnlocked = false;
    public int manaCost = 20;
    public float cooldown = 10; // how long it takes to recharge the spell

    // TODO: Effect of the spell, ...
}

// Spell assigned to a slot and used in a race
public class EquippedSpell {
    private Spell spell;
    private float charge = 1; // percentage but between 0 and 1

    public EquippedSpell(Spell spell, float charge = 1) {
        this.spell = spell;
        this.charge = charge;
    }

    public void CastSpell() {
        if (charge >= 1 && PlayerState.Instance.raceState.currentMana >= spell.manaCost) {
            PlayerState.Instance.raceState.ChangeManaAmount(-spell.manaCost);
            // TODO: Invoke effect of the spell
            // TODO: Tween the charge value so that the decrease is animated
            charge = 0;
        }
    }

    public void Recharge() {
        // TODO: Tween the value to animate the increase
        this.charge = 1;
    }

    public void UpdateCharge(float timeDelta) {
        this.charge = Mathf.Clamp(this.charge + (timeDelta / spell.cooldown), 0, 1);
    }

    public void Reset() {
        charge = 1; // it is fully charged at the beginning
    }
}
