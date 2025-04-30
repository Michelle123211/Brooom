using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// An object representation of the Introduction tutorial stage - introducing the player to the basic controls and track elements.
/// It keeps track of the progress within this stage and moves forward from one step to another.
/// </summary>
public class IntroductionTutorial : TutorialStageBase {

	// All steps of this tutorial stage
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

	/// <inheritdoc/>
	protected override string LocalizationKeyPrefix => "Introduction";

	/// <summary>
	/// <inheritdoc/>
	/// Loads Race scene so that the next tutorial stage can be started rightaway.
	/// </summary>
	public override void Finish() {
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} finished.");
		// Load Race scene so that the next stage can be started
		SceneLoader.Instance.LoadScene(Scene.Race);
	}

	/// <inheritdoc/>
	public override string GetCurrentState() {
		return currentStep.ToString();
	}

	/// <inheritdoc/>
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

	/// <summary>
	/// <inheritdoc/>
	/// For this stage to start, the current scene must be Tutorial.
	/// But if the tutorial is disabled in settings, this stage is automatically triggered so it can be skipped and the tutorial can move on. 
	/// </summary>
	/// <returns><inheritdoc/></returns>
	protected override bool CheckTriggerConditions() {
		// Tutorial scene
		//	- But if the tutorial is disabled, we trigger it automatically so it can be skipped and we are not stuck here in case tutorial is enabled later on
		if (!SettingsUI.enableTutorial) return true;
		return SceneLoader.Instance.CurrentScene == Scene.Tutorial;
	}

	/// <inheritdoc/>
	protected override IEnumerator InitializeTutorialStage() {
		// We should be already in Tutorial scene
		TutorialSceneManager.Instance.ResetAll();
		Tutorial.Instance.panel.ShowEscapePanel();
		yield break;
	}

	/// <summary>
	/// Updates the tutorial stage (called from <c>Update()</c> method).
	/// Handles starting the scenario, moving from one part of this tutorial stage to another, and also moving to the finished state.
	/// </summary>
	/// <returns><inheritdoc/></returns>
	protected override bool UpdateTutorialStage() {
		// Handle moving from one part to another
		switch (currentStep) {
			case Step.NotStarted:
				currentStep = Step.Part1_Start;
				Tutorial.Instance.StartCoroutine(GoThroughPart1());
				break;
			case Step.Part1_End:
			case Step.BeforePart2:
				currentStep = Step.Part2_Start;
				Tutorial.Instance.StartCoroutine(GoThroughPart2());
				break;
			case Step.Part2_End:
				currentStep = Step.Finished;
				return false;
		}
		return true;
	}

	// The whole scenario of the first part of this tutorial stage
	private IEnumerator GoThroughPart1() {
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} (part 1) started.");
		
		// Introduction
		TutorialBasicManager.Instance.DisablePlayerActions();
		currentStep = Step.Intro;
		Tutorial.Instance.FadeOut();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()), TutorialPanelAlignment.Middle);
		
		// Forward, brake, turn
		currentStep = Step.Movement_Basics;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
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
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		string[] upDown = InputManager.Instance.GetBindingTextForAction("Pitch").Split('/');
		yield return Tutorial.Instance.panel.HideTutorialPanelAndWaitUntilInvisible();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitUntilVisible(
			string.Format(GetLocalizedText(currentStep.ToString()), upDown[0], upDown[1]));
		yield return WaitUntilStepIsFinished<UpAndDownMovementProgress>();
		
		// Look around, back view, reset view
		currentStep = Step.Movement_LookingAround;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		string backView = InputManager.Instance.GetBindingTextForAction("BackView");
		string resetView = InputManager.Instance.GetBindingTextForAction("ResetView");
		yield return Tutorial.Instance.panel.HideTutorialPanelAndWaitUntilInvisible();
		TutorialBasicManager.Instance.EnableLookingAround();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitUntilVisible(
			string.Format(GetLocalizedText(currentStep.ToString()), backView, resetView));
		yield return WaitUntilStepIsFinished<LookAroundMovementProgress>();
		
		// Showing trigger zone
		currentStep = Step.Movement_Zone;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		yield return Tutorial.Instance.panel.HideTutorialPanelAndWaitUntilInvisible();
		TutorialBasicManager.Instance.DisablePlayerActions();
		TutorialSceneManager.Instance.ShowTutorialTriggerZone();
		TutorialBasicManager.Instance.cutsceneCamera.MoveCameraToLookAt(TutorialSceneManager.Instance.tutorialTriggerZone.transform, new Vector3(0, -20, 50));
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// Flying freely
		currentStep = Step.Movement_FreeMovement;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
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
	}

	// The whole scenario of the seconds part of this tutorial stage
	private IEnumerator GoThroughPart2() {
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} (part 2) started.");

		// Hoop introduction
		currentStep = Step.Track_Hoop;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		TutorialBasicManager.Instance.DisablePlayerActions();
		TutorialSceneManager.Instance.ShowSimpleTrack();
		TutorialBasicManager.Instance.cutsceneCamera.MoveCameraToLookAt(TutorialSceneManager.Instance.hoops[0].transform, 10, Vector3.forward);
		TutorialSceneManager.Instance.HideTutorialTriggerZone();
		TutorialBasicManager.Instance.ResetPlayerPositionAndRotation();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// Checkpoint introduction
		currentStep = Step.Track_Checkpoint;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		TutorialBasicManager.Instance.cutsceneCamera.MoveCameraToLookAt(TutorialSceneManager.Instance.checkpoints[0].transform, 15, Vector3.forward);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// Small track - flying through hoops and checkpoints
		currentStep = Step.Track_Practice;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		TutorialBasicManager.Instance.EnablePlayerActions();
		TutorialBasicManager.Instance.cutsceneCamera.ResetView();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitUntilVisible(GetLocalizedText(currentStep.ToString()));
		yield return WaitUntilStepIsFinished<SmallTrackProgress>();
		
		// Bonus introduction
		currentStep = Step.Track_Bonus;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		TutorialBasicManager.Instance.DisablePlayerActions();
		TutorialSceneManager.Instance.HideSimpleTrack();
		UtilsMonoBehaviour.SetActiveForAll(TutorialSceneManager.Instance.speedBonuses, true);
		TutorialBasicManager.Instance.cutsceneCamera.MoveCameraToLookAt(TutorialSceneManager.Instance.speedBonuses[0].transform, 4, Vector3.forward);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// Pick up bonus
		currentStep = Step.Track_BonusPickUp;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		TutorialBasicManager.Instance.RotatePlayerTowards(TutorialSceneManager.Instance.speedBonuses[0].transform);
		TutorialBasicManager.Instance.EnablePlayerActions();
		TutorialBasicManager.Instance.cutsceneCamera.ResetView();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitUntilVisible(GetLocalizedText(currentStep.ToString()));
		yield return WaitUntilStepIsFinished<BonusProgress<SpeedBonusEffect>>();
		
		// Effects introduction
		currentStep = Step.Track_Effects;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		yield return new WaitForSecondsRealtime(1);
		Tutorial.Instance.highlighter.Highlight(UtilsMonoBehaviour.FindObject<EffectsUI>().GetComponent<RectTransform>(), padding: 10);
		PauseGame();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// Bonus respawn + pick up
		currentStep = Step.Track_BonusRespawn;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		ResumeGame();
		Tutorial.Instance.highlighter.StopHighlighting();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitUntilVisible(GetLocalizedText(currentStep.ToString()));
		yield return WaitUntilStepIsFinished<BonusProgress<SpeedBonusEffect>>();
		yield return WaitUntilStepIsFinished<EffectsProgress>();
		
		// Showing trigger zone
		currentStep = Step.Track_Zone;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		yield return Tutorial.Instance.panel.HideTutorialPanelAndWaitUntilInvisible();
		TutorialBasicManager.Instance.DisablePlayerActions();
		TutorialSceneManager.Instance.ShowTutorialTriggerZone();
		TutorialBasicManager.Instance.cutsceneCamera.MoveCameraToLookAt(TutorialSceneManager.Instance.tutorialTriggerZone.transform, new Vector3(0, -20, 50));
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// Flying freely
		currentStep = Step.Track_FreeMovement;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		TutorialBasicManager.Instance.RotatePlayerTowards(TutorialSceneManager.Instance.tutorialTriggerZone.transform);
		TutorialBasicManager.Instance.EnablePlayerActions();
		TutorialBasicManager.Instance.cutsceneCamera.ResetView();
		yield return WaitUntilStepIsFinished<TutorialTriggerZoneProgress>();
		
		// End
		Tutorial.Instance.panel.HideAllTutorialPanels();
		currentStep = Step.Part2_End;
	}

}


/// <summary>
/// A class tracking progress based on basic movement.
/// The player has to use actions for flying forward, braking and turning several times.
/// </summary>
internal class BasicMovementProgress : TutorialStepProgressTracker {

	private int forwardCount, brakeCount, leftCount, rightCount;
	private float previousForwardValue, previousTurnValue;

	/// <inheritdoc/>
	protected override bool CheckIfPossibleToMoveToNextStep() {
		// Enough time has passed (10 s) and each direction was used enough times (2-3x) to progress further
		return elapsedTime > 10 && forwardCount >= 2 && brakeCount >= 2 && leftCount >= 3 && rightCount >= 3;
	}

	/// <inheritdoc/>
	protected override void FinishStepProgress() {
	}

	/// <inheritdoc/>
	protected override void InitializeStepProgress() {
		forwardCount = 0;
		brakeCount = 0;
		leftCount = 0;
		rightCount = 0;
		previousForwardValue = 0;
		previousTurnValue = 0;
	}

	/// <inheritdoc/>
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

/// <summary>
/// A class tracking progress based on basic movement.
/// The player has to use actions for flying up and down several times.
/// </summary>
internal class UpAndDownMovementProgress : TutorialStepProgressTracker {

	private int upCount, downCount;
	private float previousUpValue;

	/// <inheritdoc/>
	protected override bool CheckIfPossibleToMoveToNextStep() {
		// Enough time has passed (10 s) and each direction was used enough times (3x) to progress further
		return elapsedTime > 10 && upCount >= 3 && downCount >= 3;
	}

	/// <inheritdoc/>
	protected override void FinishStepProgress() {
	}

	/// <inheritdoc/>
	protected override void InitializeStepProgress() {
		upCount = 0;
		downCount = 0;
		previousUpValue = -1;
	}

	/// <inheritdoc/>
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

/// <summary>
/// A class tracking progress based on looking around.
/// The player has to use actions for back view and reseting view several times.
/// </summary>
internal class LookAroundMovementProgress : TutorialStepProgressTracker {

	private int resetViewCount, backViewCount;

	/// <inheritdoc/>
	protected override bool CheckIfPossibleToMoveToNextStep() {
		// Enough time has passed (10 s) and each action was used enough times (2x) to progress further
		return elapsedTime > 10 && resetViewCount >= 2 && backViewCount >= 4; // back view is toggle, so we need at least 4
	}

	/// <inheritdoc/>
	protected override void FinishStepProgress() {
	}

	/// <inheritdoc/>
	protected override void InitializeStepProgress() {
		resetViewCount = 0;
		backViewCount = 0;
	}

	/// <inheritdoc/>
	protected override void UpdateStepProgress() {
		if (InputManager.Instance.GetBoolValue("ResetView"))
			resetViewCount++;
		if (InputManager.Instance.GetBoolValue("BackView"))
			backViewCount++;
	}
}

/// <summary>
/// A class tracking progress based on a tutorial trigger zone.
/// The player has to enter the trigger zone.
/// </summary>
internal class TutorialTriggerZoneProgress : TutorialStepProgressTracker {

	private bool zoneTriggered;

	/// <inheritdoc/>
	protected override bool CheckIfPossibleToMoveToNextStep() {
		return zoneTriggered;
	}

	/// <inheritdoc/>
	protected override void FinishStepProgress() {
		TutorialSceneManager.Instance.tutorialTriggerZone.onPlayerEntered -= OnPlayerEnteredZone;
	}

	/// <inheritdoc/>
	protected override void InitializeStepProgress() {
		zoneTriggered = false;
		TutorialSceneManager.Instance.tutorialTriggerZone.onPlayerEntered += OnPlayerEnteredZone;
	}

	/// <inheritdoc/>
	protected override void UpdateStepProgress() {
	}

	private void OnPlayerEnteredZone() {
		zoneTriggered = true;
	}
}

/// <summary>
/// A class tracking progress in a small track.
/// The player has to fly through all hoops and checkpoints.
/// </summary>
internal class SmallTrackProgress : TutorialStepProgressTracker {

	private bool[] hoopPassed;
	private int hoopsRemaining;

	/// <inheritdoc/>
	protected override bool CheckIfPossibleToMoveToNextStep() {
		return hoopsRemaining == 0;
	}

	/// <inheritdoc/>
	protected override void FinishStepProgress() {
		// Unregister callbacks
		foreach (var hoop in TutorialSceneManager.Instance.hoops) {
			hoop.onHoopPassed -= OnHoopPassed;
		}
		foreach (var hoop in TutorialSceneManager.Instance.checkpoints) {
			hoop.onHoopPassed -= OnHoopPassed;
		}
	}

	/// <inheritdoc/>
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

	/// <inheritdoc/>
	protected override void UpdateStepProgress() {
	}

	private void OnHoopPassed(int hoopIndex) {
		if (!hoopPassed[hoopIndex]) hoopsRemaining--;
		hoopPassed[hoopIndex] = true;
	}
}

/// <summary>
/// A class tracking progress based collected bonuses.
/// The player has to collect a certain type of bonus a certain number of times.
/// The type parameter determines the type of bonus and a static data field determines the number.
/// </summary>
public class BonusProgress<T> : TutorialStepProgressTracker where T : BonusEffect {
	
	public static int count = 1; // How many times the bonus should be picked up before the progress is finished (static so we can set it without instantiation)

	private int bonusPickedUpCount;

	/// <inheritdoc/>
	protected override bool CheckIfPossibleToMoveToNextStep() {
		return bonusPickedUpCount == count;
	}

	/// <inheritdoc/>
	protected override void FinishStepProgress() {
		Messaging.UnregisterFromMessage("BonusPickedUp", OnBonusPickedUp);
	}

	/// <inheritdoc/>
	protected override void InitializeStepProgress() {
		Messaging.RegisterForMessage("BonusPickedUp", OnBonusPickedUp);
		bonusPickedUpCount = 0;
	}

	/// <inheritdoc/>
	protected override void UpdateStepProgress() {
	}

	private void OnBonusPickedUp(GameObject bonus) {
		// Check if it is bonus of the desired type
		if (bonus.TryGetComponent<T>(out _)) bonusPickedUpCount++;
	}
}

/// <summary>
/// A class tracking progress based on effects affecting the player.
/// There mustn't be any effect affecting the player.
/// </summary>
internal class EffectsProgress : TutorialStepProgressTracker {

	EffectibleCharacter playerEffects;

	/// <inheritdoc/>
	protected override bool CheckIfPossibleToMoveToNextStep() {
		return playerEffects.effects.Count == 0;
	}

	/// <inheritdoc/>
	protected override void FinishStepProgress() {
	}

	/// <inheritdoc/>
	protected override void InitializeStepProgress() {
		playerEffects = TutorialSceneManager.Instance.player.GetComponentInChildren<EffectibleCharacter>();
	}

	/// <inheritdoc/>
	protected override void UpdateStepProgress() {
	}
}