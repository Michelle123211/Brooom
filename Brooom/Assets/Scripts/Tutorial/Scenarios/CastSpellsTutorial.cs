using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastSpellsTutorial : TutorialStageBase {

	private enum Step {
		NotStarted,
		Started,
		LetsGo, // load scene
		AvailableSpells, // spells in HUD
		Mana, // mana in HUD
		ManaBonus,
		FillMana, // fly around, colect mana bonuses and fill mana bar up
		ManaFull, // mana is full
		Targets, // possible targets
		SpellTarget, // target of the currently selected spell
		CastSpell, // find target, cast spell
		Cooldown, // there is a cooldown
		RechargeBonus,
		CastSpell2, // fly around, collect recharge bonus and cast spell again
		SwitchSpell,
		Finished
	}
	private Step currentStep = Step.NotStarted;

	protected override string LocalizationKeyPrefix => "CastSpells";

	public override void Finish() {
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} finished.");
		Tutorial.Instance.FadeIn();
		Tutorial.Instance.panel.HideAllTutorialPanels();
		Tutorial.Instance.highlighter.StopHighlighting();
		UtilsMonoBehaviour.FindObject<GamePause>()?.ResumeGame();
		if (SceneLoader.Instance.CurrentScene != Scene.PlayerOverview)
			SceneLoader.Instance.LoadScene(Scene.PlayerOverview);
	}

	public override string GetCurrentState() {
		return currentStep.ToString();
	}

	public override void SetCurrentState(string state) {
		// Always starting from the beginning
		currentStep = Step.NotStarted;
	}

	protected override bool CheckTriggerConditions() {
		// Player Overview scene + at least one equipped spell
		if (SceneLoader.Instance.CurrentScene != Scene.PlayerOverview) return false;
		foreach (var equippedSpell in PlayerState.Instance.equippedSpells) {
			if (equippedSpell != null && !string.IsNullOrEmpty(equippedSpell.Identifier)) { // there is an equipped spell
				return true;
			}
		}
		return false;
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
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} started.");

		// Introduction + load scene
		currentStep = Step.LetsGo;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		if (TutorialPlayerOverviewReferences.Instance.shopUI.activeInHierarchy)
			Tutorial.Instance.highlighter.Highlight(TutorialPlayerOverviewReferences.Instance.equippedSpellsShop, true, padding: 10);
		else
			Tutorial.Instance.highlighter.Highlight(TutorialPlayerOverviewReferences.Instance.equippedSpellsOverview, true, padding: 5);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		Tutorial.Instance.panel.HideEscapePanel();
		SceneLoader.Instance.LoadScene(Scene.Tutorial);
		yield return new WaitUntil(() => SceneLoader.Instance.CurrentScene == Scene.Tutorial);

		// Spells in HUD
		currentStep = Step.AvailableSpells;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		Tutorial.Instance.panel.ShowEscapePanel();
		TutorialBasicManager.Instance.DisablePlayerActions();
		Tutorial.Instance.highlighter.Highlight(TutorialSceneManager.Instance.availableSpellsRect, padding: 5);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));

		// Mana bar in HUD
		currentStep = Step.Mana;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		Tutorial.Instance.highlighter.Highlight(TutorialSceneManager.Instance.manaBarRect, padding: 5);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));

		// Mana bonus
		currentStep = Step.ManaBonus;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		Tutorial.Instance.highlighter.StopHighlighting();
		UtilsMonoBehaviour.SetActiveForAll(TutorialSceneManager.Instance.manaBonuses, true);
		TutorialBasicManager.Instance.cutsceneCamera.MoveCameraToLookAt(TutorialSceneManager.Instance.manaBonuses[0].transform, 4, Vector3.forward);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));

		// Collect mana
		currentStep = Step.FillMana;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		TutorialBasicManager.Instance.RotatePlayerTowards(TutorialSceneManager.Instance.manaBonuses[0].transform);
		TutorialBasicManager.Instance.cutsceneCamera.ResetView();
		Tutorial.Instance.panel.ShowTutorialPanel(GetLocalizedText(currentStep.ToString()));
		TutorialBasicManager.Instance.EnablePlayerActions();
		yield return WaitUntilStepIsFinished<ManaProgress>(); // mana is full + at least one bonus was picked up

		// Mana bar full
		currentStep = Step.ManaFull;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		TutorialBasicManager.Instance.DisablePlayerActions();
		yield return new WaitForSecondsRealtime(0.75f); // to let mana tween finish
		PauseGame();
		Tutorial.Instance.highlighter.Highlight(TutorialSceneManager.Instance.spellsAndManaBarRect);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// Spell targets
		currentStep = Step.Targets;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));

		// Current target
		currentStep = Step.SpellTarget;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(
			string.Format(GetLocalizedText(currentStep.ToString()),
			GetCurrentSpellTarget()));

		// Find target + cast spell
		currentStep = Step.CastSpell;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		TutorialSceneManager.Instance.ShowAllPossibleSpellTargets();
		Tutorial.Instance.highlighter.StopHighlighting();
		Tutorial.Instance.panel.ShowTutorialPanel(GetLocalizedText(currentStep.ToString()));
		ResumeGame();
		TutorialBasicManager.Instance.EnablePlayerActions();
		yield return WaitUntilStepIsFinished<SpellCastProgress>();
		yield return new WaitForSecondsRealtime(5f); // to let the player observe the spell effect

		// Cooldown
		currentStep = Step.Cooldown;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		TutorialBasicManager.Instance.DisablePlayerActions();
		Tutorial.Instance.highlighter.Highlight(TutorialSceneManager.Instance.spellsAndManaBarRect);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));

		// Recharge bonus
		currentStep = Step.RechargeBonus;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		Tutorial.Instance.highlighter.StopHighlighting();
		GameObject rechargeBonus = GetSuitableRechargeBonus();
		rechargeBonus.SetActive(true);
		TutorialBasicManager.Instance.cutsceneCamera.MoveCameraToLookAt(rechargeBonus.transform, 4, Vector3.forward);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));

		// Recharge and cast spell
		currentStep = Step.CastSpell2;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		TutorialBasicManager.Instance.RotatePlayerTowards(rechargeBonus.transform);
		TutorialBasicManager.Instance.cutsceneCamera.ResetView();
		Tutorial.Instance.panel.ShowTutorialPanel(GetLocalizedText(currentStep.ToString()));
		TutorialBasicManager.Instance.EnablePlayerActions();
		yield return WaitUntilStepIsFinished<BonusProgress<RechargeSpellsBonusEffect>>(); // bonus picked up
		yield return WaitUntilStepIsFinished<SpellCastProgress>(); // spell cast afterwards

		// Switch spell
		currentStep = Step.SwitchSpell;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		PauseGame();
		TutorialBasicManager.Instance.DisablePlayerActions();
		Tutorial.Instance.FadeOut();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));

		// End
		currentStep = Step.Finished;
		Tutorial.Instance.panel.HideTutorialPanel();
	}

	private string GetCurrentSpellTarget() {
		// Get current spell and its target type
		SpellController spellController = TutorialSceneManager.Instance.player.GetComponentInChildren<SpellController>();
		Spell currentSpell = spellController.GetCurrentlySelectedSpell();
		return LocalizationManager.Instance.GetLocalizedString($"Spell{currentSpell.Identifier}Target");
	}

	private GameObject GetSuitableRechargeBonus() {
		// Choose the one farthest from the player
		float maxDistance = float.MinValue;
		GameObject farthestBonus = null;
		foreach (var bonus in TutorialSceneManager.Instance.rechargeBonuses) {
			float distance = Vector3.Distance(bonus.transform.position, TutorialSceneManager.Instance.player.transform.position);
			if (distance > maxDistance) {
				maxDistance = distance;
				farthestBonus = bonus.gameObject;
			}
		}
		return farthestBonus;
	}

}

internal class ManaProgress : TutorialStepProgressTracker {

	private SpellController spellController;
	private bool manaBonusCollected;

	protected override bool CheckIfPossibleToMoveToNextStep() {
		// Mana is full + at least one bonus was picked up
		return spellController.CurrentMana >= spellController.MaxMana && manaBonusCollected;
	}

	protected override void FinishStepProgress() {
		Messaging.UnregisterFromMessage("BonusPickedUp", OnBonusPickedUp);
	}

	protected override void InitializeStepProgress() {
		spellController = TutorialSceneManager.Instance.player.GetComponentInChildren<SpellController>();
		manaBonusCollected = false;
		Messaging.RegisterForMessage("BonusPickedUp", OnBonusPickedUp);
	}

	protected override void UpdateStepProgress() {
	}

	private void OnBonusPickedUp(GameObject bonus) {
		if (bonus.TryGetComponent<ManaBonusEffect>(out _)) manaBonusCollected = true;
	}
}

internal class SpellCastProgress : TutorialStepProgressTracker {

	private bool spellCast = false;

	protected override bool CheckIfPossibleToMoveToNextStep() {
		return spellCast;
	}

	protected override void FinishStepProgress() {
		Messaging.UnregisterFromMessage("SpellCast", OnSpellCast);
	}

	protected override void InitializeStepProgress() {
		Messaging.RegisterForMessage("SpellCast", OnSpellCast);
	}

	protected override void UpdateStepProgress() {
	}

	private void OnSpellCast(string spellIdentifier) {
		spellCast = true;
	}

}