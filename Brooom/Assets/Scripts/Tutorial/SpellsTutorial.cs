using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellsTutorial : TutorialStageBase {

	private enum SpellsTutorialState { 
		EquipSpell,
		CastSpell // mana bonus, cast spell, recharge bonus
	}

	public override void Finish() {
		// TODO
	}

	public override string GetCurrentState() {
		// TODO
		return string.Empty;
	}

	public override void SetCurrentState(string state) {
		// TODO
	}

	protected override bool CheckTriggerConditions() {
		// TODO
		return true;
	}

	protected override IEnumerator InitializeTutorialStage() {
		// TODO
		yield break;
	}

	protected override bool UpdateTutorialStage() {
		// TODO
		return true;
	}
}
