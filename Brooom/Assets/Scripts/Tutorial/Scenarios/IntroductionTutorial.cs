using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroductionTutorial : TutorialStageBase {

	private enum Step {
		NotStarted,
		Part1_Start,
		Intro,
		Movement_Basics, // forward, turn, brake
		Movement_UpAndDown,
		Movement_LookingAround, // look around, reset
		Movement_Zone, // zone to enter to proceed further
		Movement_FreeMovement, // can fly freely and proceed whenever
		Part1_End,
		BeforePart2,
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
	private Step currentStep = Step.NotStarted;

	protected override string LocalizationKeyPrefix => "Introduction";

	public override void Finish() {
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.Finish()");
		// Load Race scene so that the next stage can be started
		SceneLoader.Instance.LoadScene(Scene.Race);
	}

	public override string GetCurrentState() {
		return currentStep.ToString();
	}

	public override void SetCurrentState(string state) {
		Step lastStep = Enum.Parse<Step>(state);
		// Go back to a checkpoint
		if (lastStep >= Step.Finished) // already finished
			lastStep = Step.Finished;
		else if (lastStep >= Step.BeforePart2) // in the middle of second part, go to the beginning
			lastStep = Step.BeforePart2;
		else // possibly in the middle of first part, go to the beginning
			lastStep = Step.NotStarted;
		currentStep = lastStep;
	}

	protected override bool CheckTriggerConditions() {
		// Tutorial scene
		//	- But if the tutorial is disabled, we trigger it automatically so it can be skipped and we are not stuck here in case tutorial is enabled later on
		if (Tutorial.Instance.debugLogs && !SettingsUI.enableTutorial)
			Debug.Log($"IntroductionTutorial.CheckTriggerConditions(): Conditions satisfied, tutorial enabled is {SettingsUI.enableTutorial}.");
		if (!SettingsUI.enableTutorial) return true;
		if (Tutorial.Instance.debugLogs && SceneLoader.Instance.CurrentScene == Scene.Tutorial)
			Debug.Log($"IntroductionTutorial.CheckTriggerConditions(): Conditions satisfied, scene is {SceneLoader.Instance.CurrentScene}.");
		return SceneLoader.Instance.CurrentScene == Scene.Tutorial;
	}

	protected override IEnumerator InitializeTutorialStage() {
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.InitializeTutorialStage()");
		// We should be already in Tutorial scene
		TutorialSceneManager.Instance.ResetAll();
		Tutorial.Instance.panel.ShowEscapePanel();
		yield break;
	}

	protected override bool UpdateTutorialStage() {
		// Handle moving from one part to another
		switch (currentStep) {
			case Step.NotStarted:
				currentStep = Step.Part1_Start;
				if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.UpdateTutorialStage(): Starting GoThroughPart1() as a coroutine.");
				Tutorial.Instance.StartCoroutine(GoThroughPart1());
				break;
			case Step.Part1_End:
			case Step.BeforePart2:
				if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.UpdateTutorialStage(): Starting GoThroughPart2() as a coroutine.");
				currentStep = Step.Part2_Start;
				Tutorial.Instance.StartCoroutine(GoThroughPart2());
				break;
			case Step.Part2_End:
				if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.UpdateTutorialStage(): Finishing the tutorial.");
				currentStep = Step.Finished;
				return false;
		}
		return true;
	}

	private IEnumerator GoThroughPart1() {
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.GoThroughPart1(): Started.");
		
		// Introduction
		TutorialBasicManager.Instance.DisablePlayerActions();
		currentStep = Step.Intro;
		Tutorial.Instance.FadeOut();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()), TutorialPanelAlignment.Middle);
		
		// Forward, brake, turn
		currentStep = Step.Movement_Basics;
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.GoThroughPart1(): Current step {currentStep}.");
		Tutorial.Instance.FadeIn();
		TutorialBasicManager.Instance.EnablePlayerActions(false);
		string[] forwardBrake = InputManager.Instance.GetBindingTextForAction("Forward").Split('/');
		string[] leftRight = InputManager.Instance.GetBindingTextForAction("Turn").Split('/');
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitUntilVisible(
			string.Format(GetLocalizedText(currentStep.ToString()),
				forwardBrake[0], forwardBrake[1], leftRight[0], leftRight[1]));
		yield return WaitUntilStepIsFinished<BasicMovementProgress>();
		
		// Up and down
		currentStep = Step.Movement_UpAndDown;
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.GoThroughPart1(): Current step {currentStep}.");
		string[] upDown = InputManager.Instance.GetBindingTextForAction("Pitch").Split('/');
		yield return Tutorial.Instance.panel.HideTutorialPanelAndWaitUntilInvisible();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitUntilVisible(
			string.Format(GetLocalizedText(currentStep.ToString()), upDown[0], upDown[1]));
		yield return WaitUntilStepIsFinished<UpAndDownMovementProgress>();
		
		// Look around, back view, reset view
		currentStep = Step.Movement_LookingAround;
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.GoThroughPart1(): Current step {currentStep}.");
		string backView = InputManager.Instance.GetBindingTextForAction("BackView");
		string resetView = InputManager.Instance.GetBindingTextForAction("ResetView");
		yield return Tutorial.Instance.panel.HideTutorialPanelAndWaitUntilInvisible();
		TutorialBasicManager.Instance.EnableLookingAround();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitUntilVisible(
			string.Format(GetLocalizedText(currentStep.ToString()), backView, resetView));
		yield return WaitUntilStepIsFinished<LookAroundMovementProgress>();
		
		// Showing trigger zone
		currentStep = Step.Movement_Zone;
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.GoThroughPart1(): Current step {currentStep}.");
		yield return Tutorial.Instance.panel.HideTutorialPanelAndWaitUntilInvisible();
		TutorialBasicManager.Instance.DisablePlayerActions();
		TutorialSceneManager.Instance.ShowTutorialTriggerZone();
		TutorialBasicManager.Instance.cutsceneCamera.MoveCameraToLookAt(TutorialSceneManager.Instance.tutorialTriggerZone.transform, new Vector3(0, -20, 50));
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// Flying freely
		currentStep = Step.Movement_FreeMovement;
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.GoThroughPart1(): Current step {currentStep}.");
		TutorialBasicManager.Instance.RotatePlayerTowards(TutorialSceneManager.Instance.tutorialTriggerZone.transform);
		TutorialBasicManager.Instance.EnablePlayerActions();
		TutorialBasicManager.Instance.cutsceneCamera.ResetView();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitUntilVisible(
			string.Format(GetLocalizedText(currentStep.ToString()),
			forwardBrake[0], forwardBrake[1], leftRight[0], leftRight[1], upDown[0], upDown[1], backView, resetView));
		yield return WaitUntilStepIsFinished<TutorialTriggerZoneProgress>();
		
		// End
		TutorialSceneManager.Instance.HideTutorialTriggerZone();
		yield return Tutorial.Instance.panel.HideTutorialPanelAndWaitUntilInvisible();
		currentStep = Step.Part1_End;
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.GoThroughPart1(): Current step {currentStep}.");
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.GoThroughPart1(): Finished.");
	}

	private IEnumerator GoThroughPart2() {
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.GoThroughPart2(): Started.");

		// Hoop introduction
		currentStep = Step.Track_Hoop;
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.GoThroughPart2(): Current step {currentStep}.");
		TutorialBasicManager.Instance.DisablePlayerActions();
		TutorialSceneManager.Instance.ShowSimpleTrack();
		TutorialBasicManager.Instance.cutsceneCamera.MoveCameraToLookAt(TutorialSceneManager.Instance.hoops[0].transform, 10, Vector3.forward);
		TutorialSceneManager.Instance.HideTutorialTriggerZone();
		TutorialBasicManager.Instance.ResetPlayerPositionAndRotation();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// Checkpoint introduction
		currentStep = Step.Track_Checkpoint;
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.GoThroughPart2(): Current step {currentStep}.");
		TutorialBasicManager.Instance.cutsceneCamera.MoveCameraToLookAt(TutorialSceneManager.Instance.checkpoints[0].transform, 15, Vector3.forward);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// Small track - flying through hoops and checkpoints
		currentStep = Step.Track_Practice;
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.GoThroughPart2(): Current step {currentStep}.");
		TutorialBasicManager.Instance.EnablePlayerActions();
		TutorialBasicManager.Instance.cutsceneCamera.ResetView();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitUntilVisible(GetLocalizedText(currentStep.ToString()));
		yield return WaitUntilStepIsFinished<SmallTrackProgress>();
		
		// Bonus introduction
		currentStep = Step.Track_Bonus;
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.GoThroughPart2(): Current step {currentStep}.");
		TutorialBasicManager.Instance.DisablePlayerActions();
		TutorialSceneManager.Instance.HideSimpleTrack();
		UtilsMonoBehaviour.SetActiveForAll(TutorialSceneManager.Instance.speedBonuses, true);
		TutorialBasicManager.Instance.cutsceneCamera.MoveCameraToLookAt(TutorialSceneManager.Instance.speedBonuses[0].transform, 4, Vector3.forward);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// Pick up bonus
		currentStep = Step.Track_BonusPickUp;
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.GoThroughPart2(): Current step {currentStep}.");
		TutorialBasicManager.Instance.RotatePlayerTowards(TutorialSceneManager.Instance.speedBonuses[0].transform);
		TutorialBasicManager.Instance.EnablePlayerActions();
		TutorialBasicManager.Instance.cutsceneCamera.ResetView();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitUntilVisible(GetLocalizedText(currentStep.ToString()));
		yield return WaitUntilStepIsFinished<BonusProgress<SpeedBonusEffect>>();
		
		// Effects introduction
		currentStep = Step.Track_Effects;
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.GoThroughPart2(): Current step {currentStep}.");
		yield return new WaitForSecondsRealtime(1);
		Tutorial.Instance.highlighter.Highlight(UtilsMonoBehaviour.FindObject<EffectsUI>().GetComponent<RectTransform>(), padding: 10);
		PauseGame();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// Bonus respawn + pick up
		currentStep = Step.Track_BonusRespawn;
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.GoThroughPart2(): Current step {currentStep}.");
		ResumeGame();
		Tutorial.Instance.highlighter.StopHighlighting();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitUntilVisible(GetLocalizedText(currentStep.ToString()));
		yield return WaitUntilStepIsFinished<BonusProgress<SpeedBonusEffect>>();
		yield return WaitUntilStepIsFinished<EffectsProgress>();
		
		// Showing trigger zone
		currentStep = Step.Track_Zone;
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.GoThroughPart2(): Current step {currentStep}.");
		yield return Tutorial.Instance.panel.HideTutorialPanelAndWaitUntilInvisible();
		TutorialBasicManager.Instance.DisablePlayerActions();
		TutorialSceneManager.Instance.ShowTutorialTriggerZone();
		TutorialBasicManager.Instance.cutsceneCamera.MoveCameraToLookAt(TutorialSceneManager.Instance.tutorialTriggerZone.transform, new Vector3(0, -20, 50));
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// Flying freely
		currentStep = Step.Track_FreeMovement;
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.GoThroughPart2(): Current step {currentStep}.");
		TutorialBasicManager.Instance.RotatePlayerTowards(TutorialSceneManager.Instance.tutorialTriggerZone.transform);
		TutorialBasicManager.Instance.EnablePlayerActions();
		TutorialBasicManager.Instance.cutsceneCamera.ResetView();
		yield return WaitUntilStepIsFinished<TutorialTriggerZoneProgress>();
		
		// End
		Tutorial.Instance.panel.HideAllTutorialPanels();
		currentStep = Step.Part2_End;
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.GoThroughPart2(): Current step {currentStep}.");
		if (Tutorial.Instance.debugLogs) Debug.Log($"IntroductionTutorial.GoThroughPart2(): Finished.");
	}

}

// The player has used actions for flying forward, braking and turning
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

// The player has used actions for flying up and down
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

// The player has used actions for back view and resetting view
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

// The player has entered the tutorial trigger zone
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

// The player has flown through all hoops and checkpoints
internal class SmallTrackProgress : TutorialStepProgressTracker {

	private bool[] hoopPassed;
	private int hoopsRemaining;

	protected override bool CheckIfPossibleToMoveToNextStep() {
		return hoopsRemaining == 0;
	}

	protected override void FinishStepProgress() {
		// Unregister callbacks
		foreach (var hoop in TutorialSceneManager.Instance.hoops) {
			hoop.onHoopPassed -= OnHoopPassed;
		}
		foreach (var hoop in TutorialSceneManager.Instance.checkpoints) {
			hoop.onHoopPassed -= OnHoopPassed;
		}
	}

	protected override void InitializeStepProgress() {
		// Activate all hoops and checkpoints and register callbacks
		int index = 1; // starting from 1 and not from zero so that race-specific code is not invoked (next hoop to pass is initialized to 0)
		foreach (var hoop in TutorialSceneManager.Instance.hoops) {
			hoop.Activate(index);
			hoop.onHoopPassed += OnHoopPassed;
			index++;
		}
		foreach (var hoop in TutorialSceneManager.Instance.checkpoints) {
			hoop.Activate(index);
			hoop.onHoopPassed += OnHoopPassed;
			index++;
		}
		hoopPassed = new bool[index];
		hoopsRemaining = index - 1;
	}

	protected override void UpdateStepProgress() {
	}

	private void OnHoopPassed(int hoopIndex) {
		if (!hoopPassed[hoopIndex]) hoopsRemaining--;
		hoopPassed[hoopIndex] = true;
	}
}

// A certain type of bonus has been picked up a certain number of times
public class BonusProgress<T> : TutorialStepProgressTracker where T : BonusEffect {
	
	public static int count = 1; // How many times the bonus should be picked up before the progress is finished (static so we can set it without instantiation)

	private int bonusPickedUpCount;

	protected override bool CheckIfPossibleToMoveToNextStep() {
		return bonusPickedUpCount == count;
	}

	protected override void FinishStepProgress() {
		Messaging.UnregisterFromMessage("BonusPickedUp", OnBonusPickedUp);
	}

	protected override void InitializeStepProgress() {
		Messaging.RegisterForMessage("BonusPickedUp", OnBonusPickedUp);
		bonusPickedUpCount = 0;
	}

	protected override void UpdateStepProgress() {
	}

	private void OnBonusPickedUp(GameObject bonus) {
		// Check if it is bonus of the desired type
		if (bonus.TryGetComponent<T>(out _)) bonusPickedUpCount++;
	}
}

// There are no more effects affecting the player
internal class EffectsProgress : TutorialStepProgressTracker {

	EffectibleCharacter playerEffects;

	protected override bool CheckIfPossibleToMoveToNextStep() {
		return playerEffects.effects.Count == 0;
	}

	protected override void FinishStepProgress() {
	}

	protected override void InitializeStepProgress() {
		playerEffects = TutorialSceneManager.Instance.player.GetComponentInChildren<EffectibleCharacter>();
	}

	protected override void UpdateStepProgress() {
	}
}