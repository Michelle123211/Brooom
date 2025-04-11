using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopTutorial : TutorialStageBase {

	private enum Step {
		NotStarted,
		Started,
		Shop, // button to enter shop
		Spells,
		BroomUpgrades,
		Finished
	}
	private Step currentStep = Step.NotStarted;

	protected override string LocalizationKeyPrefix => "Shop";

	private int cheapestItemPrice = int.MaxValue; // store price of the cheapest item (to be able to determine if the player has enough coins to buy something)

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
		// Player Overview scene + enough coins to buy first item in a shop
		return SceneLoader.Instance.CurrentScene == Scene.PlayerOverview && HasEnoughCoinsToBuySomething();
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

		// Shop button
		currentStep = Step.Shop;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		Tutorial.Instance.highlighter.Highlight(
			TutorialPlayerOverviewReferences.Instance.shopButton, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitUntilVisible(GetLocalizedText(currentStep.ToString()));
		yield return new WaitUntil(() => TutorialPlayerOverviewReferences.Instance.shopUI.activeInHierarchy); // Shop has been opened
		
		// Spells
		currentStep = Step.Spells;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		Tutorial.Instance.highlighter.Highlight(
			TutorialPlayerOverviewReferences.Instance.spells, true, padding: 10);
		yield return Tutorial.Instance.panel.HideTutorialPanelAndWaitUntilInvisible();
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// Broom upgrades
		currentStep = Step.BroomUpgrades;
		Analytics.Instance.LogEvent(AnalyticsCategory.Tutorial, $"Tutorial stage {LocalizationKeyPrefix} moved to step {currentStep}.");
		Tutorial.Instance.highlighter.Highlight(
			TutorialPlayerOverviewReferences.Instance.broomUpgrades, true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		
		// End
		currentStep = Step.Finished;
	}

	private bool HasEnoughCoinsToBuySomething() {
		if (cheapestItemPrice == int.MaxValue) InitializeData();
		return PlayerState.Instance.Coins >= cheapestItemPrice;
	}

	private void InitializeData() {
		// Find the cheapest item in the shop
		cheapestItemPrice = int.MaxValue;
		// Go through all spells
		foreach (var spell in SpellManager.Instance.AllSpells) {
			if (spell.CoinsCost < cheapestItemPrice)
				cheapestItemPrice = spell.CoinsCost;
		}
		// Go through all broom upgrades
		foreach (var upgrade in TutorialPlayerOverviewReferences.Instance.broom.GetAvailableUpgrades()) {
			if (upgrade.CoinsCostOfEachLevel[0] < cheapestItemPrice) // check only the first level
				cheapestItemPrice = upgrade.CoinsCostOfEachLevel[0];
		}
	}

}
