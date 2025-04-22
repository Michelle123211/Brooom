using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class managing a list of spells which are being cast at the racer.
/// </summary>
public class AIIncomingSpellTracker : IncomingSpellsTracker {

	// No additional functionality is needed on top of IncomingSpellsTracker

	protected override void OnIncomingSpellAdded(IncomingSpellInfo spellInfo) {
	}

	protected override void OnIncomingSpellRemoved(IncomingSpellInfo spellInfo) {
	}

	protected override void UpdateAfterParent() {
	}
}
