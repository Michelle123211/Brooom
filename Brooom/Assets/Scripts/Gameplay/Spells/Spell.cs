using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;


public enum SpellCategory {
    Invalid,
    SelfCast,
    OpponentCurse,
    EnvironmentManipulation,
    ObjectApparition
}

public enum SpellTargetType { 
    Invalid,
    Self,
    Opponent,
    Direction,
    Object
}


public class Spell : MonoBehaviour {
    public string identifier = string.Empty; // usually spell name without spaces
    public string spellName = string.Empty;
    public Sprite icon;
    // Spell description and target description is obtained from localization tables

    public int coinsCost = 250;
    public int manaCost = 20;
    public float cooldown = 10; // how long it takes to recharge the spell

    public SpellCategory category = SpellCategory.Invalid;
    public SpellTargetType targetType = SpellTargetType.Invalid;
    public string spellTargetTag; // tag of the target object in case of SpellTargetType.Object

    [Tooltip("A prefab of SpellEffectController component which is responsible for controlling the visual and actual effect of casting the spell.")]
    [SerializeField] private SpellEffectController effectControllerPrefab;

    public void CastSpell(SpellTarget spellTarget) {
        // Create a separate instance of the spell effect controller (so it could be casted by several racers at once)
        SpellEffectController effect = Instantiate<SpellEffectController>(effectControllerPrefab);
        effect.InvokeSpellEffect(this, spellTarget);
    }
}

// Spell assigned to a slot and used in a race
public class SpellInRace {
    public Spell spell;
    public float charge = 1; // percentage but between 0 and 1

    // Callbacks on changes
    public Action onBecomesAvailable;
    public Action onBecomesUnavailable;

    public bool isAvailable = false; // is fully charged and there is enough mana

    public SpellInRace(Spell spell, float charge = 1) {
        this.spell = spell;
        this.charge = charge;
    }

    public void CastSpell(SpellTarget spellTarget) {
        spell.CastSpell(spellTarget);
        DOTween.To(() => charge, x => charge = x, 0, 0.3f);
    }

    public void Recharge() {
        DOTween.To(() => charge, x => charge = x, 1, 0.3f);
    }

    public void UpdateCharge(float timeDelta) {
        if (spell == null) return;
        this.charge = Mathf.Clamp(this.charge + (timeDelta / spell.cooldown), 0, 1);
    }

    public bool IsSpellAvailable(int currentMana) {
        if (IsEmpty()) return false;
        return this.charge >= 1 && currentMana >= spell.manaCost; // fully charged and enough mana
    }

    public bool IsEmpty() {
        return spell == null || string.IsNullOrEmpty(spell.identifier);
    }

    public void UpdateAvailability(int currentMana) {
        bool newAvailability = IsSpellAvailable(currentMana);
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

    public void Reset() {
        charge = 1; // it is fully charged at the beginning
    }
}
