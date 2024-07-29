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
    [Tooltip("Spell's identifier, usually a spell name without spaces.")]
    [SerializeField]
    private string identifier = string.Empty; // usually spell name without spaces
    public string Identifier { get => identifier; private set => identifier = value; }

    [Tooltip("Human-readable spell name.")]
    [SerializeField]
    private string spellName = string.Empty;
    public string SpellName { get => spellName; private set => spellName = value; }

    [Tooltip("Spell icon used everywhere in the UI to represent the given spell, e.g. in HUD, shop etc.")]
    [SerializeField]
    private Sprite icon;
    public Sprite Icon { get => icon; private set => icon = value; }

    // Spell description and target description is obtained from localization tables

    [Tooltip("How much it costs to unlock the spell in the shop.")]
    [SerializeField]
    private int coinsCost = 250;
    public int CoinsCost { get => coinsCost; private set => coinsCost = value; }

    [Tooltip("How much it costs to cast the spell.")]
    [SerializeField]
    private int manaCost = 20;
    public int ManaCost { get => manaCost; private set => manaCost = value; }

    [Tooltip("For how long you have to wait before the spell is charged again and ready to be casted.")]
    [SerializeField]
    private float cooldown = 10f; // how long it takes to recharge the spell
    public float Cooldown { get => cooldown; private set => cooldown = value; }

    [Tooltip("Spells are divided into several different categories.")]
    [SerializeField]
    private SpellCategory category = SpellCategory.Invalid;
    public SpellCategory Category { get => category; private set => category = value; }

    [Tooltip("What is the spell casted at, e.g. opponent, general direction etc.")]
    [SerializeField]
    private SpellTargetType targetType = SpellTargetType.Invalid;
    public SpellTargetType TargetType { get => targetType; private set => targetType = value; }

    [Tooltip("If the spell is casted at objects, only objects with the given tag are considered.")]
    [SerializeField]
    private string spellTargetTag = string.Empty; // tag of the target object in case of SpellTargetType.Object
    public string SpellTargetTag { get => spellTargetTag; private set => spellTargetTag = value; }

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
    public Spell Spell { get; private set; }
    public float Charge { get; private set; } = 1; // percentage but between 0 and 1

    public bool IsAvailable { get; private set; } = false; // is fully charged and there is enough mana

    // Callbacks on changes
    public event Action onBecomesAvailable;
    public event Action onBecomesUnavailable;

    public SpellInRace(Spell spell, float charge = 1) {
        this.Spell = spell;
        this.Charge = charge;
    }

    public void CastSpell(SpellTarget spellTarget) {
        Spell.CastSpell(spellTarget);
        DOTween.To(() => Charge, x => Charge = x, 0, 0.3f);
    }

    public void Recharge() {
        DOTween.To(() => Charge, x => Charge = x, 1, 0.3f);
    }

    public void UpdateCharge(float timeDelta) {
        if (Spell == null) return;
        this.Charge = Mathf.Clamp(this.Charge + (timeDelta / Spell.Cooldown), 0, 1);
    }

    public bool IsSpellAvailable(int currentMana) {
        if (IsEmpty()) return false;
        return this.Charge >= 1 && currentMana >= Spell.ManaCost; // fully charged and enough mana
    }

    public bool IsEmpty() {
        return Spell == null || string.IsNullOrEmpty(Spell.Identifier);
    }

    public void UpdateAvailability(int currentMana) {
        bool newAvailability = IsSpellAvailable(currentMana);
        // Check if the state changed
        bool becomesAvailable = false;
        bool becomesUnavailable = false;
        if (newAvailability != IsAvailable) {
            becomesAvailable = newAvailability;
            becomesUnavailable = !newAvailability;
            IsAvailable = newAvailability;
        }
        // Invoke callbacks
        if (becomesAvailable) onBecomesAvailable?.Invoke();
        if (becomesUnavailable) onBecomesUnavailable?.Invoke();
    }

    public void Reset() {
        Charge = 1; // it is fully charged at the beginning
    }
}
