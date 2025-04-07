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
		return SceneLoader.Instance.CurrentScene == Scene.PlayerOverview && TutorialPlayerOverviewReferences.Instance.shopUI.activeInHierarchy;
	}

	protected override IEnumerator InitializeTutorialStage() {
		Tutorial.Instance.panel.ShowEscapePanel(false);
		Tutorial.Instance.highlighter.Highlight(null, true); // block raycasts
		yield break;
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
		// Testing Track button
		currentStep = Step.TestingTrack;
		Tutorial.Instance.highlighter.Highlight(TutorialPlayerOverviewReferences.Instance.testingTrackButton, true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		// End
		currentStep = Step.Finished;
	}

}
