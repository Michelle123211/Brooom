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
		if (Tutorial.Instance.debugLogs) Debug.Log($"TestingTrackTutorial.Finish()");
		Tutorial.Instance.panel.HideAllTutorialPanels();
		Tutorial.Instance.highlighter.StopHighlighting();
	}

	public override string GetCurrentState() {
		return currentStep.ToString();
	}

	public override void SetCurrentState(string state) {
		// Always starting from the beginning
		currentStep = Step.NotStarted;
	}

	protected override bool CheckTriggerConditions() {
		// Player Overview scene with Shop open
		if (Tutorial.Instance.debugLogs && SceneLoader.Instance.CurrentScene == Scene.PlayerOverview && TutorialPlayerOverviewReferences.Instance.shopUI.activeInHierarchy)
			Debug.Log($"TestingTrackTutorial.CheckTriggerConditions(): Conditions satisfied, scene is {SceneLoader.Instance.CurrentScene} and shop visible is {TutorialPlayerOverviewReferences.Instance.shopUI.activeInHierarchy}.");
		return SceneLoader.Instance.CurrentScene == Scene.PlayerOverview && TutorialPlayerOverviewReferences.Instance.shopUI.activeInHierarchy;
	}

	protected override IEnumerator InitializeTutorialStage() {
		if (Tutorial.Instance.debugLogs) Debug.Log($"TestingTrackTutorial.InitializeTutorialStage()");
		Tutorial.Instance.panel.ShowEscapePanel(false);
		Tutorial.Instance.highlighter.Highlight(null, true); // block raycasts
		yield break;
	}

	protected override bool UpdateTutorialStage() {
		// Handle starting the tutorial scenario
		if (currentStep == Step.NotStarted) {
			currentStep = Step.Started;
			if (Tutorial.Instance.debugLogs) Debug.Log($"TestingTrackTutorial.UpdateTutorialStage(): Starting GoThroughTutorialScenario() as a coroutine.");
			Tutorial.Instance.StartCoroutine(GoThroughTutorialScenario());
		}
		return currentStep != Step.Finished;
	}

	private IEnumerator GoThroughTutorialScenario() {
		if (Tutorial.Instance.debugLogs) Debug.Log($"TestingTrackTutorial.GoThroughTutorialScenario(): Started.");

		// Testing Track button
		currentStep = Step.TestingTrack;
		if (Tutorial.Instance.debugLogs) Debug.Log($"TestingTrackTutorial.GoThroughTutorialScenario(): Current step {currentStep}.");
		Tutorial.Instance.highlighter.Highlight(TutorialPlayerOverviewReferences.Instance.testingTrackButton, true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// End
		currentStep = Step.Finished;
		if (Tutorial.Instance.debugLogs) Debug.Log($"TestingTrackTutorial.GoThroughTutorialScenario(): Current step {currentStep}.");
		if (Tutorial.Instance.debugLogs) Debug.Log($"TestingTrackTutorial.GoThroughTutorialScenario(): Finished.");
	}

}
