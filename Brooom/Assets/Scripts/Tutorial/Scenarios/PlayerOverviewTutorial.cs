using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOverviewTutorial : TutorialStageBase {

	private enum Step {
		NotStarted,
		Started,
		Coins,
		Leaderboard,
		Stats,
		Endurance,
		Speed,
		Dexterity,
		Precision,
		Magic,
		Finished
	}
	private Step currentStep = Step.NotStarted;

	protected override string LocalizationKeyPrefix => "PlayerOverview";

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
		// TODO: Player Overview scene
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
