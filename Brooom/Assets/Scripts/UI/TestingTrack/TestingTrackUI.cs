using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;


// HUD in Testing Track
// Displaying speed, altitude, spell slots and description of currently selected spell and instructions on how to use spells
public sealed class TestingTrackUI : SimplifiedHUD {

	[SerializeField] TooltipPanel spellDescriptionUI;
	[SerializeField] TooltipPanel spellInstructionsUI;
	[Tooltip("Custom tags and their style which is used to format spell description and spell instructions texts.")]
	[SerializeField] TooltipStyle tooltipStyle;

	private SpellController playerSpellController;
	private TagsMapping tagsMapping;


	#region Currently selected spell description
	private void ShowSpellDescription(int _) { // parameter is there because it is used as a callback on currently selected spell changed
		if (tagsMapping == null) tagsMapping = TooltipController.GetCustomTagsToTMProTagsMapping(tooltipStyle);

		Spell spell = playerSpellController.GetCurrentlySelectedSpell();
		if (spell == null || string.IsNullOrEmpty(spell.Identifier)) {
			if (spellDescriptionUI.gameObject.activeSelf) HideSpellDescription();
			return;
		}
		if (spellDescriptionUI.gameObject.activeSelf) {
			HideSpellDescription();
			Invoke(nameof(SetSpellDescriptionContent), 0.15f);
		} else {
			SetSpellDescriptionContent();
		}
	}

	private void SetSpellDescriptionContent() {
		Spell spell = playerSpellController.GetCurrentlySelectedSpell();
		string keyPrefix = $"Spell{spell.Identifier}";

		spellDescriptionUI.SetContent(new List<string> {
			// Spell name
			TooltipController.ReplaceCustomTagsWithTMProTags(
				$"~~{spell.SpellName}~~",
				tagsMapping),
			// Mana cost
			TooltipController.ReplaceCustomTagsWithTMProTags(
				$"=={LocalizationManager.Instance.GetLocalizedString("SpellInfoMana")}: {spell.ManaCost}==",
				tagsMapping),
			// Spell description
			TooltipController.ReplaceCustomTagsWithTMProTags(
				LocalizationManager.Instance.GetLocalizedString($"{keyPrefix}Description"),
				tagsMapping),
			// Spell target
			TooltipController.ReplaceCustomTagsWithTMProTags(
				$"**{LocalizationManager.Instance.GetLocalizedString("SpellInfoTarget")}:** {LocalizationManager.Instance.GetLocalizedString($"{keyPrefix}Target")}",
				tagsMapping),
			// Cooldown
			TooltipController.ReplaceCustomTagsWithTMProTags(
				$"&&{LocalizationManager.Instance.GetLocalizedString("SpellInfoCooldown")} {spell.Cooldown} s&&",
				tagsMapping)
		});

		spellDescriptionUI.gameObject.TweenAwareEnable();
	}

	private void HideSpellDescription() {
		spellDescriptionUI.gameObject.TweenAwareDisable();
	}
	#endregion

	#region Spell instructions
	private void ShowSpellInstructions() {
		if (tagsMapping == null) tagsMapping = TooltipController.GetCustomTagsToTMProTagsMapping(tooltipStyle);

		spellInstructionsUI.SetContent(new List<string> {
			TooltipController.ReplaceCustomTagsWithTMProTags(
				LocalizationManager.Instance.GetLocalizedString("TestingLabelSpellInstructions"),
				tagsMapping)
		});
		spellInstructionsUI.gameObject.TweenAwareEnable();
	}
	#endregion

	protected override void Start_Derived() {
		// Initialize data fields
		playerSpellController = UtilsMonoBehaviour.FindObjectOfTypeAndTag<SpellController>("Player");
		base.Start_Derived();
		// Initialize spell description
		playerSpellController.onSelectedSpellChanged += ShowSpellDescription;
		if (tooltipStyle == null)
			tooltipStyle = Resources.Load<TooltipStyle>("DefaultTooltipStyle");
		spellDescriptionUI.ChangeAppearance(tooltipStyle);
		spellInstructionsUI.ChangeAppearance(tooltipStyle);
		if (playerSpellController.HasEquippedSpells()) {
			ShowSpellDescription(playerSpellController.selectedSpell);
			ShowSpellInstructions();
		}
	}

	private void OnDestroy() {
		playerSpellController.onSelectedSpellChanged -= ShowSpellDescription;
	}

}