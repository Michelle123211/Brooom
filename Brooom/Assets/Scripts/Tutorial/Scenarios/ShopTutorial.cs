using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopTutorial : TutorialStageBase {

	private enum Step {
		Started,
		Shop, // button to enter shop
		Spells,
		BroomUpgrades,
		Finished
	}
	private Step currentStep = Step.Started;

	protected override string LocalizationKeyPrefix => "Shop";

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
		// TODO: Player Overview scene + enough coins to buy first item in a shop
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
