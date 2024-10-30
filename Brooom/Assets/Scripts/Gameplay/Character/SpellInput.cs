using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


// Class representing a common base for communication with SpellController to select and cast spells
public abstract class SpellInput : MonoBehaviour {

    [Tooltip("SpellController component assigned to this racer.")]
    [SerializeField] protected SpellController spellController;

    [Tooltip("What should happen if spell casting is enabled, e.g. which components should be enabled.")]
    [SerializeField] private UnityEvent onSpellCastingEnabled;
    [Tooltip("What should happen if spell casting is disabled, e.g. which components should be disabled.")]
    [SerializeField] private UnityEvent onSpellCastingDisabled;

    private bool isInitialized = false;
    private bool spellsEnabled = false;

    // Enables all spell casting components only if it makes sense (i.e. the racer has at least one spell quipped)
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

    public void DisableSpellCasting() {
        // Disable all components used for spell casting
        spellsEnabled = false;
        if (!Utils.IsNullEvent(onSpellCastingDisabled)) onSpellCastingDisabled.Invoke();
        this.enabled = false;
    }

	protected abstract void UpdateWhenGameIsRunning();

    private void Update() {
        if (!spellsEnabled && !TryEnableSpellCasting()) {
            this.enabled = false;
            return;
        }
        // Do nothing if the game is paused
        if (GamePause.pauseState != GamePauseState.Running) return;
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
