using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOverviewTutorial : TutorialStageBase {

	private enum Step {
		NotStarted,
		Started,
		Overview,
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
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} finished.");
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
		// Player Overview scene
		return SceneLoader.Instance.CurrentScene == Scene.PlayerOverview;
	}

	protected override IEnumerator InitializeTutorialStage() {
		Tutorial.Instance.panel.ShowEscapePanel(false);
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
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} started.");

		// Overview introduction
		currentStep = Step.Overview;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		Tutorial.Instance.FadeOut();
		Tutorial.Instance.highlighter.Highlight(null, true); // don't highlight anything, but block raycasts
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// Coins
		currentStep = Step.Coins;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		Tutorial.Instance.FadeIn();
		Tutorial.Instance.highlighter.Highlight(
			TutorialPlayerOverviewReferences.Instance.coins, true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// Leaderboard
		currentStep = Step.Leaderboard;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		Tutorial.Instance.highlighter.Highlight(
			TutorialPlayerOverviewReferences.Instance.leaderboard, true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// Stats graph
		currentStep = Step.Stats;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		Tutorial.Instance.highlighter.Highlight(
			TutorialPlayerOverviewReferences.Instance.graph, true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// Endurance stat
		currentStep = Step.Endurance;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		string statName = LocalizationManager.Instance.GetLocalizedString("PlayerStatEndurance");
		Tutorial.Instance.highlighter.Highlight(
			TutorialPlayerOverviewReferences.Instance.GetGraphLabel(statName), true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(
			string.Format(GetLocalizedText(currentStep.ToString()),
				statName));
		
		// Speed stat
		currentStep = Step.Speed;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		statName = LocalizationManager.Instance.GetLocalizedString("PlayerStatSpeed");
		Tutorial.Instance.highlighter.Highlight(
			TutorialPlayerOverviewReferences.Instance.GetGraphLabel(statName), true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(
			string.Format(GetLocalizedText(currentStep.ToString()),
				statName));
		
		// Dexterity stat
		currentStep = Step.Dexterity;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		statName = LocalizationManager.Instance.GetLocalizedString("PlayerStatDexterity");
		Tutorial.Instance.highlighter.Highlight(
			TutorialPlayerOverviewReferences.Instance.GetGraphLabel(statName), true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(
			string.Format(GetLocalizedText(currentStep.ToString()),
				statName));
		
		// Precision stat
		currentStep = Step.Precision;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		statName = LocalizationManager.Instance.GetLocalizedString("PlayerStatPrecision");
		Tutorial.Instance.highlighter.Highlight(
			TutorialPlayerOverviewReferences.Instance.GetGraphLabel(statName), true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(
			string.Format(GetLocalizedText(currentStep.ToString()),
				statName));
		
		// Magic stat
		currentStep = Step.Magic;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		statName = LocalizationManager.Instance.GetLocalizedString("PlayerStatMagic");
		Tutorial.Instance.highlighter.Highlight(
			TutorialPlayerOverviewReferences.Instance.GetGraphLabel(statName), true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(
			string.Format(GetLocalizedText(currentStep.ToString()),
				statName));
		
		// End
		currentStep = Step.Finished;
	}

}
