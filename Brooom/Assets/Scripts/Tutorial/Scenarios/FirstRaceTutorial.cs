using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstRaceTutorial : TutorialStageBase {

	private enum Step {
		Started,
		Training, // training, reset
		StartingZone,
		FreeMovement, // can fly freely and proceed whenever
		SkipTraining, // an option to skip training from now on
		Finished
	}
	private Step currentStep = Step.Started;

	public override void Finish() {
		// TODO
		// TODO: The following is throwing exception right now because we are not checking for Race scene in trigger conditions, so this tutorial stage is invoked immediately
		Utils.FindObject<GamePause>().SetupOptionsForRace();
	}

	public override string GetCurrentState() {
		// TODO
		return string.Empty;
	}

	public override void SetCurrentState(string state) {
		// TODO
	}

	protected override bool CheckTriggerConditions() {
		// TODO: Race scene
		return true;
	}

	protected override IEnumerator InitializeTutorialStage() {
		// TODO: Set initial state and prepare everything necessary
		Utils.FindObject<GamePause>().SetupOptionsForTutorial();
		yield break;
	}

	protected override bool UpdateTutorialStage() {
		// TODO
		return true;
	}
}
