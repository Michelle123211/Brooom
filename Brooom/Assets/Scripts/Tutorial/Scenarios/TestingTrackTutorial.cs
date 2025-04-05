using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingTrackTutorial : TutorialStageBase {

	private enum Step {
		NotStarted,
		Started,
		TestingTrack, // button to enter testing track
		Finished
	}
	private Step currentStep = Step.NotStarted;

	protected override string LocalizationKeyPrefix => "TestingTrack";

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
		// TODO: Player Overview scene with Shop open
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
