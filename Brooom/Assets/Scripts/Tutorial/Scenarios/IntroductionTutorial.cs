using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroductionTutorial : TutorialStageBase {

	private enum Step { 
		Started,
		Intro,
		Movement_Basics, // forward, turn, brake
		Movement_UpAndDown,
		Movement_LookingAround, // look around, reset
		Movement_Zone, // zone to enter to proceed further
		Movement_FreeMovement, // can fly freely and proceed whenever
		Track_Hoop,
		Track_Checkpoint,
		Track_Practise, // three hoops to fly through
		Track_Bonus, // speed bonus
		Track_BonusPickUp,
		Track_Effects,
		Track_BonusRespawn,
		Track_Zone, // zone to enter to proceed further
		Track_FreeMovement, // can fly freely and proceed whenever
		Finished
	}
	private Step currentStep = Step.Started;

	protected override string LocalizationKeyPrefix => "Introduction";

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
