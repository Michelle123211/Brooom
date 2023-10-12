using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpellInput : MonoBehaviour {

	private SpellController spellController;

	private void Update() {
		// Do nothing if the game is paused
		if (GamePause.pauseState != GamePauseState.Running) return;

		// Detect mouse click and cast the currently selected spell
		if (Input.GetMouseButtonDown(0)) {
			spellController.CastCurrentlySelectedSpell();
		}
		// Detect mouse wheel and change the currently selected spell
		// TODO: Try it with other HW if it works just like that everywhere (or it is necessary to e.g. accumulate value over frames, change spell if over some threshold, use mouse sensitivity)
		float scroll = Input.mouseScrollDelta.y;
		if (scroll != 0) {
			if (scroll > 0)
				spellController.ChangeSelectedSpell(IterationDirection.Decreasing); // up
			else 
				spellController.ChangeSelectedSpell(IterationDirection.Increasing); // down
		}
	}

	private void Awake() {
		spellController = GetComponent<SpellController>();
	}
}
