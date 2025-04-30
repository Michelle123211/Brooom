using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// An object representation of the Testing Track tutorial stage - introducing the player to the option to enter testing track.
/// It keeps track of the progress within this stage and moves forward from one step to another.
/// </summary>
public class TestingTrackTutorial : TutorialStageBase {

	// All steps of this tutorial stage
	private enum Step {
		NotStarted,
		Started,
		TestingTrack, // button to enter testing track
		Finished
	}
	private Step currentStep = Step.NotStarted;

	/// <inheritdoc/>
	protected override string LocalizationKeyPrefix => "TestingTrack";

	/// <inheritdoc/>
	public override void Finish() {
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} finished.");
		Tutorial.Instance.panel.HideAllTutorialPanels();
		Tutorial.Instance.highlighter.StopHighlighting();
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
	/// For this stage to start, the current scene must be PlayerOverview with the Shop open.
	/// </summary>
	/// <returns><inheritdoc/></returns>
	protected override bool CheckTriggerConditions() {
		// Player Overview scene with Shop open
		return SceneLoader.Instance.CurrentScene == Scene.PlayerOverview && TutorialPlayerOverviewReferences.Instance.shopUI.activeInHierarchy;
	}

	/// <inheritdoc/>
	protected override IEnumerator InitializeTutorialStage() {
		Tutorial.Instance.panel.ShowEscapePanel(false);
		Tutorial.Instance.highlighter.Highlight(null, true); // block raycasts
		yield break;
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

		// Testing Track button
		currentStep = Step.TestingTrack;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		Tutorial.Instance.highlighter.Highlight(TutorialPlayerOverviewReferences.Instance.testingTrackButton, true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// End
		currentStep = Step.Finished;
	}

}
