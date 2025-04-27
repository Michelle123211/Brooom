using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;


/// <summary>
/// Possible spell categories (based on the spell's overall functionality).
/// </summary>
public enum SpellCategory {
    Invalid,
    SelfCast,
    OpponentCurse,
    EnvironmentManipulation,
    ObjectApparition
}

/// <summary>
/// Possible spell targets.
/// </summary>
public enum SpellTargetType { 
    Invalid,
    Self,
    Opponent,
    Direction,
    Object
}

/// <summary>
/// A component representing a spell in the game.
/// It contains all basic information about that particular spell and also provides a method for casting it.
/// </summary>
public class Spell : MonoBehaviour {

    // In the following lines, the field is declared explicitly instead of an auto-implemented property to work more easily with a custom editor

    [Tooltip("Spell's identifier, usually a spell name without spaces.")]
    [SerializeField]
    private string identifier = string.Empty;
    public string Identifier { get => identifier; private set => identifier = value; }

    [Tooltip("Human-readable spell name, may contain spaces.")]
    [SerializeField]
    private string spellName = string.Empty;
    public string SpellName { get => spellName; private set => spellName = value; }

    [Tooltip("Color assigned to the spell, will be used for different visual effects when casting the spell.")]
    [SerializeField]
    private Color baseColor;
    public Color BaseColor { get => baseColor; private set => baseColor = value; }

    [Tooltip("Color assigned to the spell, will be used for emission of different visual effects when casting the spell.")]
    [SerializeField]
    [ColorUsage(true, true)]
    private Color emissionColor;
    public Color EmissionColor { get => emissionColor; private set => emissionColor = value; }

    [Tooltip("Spell icon used everywhere in the UI to represent the given spell, e.g. in HUD, shop etc.")]
    [SerializeField]
    private Sprite icon;
    public Sprite Icon { get => icon; private set => icon = value; }

    [Tooltip("Spell icon used to indicate a spell being cast towards the player.")]
    [SerializeField]
    private Sprite indicatorIcon;
    public Sprite IndicatorIcon { get => indicatorIcon; private set => indicatorIcon = value; }

    // Spell description and target description is obtained from localization tables (based on the spell's identifier)

    [Tooltip("How much it costs to unlock the spell in the shop.")]
    [SerializeField]
    private int coinsCost = 250;
    public int CoinsCost { get => coinsCost; private set => coinsCost = value; }

    [Tooltip("How much it costs to cast the spell.")]
    [SerializeField]
    private int manaCost = 20;
    public int ManaCost { get => manaCost; private set => manaCost = value; }

    [Tooltip("For how long you have to wait before the spell is charged again and ready to be cast.")]
    [SerializeField]
    private float cooldown = 10f;
    public float Cooldown { get => cooldown; private set => cooldown = value; }

    [Tooltip("Spells are divided into several different categories based on their functionality in general.")]
    [SerializeField]
    private SpellCategory category = SpellCategory.Invalid;
    public SpellCategory Category { get => category; private set => category = value; }

    [Tooltip("What is the spell cast at, e.g. opponent, general direction etc.")]
    [SerializeField]
    private SpellTargetType targetType = SpellTargetType.Invalid;
    public SpellTargetType TargetType { get => targetType; private set => targetType = value; }

    [Tooltip("If the spell is cast at objects, only objects with the given tag are considered.")]
    [SerializeField]
    private string spellTargetTag = string.Empty; // tag of the target object in case of SpellTargetType.Object
    public string SpellTargetTag { get => spellTargetTag; private set => spellTargetTag = value; }

    [Tooltip("Whether the spell affects a potential target in a positive or negative way.")]
    [SerializeField]
    private bool isPositive = false;
    public bool IsPositive { get => isPositive; private set => isPositive = value; }

    [Tooltip("A prefab of SpellEffectController component which is responsible for controlling the visual and actual effect of casting the spell.")]
    [SerializeField] private SpellEffectController effectControllerPrefab;

    /// <summary>
    /// Starts casting the spell and invokes its effect using an instance of <c>SpellEffectController</c> and the given spell cast parameters.
    /// </summary>
    /// <param name="castParameters">Parameters for casting the spell (e.g., source, target).</param>
    public void CastSpell(SpellCastParameters castParameters) {
        // Create a separate instance of the spell effect controller (so it could be cast by several racers at once)
        SpellEffectController effect = Instantiate<SpellEffectController>(effectControllerPrefab, castParameters.GetCastPosition(), Quaternion.identity);
        effect.InvokeSpellEffect(castParameters);
    }

}


/// <summary>
/// A class representing a spell assigned to a slot and used in a race.
/// It provides useful properties and methods to check if the spell is ready to be cast, i.e. if there is enough mana and the spell is charged.
/// </summary>
public class SpellInRace {

    /// <summary>Represented spell.</summary>
    public Spell Spell { get; private set; }
    /// <summary>Percentage of the spell's charge as a number between 0 and 1. When it is 1, the spell is no longer on cooldown.</summary>
    public float Charge { get; private set; } = 1;

    /// <summary>Whether the spell is fully charged and there is enough mana to cast it.</summary>
    public bool IsAvailable { get; private set; } = false;
    /// <summary>Whether there is enough mana for casting the spell.</summary>
    public bool HasEnoughMana { get; private set; } = false;

    // Callbacks on changes
    /// <summary>Called when the spell becomes available (i.e. it is charged and there is enough mana to cast it).</summary>
    public event Action<Spell> onBecomesAvailable;
    /// <summary>Called when the spell becomes unavailable (i.e. it is not charged and/or there is not enough mana to cast it).</summary>
    public event Action<Spell> onBecomesUnavailable;

    /// <summary>
    /// Creates an instance of a spell assigned to a slot and used in a race.
    /// </summary>
    /// <param name="spell">The assigned spell.</param>
    /// <param name="charge">The initial charge of the spell between 0 (not charged at all) and 1 (fully charged).</param>
    public SpellInRace(Spell spell, float charge = 1) {
        this.Spell = spell;
        this.Charge = charge;
    }

    /// <summary>
    /// Starts casting the assigned spell and invokes its effect using an instance of <c>SpellEffectController</c> and the given spell cast parameters.
    /// Also puts the spell on cooldown.
    /// </summary>
    /// <param name="castParameters">Parameters for casting the spell (e.g., source, target).</param>
    public void CastSpell(SpellCastParameters castParameters) {
        Spell.CastSpell(castParameters);
        DOTween.To(() => Charge, x => Charge = x, 0, 0.3f);
    }

    /// <summary>
    /// Recharges the spell almost instantly.
    /// </summary>
    public void Recharge() {
        DOTween.To(() => Charge, x => Charge = x, 1, 0.3f);
    }

    /// <summary>
    /// Updates the assigned spell's charge based on the time elapsed.
    /// </summary>
    /// <param name="timeDelta">Elapsed time from the last call.</param>
    public void UpdateCharge(float timeDelta) {
        if (Spell == null) return;
        this.Charge = Mathf.Clamp(this.Charge + (timeDelta / Spell.Cooldown), 0, 1);
    }

    /// <summary>
    /// Checks whether the spell assigned to the slot is available, i.e. it is fully charged (not on cooldown) and there is enough mana for casting it.
    /// </summary>
    /// <param name="currentMana">Current amount of mana of the corresponding racer.</param>
    /// <returns><c>true</c> if the spell is ready to be used, <c>false</c> otherwise (also when there is no assigned spell).</returns>
    public bool IsSpellAvailable(int currentMana) {
        if (IsEmpty()) return false;
        return this.Charge >= 1 && currentMana >= Spell.ManaCost; // fully charged and enough mana
    }

    /// <summary>
    /// Checks whether the spell slot represented by this instance is empty, i.e. there is no spell assigned.
    /// </summary>
    /// <returns><c>true</c> if the slot is empty, <c>false</c> otherwise.</returns>
    public bool IsEmpty() {
        return Spell == null || string.IsNullOrEmpty(Spell.Identifier);
    }

    /// <summary>
    /// Checks if the spell is available based on it's current charge and current mana. If the availability changes, corresponding callbacks are invoked.
    /// </summary>
    /// <param name="currentMana">Current amount of mana of the corresponding racer.</param>
    public void UpdateAvailability(int currentMana) {
        bool newAvailability = IsSpellAvailable(currentMana);
        // Check mana
        HasEnoughMana = (currentMana >= Spell.ManaCost);
        // Check if the state changed
        bool becomesAvailable = false;
        bool becomesUnavailable = false;
        if (newAvailability != IsAvailable) {
            becomesAvailable = newAvailability;
            becomesUnavailable = !newAvailability;
            IsAvailable = newAvailability;
        }
        // Invoke callbacks
        if (becomesAvailable) onBecomesAvailable?.Invoke(Spell);
        if (becomesUnavailable) onBecomesUnavailable?.Invoke(Spell);
    }

    /// <summary>
    /// Resets the state to the initial values, making the spell fully charged.
    /// </summary>
    public void Reset() {
        Charge = 1; // it is fully charged at the beginning
    }
}
