using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpellInput : SpellInput {

	protected override void UpdateWhenGameIsRunning() {
		// TODO: Select random spell from the ones which are ready to use, select random target
		//		- must have anough mana
		//		- spell must be charged and have at least one target
	}

}
