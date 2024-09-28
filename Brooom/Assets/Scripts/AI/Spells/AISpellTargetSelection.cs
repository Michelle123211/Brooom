using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpellTargetSelection : SpellTargetSelection {
	protected override Vector3 GetCurrentTargetDirection() {
		// TODO
		throw new System.NotImplementedException();
	}

	protected override GameObject GetCurrentTargetObject() {
		// TODO

		// DEBUG: Just select player object for now
		return UtilsMonoBehaviour.FindObjectOfTypeAndTag<SpellController>("Player").gameObject;
	}
}
