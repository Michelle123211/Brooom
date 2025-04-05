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

	private TutorialPlayerOverviewReferences objectReferences;

	private int cheapestItemPrice = int.MaxValue; // store price of the cheapest item (to be able to determine if the player has enough coins to buy something)

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
		// Player Overview scene + enough coins to buy first item in a shop
		return SceneLoader.Instance.CurrentScene == Scene.PlayerOverview && HasEnoughCoinsToBuySomething();
	}

	protected override IEnumerator InitializeTutorialStage() {
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
		// Shop button
		currentStep = Step.Shop;
		Tutorial.Instance.highlighter.Highlight(objectReferences.shopButton, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitUntilVisible(GetLocalizedText(currentStep.ToString()));
		yield return WaitUntilStepIsFinished<ShopButtonProgress>();
		// Spells
		currentStep = Step.Spells; Tutorial.Instance.highlighter.Highlight(objectReferences.spells, true, padding: 10);
		yield return Tutorial.Instance.panel.ShowTutorialPanelAndWaitForClick(GetLocalizedText(currentStep.ToString()));
		// Broom upgrades
		currentStep = Step.BroomUpgrades; Tutorial.Instance.highlighter.Highlight(objectReferences.broomUpgrades, true, padding: 10);
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
		if (objectReferences == null)
			objectReferences = UtilsMonoBehaviour.FindObject<TutorialPlayerOverviewReferences>();
		foreach (var upgrade in objectReferences.broom.GetAvailableUpgrades()) {
			if (upgrade.CoinsCostOfEachLevel[0] < cheapestItemPrice) // check only the first level
				cheapestItemPrice = upgrade.CoinsCostOfEachLevel[0];
		}
	}

}

internal class ShopButtonProgress : TutorialStepProgressTracker {

	private ShopUI shopUI;

	protected override bool CheckIfPossibleToMoveToNextStep() {
		// Show has been opened
		return shopUI.gameObject.activeInHierarchy;
	}

	protected override void FinishStepProgress() {
	}

	protected override void InitializeStepProgress() {
		shopUI = UtilsMonoBehaviour.FindObject<ShopUI>();
	}

	protected override void UpdateStepProgress() {
	}
}
