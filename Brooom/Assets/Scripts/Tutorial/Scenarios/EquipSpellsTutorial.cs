using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// An object representation of the Equip Spells tutorial stage - introducing the player to the concept of equipping spells to slots.
/// It keeps track of the progress within this stage and moves forward from one step to another.
/// </summary>
public class EquipSpellsTutorial : TutorialStageBase {

	// All steps of this tutorial stage
	private enum Step {
		NotStarted,
		Started,
		Slots,
		Selection,
		Equipped,
		Finished
	}
	private Step currentStep = Step.NotStarted;

	/// <inheritdoc/>
	protected override string LocalizationKeyPrefix => "EquipSpells";

	/// <inheritdoc/>
	public override void Finish() {
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} finished.");
		Tutorial.Instance.FadeIn();
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
	/// For this stage to start, the current scene must be PlayerOverview with Shop open
	/// and the player has to have one unlocked spell but no equipped spells.
	/// </summary>
	/// <returns><inheritdoc/></returns>
	protected override bool CheckTriggerConditions() {
		// Player Overview scene with Shop open + one spell purchased + no equipped spell
		if (SceneLoader.Instance.CurrentScene != Scene.PlayerOverview || !TutorialPlayerOverviewReferences.Instance.shopUI.activeInHierarchy) return false;
		if (PlayerState.Instance.availableSpellCount != 1) return false;
		foreach (var equippedSpell in PlayerState.Instance.equippedSpells) {
			if (equippedSpell != null && !string.IsNullOrEmpty(equippedSpell.Identifier)) { // there is an equipped spell
				return false;
			}
		}
		return true;
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

		// Click on equipped spells slot
		currentStep = Step.Slots;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		Tutorial.Instance.highlighter.Highlight(
			TutorialPlayerOverviewReferences.Instance.equippedSpellsShop, true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitUntilVisible(
			GetLocalizedText(currentStep.ToString()), alignment: TutorialPanelAlignment.Middle);
		Tutorial.Instance.highlighter.Highlight(
			TutorialPlayerOverviewReferences.Instance.equippedSpellsShop, false, padding: 10);
		yield return new WaitUntil(() => TutorialPlayerOverviewReferences.Instance.spellSelection.activeInHierarchy); // spell selection is displayed (i.e. an equipped spell slot was clicked)
		
		// Choose spell
		currentStep = Step.Selection;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		Tutorial.Instance.panel.HideTutorialPanel();
		Tutorial.Instance.highlighter.Highlight(null, true);
		yield return new WaitForSecondsRealtime(1); // wait for a second to allow spell selection to fully appear
		Tutorial.Instance.highlighter.Highlight(
			TutorialPlayerOverviewReferences.Instance.GetSpellFromSelection(), false, padding: 5);
		Tutorial.Instance.panel.ShowTutorialPanel(GetLocalizedText(currentStep.ToString()));
		yield return new WaitUntil(() => !TutorialPlayerOverviewReferences.Instance.spellSelection.activeInHierarchy); // spell selection is not displayed (i.e. a spell was assign to an equipped spell slot)
		
		// Show equipped spell
		currentStep = Step.Equipped;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		Tutorial.Instance.highlighter.Highlight(
			TutorialPlayerOverviewReferences.Instance.equippedSpellsShop, true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(
			GetLocalizedText(currentStep.ToString()), alignment: TutorialPanelAlignment.Middle);
		
		// End
		currentStep = Step.Finished;
	}

}
