using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Class representing a common base for communication with SpellController to select and cast spells
public abstract class SpellInput : MonoBehaviour {

    [Tooltip("SpellController component assigned to this racer.")]
    [SerializeField] protected SpellController spellController;

    private bool isInitialized = false;

	protected abstract void UpdateWhenGameIsRunning();

    private void DisableSpellCastingIfNoSpellIsEquipped() {
        if (!spellController.HasEquippedSpells()) {
            gameObject.SetActive(false);
        }
    }

    private void Update() {
        // Do nothing if the game is paused
        if (GamePause.pauseState != GamePauseState.Running) return;
        UpdateWhenGameIsRunning();
    }

	private void Start() {
        isInitialized = true;
        DisableSpellCastingIfNoSpellIsEquipped();
	}

	private void OnEnable() {
        if (isInitialized) // Start method has finished already, equipped spells must be initialized
            DisableSpellCastingIfNoSpellIsEquipped();

    }
}
