using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipSpellsTutorial : TutorialStageBase {

	private enum Step {
		NotStarted,
		Started,
		Slots,
		Selection,
		MoveOn,
		Finished
	}
	private Step currentStep = Step.NotStarted;

	protected override string LocalizationKeyPrefix => "EquipSpells";

	public override void Finish() {
		// TODO: Go to Tutorial scene
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
