using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// An object representation of the First Race tutorial stage - introducing the player to the concept of training and starting zone.
/// It keeps track of the progress within this stage and moves forward from one step to another.
/// </summary>
public class FirstRaceTutorial : TutorialStageBase {

	// All steps of this tutorial stage
	private enum Step {
		NotStarted,
		Started,
		Training, // training, reset
		StartingZone,
		FreeMovement, // can fly freely and proceed whenever
		SkipTraining, // an option to skip training from now on
		Finished
	}
	private Step currentStep = Step.NotStarted;

	/// <inheritdoc/>
	protected override string LocalizationKeyPrefix => "FirstRace";

	private CharacterMovementController player;

	/// <inheritdoc/>
	public override void Finish() {
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} finished.");
		// We should stay in the Race scene, so we need to reset everything
		GamePause gamePause = UtilsMonoBehaviour.FindObject<GamePause>();
		gamePause.SetupOptionsForRace();
		gamePause.ResumeGame(); // calling directly and not through ResumeGame() because that would postpone it to update which will no longer happen
		Utils.DisableCursor();
		Tutorial.Instance.panel.HideAllTutorialPanels();
		Tutorial.Instance.skipTrainingPanel.TweenAwareDisable();
		if (currentStep != Step.Finished) // tutorial was skipped
			TutorialBasicManager.Instance.EnablePlayerActions();
		TutorialBasicManager.Instance.cutsceneCamera.ResetView();
	}

	/// <inheritdoc/>
	public override string GetCurrentState() {
		return currentStep.ToString();
	}

	/// <inheritdoc/>
	public override void SetCurrentState(string state) {
		// Always starting from the beginning
		currentStep = Step.NotStarted;
	}

	/// <summary>
	/// <inheritdoc/>
	/// For this stage to start, the current scene must be Race.
	/// </summary>
	/// <returns><inheritdoc/></returns>
	protected override bool CheckTriggerConditions() {
		// Race scene
		return SceneLoader.Instance.CurrentScene == Scene.Race;
	}

	/// <inheritdoc/>
	protected override IEnumerator InitializeTutorialStage() {
		// Prepare everything necessary
		SettingsUI.skipTraining = false; // in tutorial, don't skip training regardless of the settings
		player = UtilsMonoBehaviour.FindObjectOfTypeAndTag<CharacterMovementController>("Player");
		UtilsMonoBehaviour.FindObject<GamePause>().SetupOptionsForTutorial();
		Tutorial.Instance.panel.ShowEscapePanel();
		// Let RaceController know tutorial is running (so that training is not skipped and visited regions are not stored)
		yield return new WaitUntil(() => RaceControllerBase.Instance.IsInitialized);
		(RaceControllerBase.Instance as RaceController).isInTutorial = true;
	}

	/// <summary>
	/// Updates the tutorial stage (called from <c>Update()</c> method).
	/// Handles starting the scenario.
	/// </summary>
	/// <returns><inheritdoc/></returns>
	protected override bool UpdateTutorialStage() {
		// Handle starting the tutorial scenario
		if (currentStep == Step.NotStarted) {
			currentStep = Step.Started;
			Tutorial.Instance.StartCoroutine(GoThroughTutorialScenario());
		}
		return currentStep != Step.Finished;
	}

	// The whole scenario of this tutorial stage
	private IEnumerator GoThroughTutorialScenario() {
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} started.");

		// Training + reset
		currentStep = Step.Training;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		TutorialBasicManager.Instance.DisablePlayerActions();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(
			string.Format(GetLocalizedText(currentStep.ToString()),
				InputManager.Instance.GetBindingTextForAction("Restart")));

		// Showing starting zone
		currentStep = Step.StartingZone;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		TutorialBasicManager.Instance.cutsceneCamera.MoveCameraToLookAt(UtilsMonoBehaviour.FindObject<StartingZone>().transform, new Vector3(0, -20, 50));
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// Fly freely and proceed whenever
		currentStep = Step.FreeMovement;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		TutorialBasicManager.Instance.EnablePlayerActions();
		TutorialBasicManager.Instance.cutsceneCamera.ResetView();
		yield return WaitUntilStepIsFinished<StartingZoneProgress>();
		
		// Skip training option
		currentStep = Step.SkipTraining;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		PauseGame();
		Utils.EnableCursor();
		Tutorial.Instance.skipTrainingPanel.TweenAwareEnable();
		yield return WaitUntilStepIsFinished<SkipTrainingPanelProgress>();
		
		// End
		Tutorial.Instance.skipTrainingPanel.TweenAwareDisable();
		ResumeGame();
		Utils.DisableCursor();
		Tutorial.Instance.panel.HideAllTutorialPanels();
		currentStep = Step.Finished;
	}

}

/// <summary>
/// A class tracking progress based on starting zone.
/// The player has to enter the race starting zone and the training needs to be ended.
/// </summary>
internal class StartingZoneProgress : TutorialStepProgressTracker {

	bool trainingEnded = false;

	protected override bool CheckIfPossibleToMoveToNextStep() {
		return trainingEnded;
	}

	protected override void FinishStepProgress() {
		Messaging.UnregisterFromMessage("TrainingEnded", OnTrainingEnded);
	}

	protected override void InitializeStepProgress() {
		Messaging.RegisterForMessage("TrainingEnded", OnTrainingEnded);
	}

	protected override void UpdateStepProgress() {
	}

	private void OnTrainingEnded(int _) {
		trainingEnded = true;
	}
}

/// <summary>
/// A class tracking progress based on displayed dialog for selectiong whether training should be skipped.
/// The player has to select an option and confirm it. This option is then persistently stored.
/// </summary>
internal class SkipTrainingPanelProgress : TutorialStepProgressTracker {

	private Button confirmButton;
	private Toggle enableTrainingToggle;
	private bool confirmed = false;

	protected override bool CheckIfPossibleToMoveToNextStep() {
		return confirmed;
	}

	protected override void FinishStepProgress() {
		confirmButton.onClick.RemoveListener(OnSettingsConfirmed);
	}

	protected override void InitializeStepProgress() {
		confirmButton = Tutorial.Instance.skipTrainingPanel.GetComponentInChildren<Button>();
		enableTrainingToggle = Tutorial.Instance.skipTrainingPanel.GetComponentInChildren<Toggle>();
		confirmButton.onClick.AddListener(OnSettingsConfirmed);
	}

	protected override void UpdateStepProgress() {
	}

	private void OnSettingsConfirmed() {
		// Save settings
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Skip training option is {(enableTrainingToggle.isOn ? "off" : "on")}.");
		SettingsUI settings = UtilsMonoBehaviour.FindObject<SettingsUI>();
		settings.LoadSettingsValues();
		SettingsUI.skipTraining = !enableTrainingToggle.isOn;
		settings.SaveSettingsValues();
		confirmed = true;
	}
}