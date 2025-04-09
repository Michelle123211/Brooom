using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipSpellsTutorial : TutorialStageBase {

	private enum Step {
		NotStarted,
		Started,
		Slots,
		Selection,
		Equipped,
		Finished
	}
	private Step currentStep = Step.NotStarted;

	protected override string LocalizationKeyPrefix => "EquipSpells";

	public override void Finish() {
		if (Tutorial.Instance.debugLogs) Debug.Log($"EquipSpellsTutorial.Finish()");
		Tutorial.Instance.FadeIn();
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
		// Player Overview scene with Shop open + one spell purchased + no equipped spell
		if (SceneLoader.Instance.CurrentScene != Scene.PlayerOverview || !TutorialPlayerOverviewReferences.Instance.shopUI.activeInHierarchy) return false;
		if (PlayerState.Instance.availableSpellCount != 1) return false;
		foreach (var equippedSpell in PlayerState.Instance.equippedSpells) {
			if (equippedSpell != null && !string.IsNullOrEmpty(equippedSpell.Identifier)) { // there is an equipped spell
				return false;
			}
		}
		if (Tutorial.Instance.debugLogs) Debug.Log($"EquipSpellsTutorial.CheckTriggerConditions(): Conditions satisfied, scene is {SceneLoader.Instance.CurrentScene}, shop visible is {TutorialPlayerOverviewReferences.Instance.shopUI.activeInHierarchy} and no spell should be equipped.");
		return true;
	}

	protected override IEnumerator InitializeTutorialStage() {
		if (Tutorial.Instance.debugLogs) Debug.Log($"EquipSpellsTutorial.InitializeTutorialStage()");
		Tutorial.Instance.panel.ShowEscapePanel(false);
		Tutorial.Instance.highlighter.Highlight(null, true); // block raycasts
		yield break;
	}

	protected override bool UpdateTutorialStage() {
		// Handle starting the tutorial scenario
		if (currentStep == Step.NotStarted) {
			currentStep = Step.Started;
			if (Tutorial.Instance.debugLogs) Debug.Log($"EquipSpellsTutorial.UpdateTutorialStage(): Starting GoThroughTutorialScenario() as a coroutine.");
			Tutorial.Instance.StartCoroutine(GoThroughTutorialScenario());
		}
		return currentStep != Step.Finished;
	}

	private IEnumerator GoThroughTutorialScenario() {
		if (Tutorial.Instance.debugLogs) Debug.Log($"EquipSpellsTutorial.GoThroughTutorialScenario(): Started.");
		// Click on equipped spells slot
		currentStep = Step.Slots;
		if (Tutorial.Instance.debugLogs) Debug.Log($"EquipSpellsTutorial.GoThroughTutorialScenario(): Current step {currentStep}.");
		Tutorial.Instance.highlighter.Highlight(
			TutorialPlayerOverviewReferences.Instance.equippedSpellsShop, true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitUntilVisible(
			GetLocalizedText(currentStep.ToString()), alignment: TutorialPanelAlignment.Middle);
		Tutorial.Instance.highlighter.Highlight(
			TutorialPlayerOverviewReferences.Instance.equippedSpellsShop, false, padding: 10);
		yield return new WaitUntil(() => TutorialPlayerOverviewReferences.Instance.spellSelection.activeInHierarchy); // spell selection is displayed (i.e. an equipped spell slot was clicked)
		
		// Choose spell
		currentStep = Step.Selection;
		if (Tutorial.Instance.debugLogs) Debug.Log($"EquipSpellsTutorial.GoThroughTutorialScenario(): Current step {currentStep}.");
		Tutorial.Instance.panel.HideTutorialPanel();
		Tutorial.Instance.highlighter.Highlight(null, true);
		yield return new WaitForSecondsRealtime(1); // wait for a second to allow spell selection to fully appear
		Tutorial.Instance.highlighter.Highlight(
			TutorialPlayerOverviewReferences.Instance.GetSpellFromSelection(), false, padding: 5);
		Tutorial.Instance.panel.ShowTutorialPanel(GetLocalizedText(currentStep.ToString()));
		yield return new WaitUntil(() => !TutorialPlayerOverviewReferences.Instance.spellSelection.activeInHierarchy); // spell selection is not displayed (i.e. a spell was assign to an equipped spell slot)
		
		// Show equipped spell
		currentStep = Step.Equipped;
		if (Tutorial.Instance.debugLogs) Debug.Log($"EquipSpellsTutorial.GoThroughTutorialScenario(): Current step {currentStep}.");
		Tutorial.Instance.highlighter.Highlight(
			TutorialPlayerOverviewReferences.Instance.equippedSpellsShop, true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(
			GetLocalizedText(currentStep.ToString()), alignment: TutorialPanelAlignment.Middle);
		
		// End
		currentStep = Step.Finished;
		if (Tutorial.Instance.debugLogs) Debug.Log($"EquipSpellsTutorial.GoThroughTutorialScenario(): Current step {currentStep}.");
		if (Tutorial.Instance.debugLogs) Debug.Log($"EquipSpellsTutorial.GoThroughTutorialScenario(): Finished.");
	}

}
