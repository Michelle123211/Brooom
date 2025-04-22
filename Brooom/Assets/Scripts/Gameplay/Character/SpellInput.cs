using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// A class representing a common base for communication with <c>SpellController</c> to select and cast spells.
/// Derived classes implement behaviour separately for player, and opponents (including AI).
/// </summary>
public abstract class SpellInput : MonoBehaviour {

    [Tooltip("SpellController component assigned to this racer.")]
    [SerializeField] protected SpellController spellController;

    [Tooltip("What should happen if spell casting is enabled, e.g. which components should be enabled.")]
    [SerializeField] private UnityEvent onSpellCastingEnabled;
    [Tooltip("What should happen if spell casting is disabled, e.g. which components should be disabled.")]
    [SerializeField] private UnityEvent onSpellCastingDisabled;

    private bool isInitialized = false;
    private bool spellsEnabled = false;

    /// <summary>
    /// Enables all spell casting components (registered in <c>onSpellCastingEnabled</c> callback) including this one, but only if it makes sense (i.e. the racer has at least one spell quipped).
    /// </summary>
    /// <returns><c>true</c> if spell casting was enabled, <c>false</c> otherwise (the racer has no equipped spells).</returns>
    public bool TryEnableSpellCasting() {
        if (spellController.HasEquippedSpells()) {
            // Enable all components used for spell casting
            spellsEnabled = true;
            if (!Utils.IsNullEvent(onSpellCastingEnabled)) onSpellCastingEnabled.Invoke();
            this.enabled = true;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Disables all spell casting components (registered in <c>onSpellCastingDisabled</c> callback) including this one.
    /// </summary>
    public void DisableSpellCasting() {
        // Disable all components used for spell casting
        spellsEnabled = false;
        if (!Utils.IsNullEvent(onSpellCastingDisabled)) onSpellCastingDisabled.Invoke();
        this.enabled = false;
    }

    /// <summary>
    /// Derived classes should use this method instead of <c>Update()</c> to do something every frame (it is called from <c>Update()</c>).
    /// </summary>
	protected abstract void UpdateWhenGameIsRunning();

    private void Update() {
        if (!spellsEnabled && !TryEnableSpellCasting()) {
            this.enabled = false;
            return;
        }
        // Do nothing if the game is paused
        if (GamePause.PauseState != GamePauseState.Running) return;
        UpdateWhenGameIsRunning();
    }

	private void Start() {
        isInitialized = true;
	}

	private void OnEnable() {
        if (isInitialized) // Start method has finished already, equipped spells must be initialized
            TryEnableSpellCasting();
    }
}
