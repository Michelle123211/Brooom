using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastSpellsTutorial : TutorialStageBase {

	private enum Step {
		NotStarted,
		Started,
		AvailableSpells, // spells in HUD
		Mana, // mana in HUD
		ManaBonus,
		FillMana, // fly around, colect mana bonuses and fill mana bar up
		ManaFull, // mana is full
		Targets, // possible targets
		SpellTarget, // target of the currently selected spell
		CastSpell, // find target, cast spell
		Cooldown, // there is a cooldown
		RechargeBonus,
		CastSpell2, // fly around, collect recharge bonus and cast spell again
		SwitchSpell,
		Finished
	}
	private Step currentStep = Step.NotStarted;

	protected override string LocalizationKeyPrefix => "CastSpells";

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
		// TODO: Tutorial scene
		return true;
	}

	protected override IEnumerator InitializeTutorialStage() {
		// TODO: Set initial state and prepare everything necessary
		yield break;
	}

	protected override bool UpdateTutorialStage() {
		// TODO
		return true;
	}
}
