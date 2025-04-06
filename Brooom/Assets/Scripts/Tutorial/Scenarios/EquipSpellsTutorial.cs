using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipSpellsTutorial : TutorialStageBase {

	private enum Step {
		NotStarted,
		Started,
		Slots,
		Selection,
		MoveOn,
		Finished
	}
	private Step currentStep = Step.NotStarted;

	protected override string LocalizationKeyPrefix => "EquipSpells";

	public override void Finish() {
		Tutorial.Instance.FadeIn();
		Tutorial.Instance.panel.HideAllTutorialPanels();
		Tutorial.Instance.highlighter.StopHighlighting();
		// Load Tutorial scene so that the next stage can be started - but only if at least one spell is equipped
		foreach (var equippedSpell in PlayerState.Instance.equippedSpells) {
			if (equippedSpell != null && !string.IsNullOrEmpty(equippedSpell.Identifier)) {
				SceneLoader.Instance.LoadScene(Scene.Tutorial);
				break;
			}
		}
	}

	public override string GetCurrentState() {
		return currentStep.ToString();
	}

	public override void SetCurrentState(string state) {
		// Always starting from the beginning
		currentStep = Step.NotStarted;
	}

	protected override bool CheckTriggerConditions() {
		// Player Overview scene with Shop open + spell purchased
		return SceneLoader.Instance.CurrentScene == Scene.PlayerOverview
			&& TutorialPlayerOverviewReferences.Instance.shopUI.activeInHierarchy
			&& PlayerState.Instance.availableSpellCount == 1;
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
		// Click on equipped spells slot
		currentStep = Step.Slots;
		Tutorial.Instance.highlighter.Highlight(
			TutorialPlayerOverviewReferences.Instance.equippedSpells, false, padding: 10);
		Tutorial.Instance.panel.ShowTutorialPanel(GetLocalizedText(currentStep.ToString()), alignment: TutorialPanelAlignment.Middle);
		yield return new WaitUntil(() => TutorialPlayerOverviewReferences.Instance.spellSelection.activeInHierarchy); // spell selection is displayed (i.e. an equipped spell slot was clicked)
		// Choose spell
		currentStep = Step.Selection;
		Tutorial.Instance.panel.HideTutorialPanel();
		Tutorial.Instance.highlighter.Highlight(null, true);
		yield return new WaitForSecondsRealtime(1); // wait for a second to allow spell selection to fully appear
		Tutorial.Instance.highlighter.Highlight(
			TutorialPlayerOverviewReferences.Instance.GetSpellFromSelection(), false, padding: 5);
		Tutorial.Instance.panel.ShowTutorialPanel(GetLocalizedText(currentStep.ToString()));
		yield return new WaitUntil(() => !TutorialPlayerOverviewReferences.Instance.spellSelection.activeInHierarchy); // spell selection is not displayed (i.e. a spell was assign to an equipped spell slot)
		// Moving on to how to use it
		currentStep = Step.MoveOn;
		Tutorial.Instance.highlighter.Highlight(null, true);
		Tutorial.Instance.FadeOut();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(
			GetLocalizedText(currentStep.ToString()), alignment: TutorialPanelAlignment.Middle);
		// End
		currentStep = Step.Finished;
	}

}
