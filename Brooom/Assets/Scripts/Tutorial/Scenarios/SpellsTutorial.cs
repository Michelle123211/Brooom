using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellsTutorial : TutorialStageBase {

	private enum Step {
		Started,
		Equip_Slots,
		Equip_Selection,
		Equip_MoveOn, // move to a scene to learn how to use spells
		Cast_AvailableSpells, // spells in HUD
		Cast_Mana, // mana in HUD
		Cast_ManaBonus,
		Cast_FillMana, // fly around, colect mana bonuses and fill mana bar up
		Cast_ManaFull, // mana is full
		Cast_Targets, // possible targets
		Cast_SpellTarget, // target of the currently selected spell
		Cast_CastSpell, // find target, cast spell
		Cast_Cooldown, // there is a cooldown
		Cast_RechargeBonus,
		Cast_CastSpell2, // fly around, collect recharge bonus and cast spell again
		Cast_SwitchSpell,
		Finished
	}
	private Step currentStep = Step.Started;

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
		// TODO: Player Overview scene with Shop open + first spell purchased
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
