using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroductionTutorial : TutorialStageBase {

	private enum IntroductionTutorialState { 
		BasicMovement, // forward, turn, brake
		FlyUpOrDown, // up and down
		LookingAround, // look around, reset
		Hoop, // hoop and checkpoint
		Bonus, // speed bonus, effects
		FreeMovement // can fly freely and proceed whenever
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
