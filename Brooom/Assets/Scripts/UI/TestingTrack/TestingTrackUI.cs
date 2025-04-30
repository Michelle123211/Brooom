using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;


/// <summary>
/// A component representing HUD in the testing track.
/// It extends the <c>SimplifiedHUD</c> class to add functionality for showing description of the currently selected spell
/// and instructions on how to use spells, on top of the already included parts (i.e. current speed, current altitude, equipped spell slots).
/// </summary>
public sealed class TestingTrackUI : SimplifiedHUD {

	[Tooltip("Panel displaying description of the currently selected spell.")]
	[SerializeField] TooltipPanel spellDescriptionUI;
	[Tooltip("Panel displaying instructions on how to use spells.")]
	[SerializeField] TooltipPanel spellInstructionsUI;
	[Tooltip("Custom tags and their style which is used to format spell description and spell instructions texts.")]
	[SerializeField] TooltipStyle tooltipStyle;

	private SpellController playerSpellController;
	private TagsMapping tagsMapping; // tags which can be used to format text in panels (it is initialized from tooltipStyle)


	#region Currently selected spell description
	// Shows description of the currently selected spell (or hides it if no spell is selected) - called whenever the selected spell changed
	private void ShowSpellDescription(int _) { // parameter is there because it is used as a callback on currently selected spell changed
		if (tagsMapping == null) tagsMapping = TooltipController.GetCustomTagsToTMProTagsMapping(tooltipStyle);

		Spell spell = playerSpellController.GetCurrentlySelectedSpell();
		if (spell == null || string.IsNullOrEmpty(spell.Identifier)) {
			if (spellDescriptionUI.gameObject.activeSelf) HideSpellDescription();
			return;
		}
		if (spellDescriptionUI.gameObject.activeSelf) {
			// Spell description is already visible - hide it first, then set its content and show it
			HideSpellDescription();
			Invoke(nameof(SetSpellDescriptionContent), 0.15f);
		} else {
			// Set the content and show it
			SetSpellDescriptionContent();
		}
	}

	// Shows the current spell's description in a panel
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

	// Hides panel displaying the current spell's description
	private void HideSpellDescription() {
		spellDescriptionUI.gameObject.TweenAwareDisable();
	}
	#endregion

	#region Spell instructions
	// Shows instructions on how to use spells
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

	/// <summary>
	/// Initializes all data fields and UI elements.
	/// </summary>
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