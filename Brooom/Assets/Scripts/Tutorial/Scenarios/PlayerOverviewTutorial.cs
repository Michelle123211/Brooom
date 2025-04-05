using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOverviewTutorial : TutorialStageBase {

	private enum Step {
		NotStarted,
		Started,
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

	private TutorialPlayerOverviewReferences objectReferences;

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
		// Player Overview scene
		return SceneLoader.Instance.CurrentScene == Scene.PlayerOverview;
	}

	protected override IEnumerator InitializeTutorialStage() {
		// TODO: Set initial state and prepare everything necessary
		Tutorial.Instance.panel.ShowEscapePanel(false);
		objectReferences = UtilsMonoBehaviour.FindObject<TutorialPlayerOverviewReferences>();
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
		// Coins
		currentStep = Step.Coins;
		Tutorial.Instance.highlighter.Highlight(objectReferences.coins, true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		// Leaderboard
		currentStep = Step.Leaderboard;
		Tutorial.Instance.highlighter.Highlight(objectReferences.leaderboard, true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		// Stats graph
		currentStep = Step.Stats;
		Tutorial.Instance.highlighter.Highlight(objectReferences.graph, true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		// Endurance stat
		currentStep = Step.Endurance;
		string statName = LocalizationManager.Instance.GetLocalizedString("PlayerStatEndurance");
		Tutorial.Instance.highlighter.Highlight(objectReferences.GetGraphLabel(statName), true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(
			string.Format(GetLocalizedText(currentStep.ToString()),
				statName));
		// Speed stat
		currentStep = Step.Speed;
		statName = LocalizationManager.Instance.GetLocalizedString("PlayerStatSpeed");
		Tutorial.Instance.highlighter.Highlight(objectReferences.GetGraphLabel(statName), true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(
			string.Format(GetLocalizedText(currentStep.ToString()),
				statName));
		// Dexterity stat
		currentStep = Step.Dexterity;
		statName = LocalizationManager.Instance.GetLocalizedString("PlayerStatDexterity");
		Tutorial.Instance.highlighter.Highlight(objectReferences.GetGraphLabel(statName), true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(
			string.Format(GetLocalizedText(currentStep.ToString()),
				statName));
		// Precision stat
		currentStep = Step.Precision;
		statName = LocalizationManager.Instance.GetLocalizedString("PlayerStatPrecision");
		Tutorial.Instance.highlighter.Highlight(objectReferences.GetGraphLabel(statName), true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(
			string.Format(GetLocalizedText(currentStep.ToString()),
				statName));
		// Magic stat
		currentStep = Step.Magic;
		statName = LocalizationManager.Instance.GetLocalizedString("PlayerStatMagic");
		Tutorial.Instance.highlighter.Highlight(objectReferences.GetGraphLabel(statName), true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(
			string.Format(GetLocalizedText(currentStep.ToString()),
				statName));
		// End
		currentStep = Step.Finished;
	}

}
