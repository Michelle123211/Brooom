using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstRaceTutorial : TutorialStageBase {

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

	protected override string LocalizationKeyPrefix => "FirstRace";

	private CharacterMovementController player;

	public override void Finish() {
		// We should stay in the Race scene, so we need to reset everything
		GamePause gamePause = UtilsMonoBehaviour.FindObject<GamePause>();
		gamePause.SetupOptionsForRace();
		gamePause.ResumeGame(); // calling directly and not through ResumeGame() because that would postpone it to update which will no longer happen
		Tutorial.Instance.panel.HideAllTutorialPanels();
		Tutorial.Instance.skipTrainingPanel.TweenAwareDisable();
		if (currentStep != Step.Finished) // tutorial was skipped
			TutorialBasicManager.Instance.EnablePlayerActions();
		TutorialBasicManager.Instance.cutsceneCamera.ResetView();
	}

	public override string GetCurrentState() {
		return currentStep.ToString();
	}

	public override void SetCurrentState(string state) {
		// Always starting from the beginning
		currentStep = Step.NotStarted;
	}

	protected override bool CheckTriggerConditions() {
		// Race scene
		return SceneLoader.Instance.CurrentScene == Scene.Race;
	}

	protected override IEnumerator InitializeTutorialStage() {
		// Prepare everything necessary
		SettingsUI.skipTraining = false; // in tutorial, don't skip training regardless of the settings
		player = UtilsMonoBehaviour.FindObjectOfTypeAndTag<CharacterMovementController>("Player");
		UtilsMonoBehaviour.FindObject<GamePause>().SetupOptionsForTutorial();
		Tutorial.Instance.panel.ShowEscapePanel();
		// Let RaceController know tutorial is running (so that training is not skipped and visited regions are not stored)
		yield return new WaitUntil(() => RaceController.Instance.IsInitialized);
	}

	protected override bool UpdateTutorialStage() {
		// Handle starting the tutorial scenario
		if (currentStep == Step.NotStarted) {
			currentStep = Step.Started;
			Tutorial.Instance.StartCoroutine(GoThroughTutorialScenario());
		}
		return currentStep != Step.Finished;
	}

	private IEnumerator GoThroughTutorialScenario() {
		// Training + reset
		currentStep = Step.Training;
		TutorialBasicManager.Instance.DisablePlayerActions();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(
			string.Format(GetLocalizedText(currentStep.ToString()),
				InputManager.Instance.GetBindingTextForAction("Restart")));
		// Showing starting zone
		currentStep = Step.StartingZone;
		TutorialBasicManager.Instance.cutsceneCamera.MoveCameraToLookAt(UtilsMonoBehaviour.FindObject<StartingZone>().transform, new Vector3(0, -20, -50));
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		// Fly freely and proceed whenever
		currentStep = Step.FreeMovement;
		TutorialBasicManager.Instance.EnablePlayerActions();
		TutorialBasicManager.Instance.cutsceneCamera.ResetView();
		yield return WaitUntilStepIsFinished<StartingZoneProgress>();
		// Skip training option
		currentStep = Step.SkipTraining;
		PauseGame();
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		Tutorial.Instance.skipTrainingPanel.TweenAwareEnable();
		yield return WaitUntilStepIsFinished<SkipTrainingPanelProgress>();
		// End
		Tutorial.Instance.skipTrainingPanel.TweenAwareDisable();
		ResumeGame();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		Tutorial.Instance.panel.HideAllTutorialPanels();
		currentStep = Step.Finished;
	}

}

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
		SettingsUI settings = UtilsMonoBehaviour.FindObject<SettingsUI>();
		settings.LoadSettingsValues();
		SettingsUI.skipTraining = !enableTrainingToggle.isOn;
		settings.SaveSettingsValues();
		confirmed = true;
	}
}