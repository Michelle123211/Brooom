using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroductionTutorial : TutorialStageBase {

	private enum Step {
		Started,
		Part1_Start,
		Intro,
		Movement_Basics, // forward, turn, brake
		Movement_UpAndDown,
		Movement_LookingAround, // look around, reset
		Movement_Zone, // zone to enter to proceed further
		Movement_FreeMovement, // can fly freely and proceed whenever
		Part1_End,
		Part2_Start,
		Track_Hoop,
		Track_Checkpoint,
		Track_Practice, // three hoops to fly through
		Track_Bonus, // speed bonus
		Track_BonusPickUp,
		Track_Effects,
		Track_BonusRespawn,
		Track_Zone, // zone to enter to proceed further
		Track_FreeMovement, // can fly freely and proceed whenever
		Part2_End,
		Finished
	}
	private Step currentStep = Step.Started;

	protected override string LocalizationKeyPrefix => "Introduction";

	public override void Finish() {
		// Load Race scene so that the next stage can be started
		SceneLoader.Instance.LoadScene(Scene.Race);
	}

	public override string GetCurrentState() {
		return currentStep.ToString();
	}

	public override void SetCurrentState(string state) {
		Step lastStep = Enum.Parse<Step>(state);
		// Go back to a checkpoint
		if (lastStep == Step.Finished) // already finished
			lastStep = Step.Finished;
		else if (lastStep > Step.Part1_End) // in the middle of second part, go to the beginning
			lastStep = Step.Part1_End;
		else // possibly in the middle of first part, go to the beginning
			lastStep = Step.Started;
		currentStep = lastStep;
	}

	protected override bool CheckTriggerConditions() {
		// Tutorial scene
		//	- But if the tutorial is disabled, we trigger it automatically so it can be skipped and we are not stuck here in case tutorial is enabled later on
		if (!SettingsUI.enableTutorial) return true;
		return SceneLoader.Instance.CurrentScene == Scene.Tutorial;
	}

	// Prepares everything necessary based on the initial step (e.g. move player if starting in Track_Hoop)
	protected override IEnumerator InitializeTutorialStage() {
		// We should be already in Tutorial scene
		TutorialSceneManager.Instance.ResetAll();
		Tutorial.Instance.panel.ShowEscapePanel();
		// Move player to a suitable position
		if (currentStep == Step.Part2_Start) {
			// Where the tutorial trigger zone would be
			TutorialSceneManager.Instance.MovePlayerTo(TutorialSceneManager.Instance.tutorialTriggerZone.transform.position.WithY(5));
		}
		yield break;
	}

	protected override bool UpdateTutorialStage() {
		// Handle moving from one part to another
		switch (currentStep) {
			case Step.Started:
				currentStep = Step.Part1_Start;
				Tutorial.Instance.StartCoroutine(GoThroughPart1());
				break;
			case Step.Part1_End:
				currentStep = Step.Part2_Start;
				Tutorial.Instance.StartCoroutine(GoThroughPart2());
				break;
			case Step.Part2_End:
				currentStep = Step.Finished;
				return false;
		}
		return true;
	}

	private IEnumerator GoThroughPart1() {
		// Introduction
		TutorialSceneManager.Instance.DisablePlayerActions(true);
		currentStep = Step.Intro;
		Tutorial.Instance.FadeOut();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()), TutorialPanelAlignment.Middle);
		// Forward, brake, turn
		currentStep = Step.Movement_Basics;
		Tutorial.Instance.FadeIn();
		TutorialSceneManager.Instance.EnablePlayerActions(false);
		string[] forwardBrake = InputManager.Instance.GetBindingTextForAction("Forward").Split('/');
		string[] leftRight = InputManager.Instance.GetBindingTextForAction("Turn").Split('/');
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitUntilVisible(
			string.Format(GetLocalizedText(currentStep.ToString()),
				forwardBrake[0], forwardBrake[1], leftRight[0], leftRight[1]));
		yield return WaitUntilStepIsFinished<BasicMovementProgress>();
		// Up and down
		currentStep = Step.Movement_UpAndDown;
		string[] upDown = InputManager.Instance.GetBindingTextForAction("Pitch").Split('/');
		yield return Tutorial.Instance.panel.HideTutorialPanelAndWaitUntilInvisible();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitUntilVisible(
			string.Format(GetLocalizedText(currentStep.ToString()), upDown[0], upDown[1]));
		yield return WaitUntilStepIsFinished<UpAndDownMovementProgress>();
		// Look around, back view, reset view
		currentStep = Step.Movement_LookingAround;
		string backView = InputManager.Instance.GetBindingTextForAction("BackView");
		string resetView = InputManager.Instance.GetBindingTextForAction("ResetView");
		yield return Tutorial.Instance.panel.HideTutorialPanelAndWaitUntilInvisible();
		TutorialSceneManager.Instance.EnableLookingAround();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitUntilVisible(
			string.Format(GetLocalizedText(currentStep.ToString()), backView, resetView));
		yield return WaitUntilStepIsFinished<LookAroundMovementProgress>();
		// Showing trigger zone
		currentStep = Step.Movement_Zone;
		yield return Tutorial.Instance.panel.HideTutorialPanelAndWaitUntilInvisible();
		TutorialSceneManager.Instance.DisablePlayerActions(true);
		TutorialSceneManager.Instance.ShowTutorialTriggerZone();
		TutorialSceneManager.Instance.cutsceneCamera.MoveCameraToLookAt(TutorialSceneManager.Instance.tutorialTriggerZone.transform, new Vector3(0, -20, 50));
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		// Flying freely
		currentStep = Step.Movement_FreeMovement;
		TutorialSceneManager.Instance.RotatePlayerTowards(TutorialSceneManager.Instance.tutorialTriggerZone.transform);
		TutorialSceneManager.Instance.EnablePlayerActions(true);
		TutorialSceneManager.Instance.cutsceneCamera.ResetView();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitUntilVisible(
			string.Format(GetLocalizedText(currentStep.ToString()),
			forwardBrake[0], forwardBrake[1], leftRight[0], leftRight[1], upDown[0], upDown[1], backView, resetView));
		yield return WaitUntilStepIsFinished<TutorialTriggerZoneProgress>();
		// End
		TutorialSceneManager.Instance.HideTutorialTriggerZone();
		Tutorial.Instance.panel.HideEscapePanel();
		yield return Tutorial.Instance.panel.HideTutorialPanelAndWaitUntilInvisible();
		currentStep = Step.Part1_End;
	}

	private IEnumerator GoThroughPart2() {
		currentStep = Step.Track_Hoop;

		currentStep = Step.Track_Checkpoint;

		currentStep = Step.Track_Practice;

		currentStep = Step.Track_Bonus;

		currentStep = Step.Track_BonusPickUp;

		currentStep = Step.Track_Effects;

		currentStep = Step.Track_BonusRespawn;

		currentStep = Step.Track_Zone;
		yield return Tutorial.Instance.panel.HideTutorialPanelAndWaitUntilInvisible();
		TutorialSceneManager.Instance.DisablePlayerActions(true);
		TutorialSceneManager.Instance.ShowTutorialTriggerZone();
		TutorialSceneManager.Instance.cutsceneCamera.MoveCameraToLookAt(TutorialSceneManager.Instance.tutorialTriggerZone.transform, new Vector3(0, -20, 50));
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));

		currentStep = Step.Track_FreeMovement;
		TutorialSceneManager.Instance.RotatePlayerTowards(TutorialSceneManager.Instance.tutorialTriggerZone.transform);
		TutorialSceneManager.Instance.EnablePlayerActions(true);
		TutorialSceneManager.Instance.cutsceneCamera.ResetView();

		currentStep = Step.Part2_End;
		yield break;
	}

}

internal class BasicMovementProgress : TutorialStepProgressTracker {

	private int forwardCount, brakeCount, leftCount, rightCount;
	private float previousForwardValue, previousTurnValue;

	protected override bool CheckIfPossibleToMoveToNextStep() {
		// Enough time has passed (10 s) and each direction was used enough times (3x) to progress further
		return elapsedTime > 10 && forwardCount >= 3 && brakeCount >= 3 && leftCount >= 3 && rightCount >= 3;
	}

	protected override void FinishStepProgress() {
	}

	protected override void InitializeStepProgress() {
		forwardCount = 0;
		brakeCount = 0;
		leftCount = 0;
		rightCount = 0;
		previousForwardValue = 0;
		previousTurnValue = 0;
	}

	protected override void UpdateStepProgress() {
		// Detect changes in direction - forward/brake, left/right
		float forwardValue = InputManager.Instance.GetFloatValue("Forward");
		float turnValue = InputManager.Instance.GetFloatValue("Turn");
		// - forward
		if (forwardValue < 0 && previousForwardValue >= 0) {
			forwardCount++;
			previousForwardValue = forwardValue;
		}
		// - brake
		if (forwardValue > 0 && previousForwardValue <= 0) {
			brakeCount++;
			previousForwardValue = forwardValue;
		}
		// - left
		if (turnValue < 0 && previousTurnValue >= 0) {
			leftCount++;
			previousTurnValue = turnValue;
		}
		// - right
		if (turnValue > 0 && previousTurnValue <= 0) {
			rightCount++;
			previousTurnValue = turnValue;
		}
	}
}

internal class UpAndDownMovementProgress : TutorialStepProgressTracker {

	private int upCount, downCount;
	private float previousUpValue;

	protected override bool CheckIfPossibleToMoveToNextStep() {
		// Enough time has passed (10 s) and each direction was used enough times (3x) to progress further
		return elapsedTime > 10 && upCount >= 3 && downCount >= 3;
	}

	protected override void FinishStepProgress() {
	}

	protected override void InitializeStepProgress() {
		upCount = 0;
		downCount = 0;
		previousUpValue = -1;
	}

	protected override void UpdateStepProgress() {
		// Detect changes in direction
		float upValue = InputManager.Instance.GetFloatValue("Pitch");
		// - up
		if (upValue < 0 && previousUpValue >= 0) {
			upCount++;
			previousUpValue = upValue;
		}
		// - down
		if (upValue > 0 && previousUpValue <= 0) {
			downCount++;
			previousUpValue = upValue;
		}
	}
}

internal class LookAroundMovementProgress : TutorialStepProgressTracker {

	private int resetViewCount, backViewCount;

	protected override bool CheckIfPossibleToMoveToNextStep() {
		// Enough time has passed (10 s) and each action was used enough times (2x) to progress further
		return elapsedTime > 10 && resetViewCount >= 2 && backViewCount >= 4; // back view is toggle, so we need at least 4
	}

	protected override void FinishStepProgress() {
	}

	protected override void InitializeStepProgress() {
		resetViewCount = 0;
		backViewCount = 0;
	}

	protected override void UpdateStepProgress() {
		if (InputManager.Instance.GetBoolValue("ResetView"))
			resetViewCount++;
		if (InputManager.Instance.GetBoolValue("BackView"))
			backViewCount++;
	}
}

internal class TutorialTriggerZoneProgress : TutorialStepProgressTracker {

	private bool zoneTriggered;

	protected override bool CheckIfPossibleToMoveToNextStep() {
		return zoneTriggered;
	}

	protected override void FinishStepProgress() {
		TutorialSceneManager.Instance.tutorialTriggerZone.onPlayerEntered -= OnPlayerEnteredZone;
	}

	protected override void InitializeStepProgress() {
		zoneTriggered = false;
		TutorialSceneManager.Instance.tutorialTriggerZone.onPlayerEntered += OnPlayerEnteredZone;
	}

	protected override void UpdateStepProgress() {
	}

	private void OnPlayerEnteredZone() {
		zoneTriggered = true;
	}
}