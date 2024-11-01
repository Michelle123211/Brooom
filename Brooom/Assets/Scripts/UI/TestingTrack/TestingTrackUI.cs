using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class TestingTrackUI : MonoBehaviour {

	[Header("Basic info")]
	[SerializeField] TextMeshProUGUI speedText;
	[SerializeField] TextMeshProUGUI altitudeText;

	[Header("Spells")]
	[SerializeField] RaceSpellsUI spellsUI; // TODO: Delete, only temporary
	[SerializeField] TooltipPanel spellDescriptionUI;
	[Tooltip("Custom tags and their style which is used to format spell description text.")]
	[SerializeField] TooltipStyle tooltipStyle;

	private CharacterMovementController playerMovementController;
	private SpellController playerSpellController;

	private TagsMapping tagsMapping;

	public void ReturnBack() {
        SceneLoader.Instance.LoadScene(Scene.PlayerOverview);
	}

	#region Speed and altitude
	public void UpdatePlayerState() {
		speedText.text = Math.Round(playerMovementController.GetCurrentSpeed(), 1).ToString();
		altitudeText.text = Math.Round(playerMovementController.GetCurrentAltitude(), 1).ToString();
	}
	#endregion

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

	private void Start() {
		// Initialize data fields
		playerMovementController = UtilsMonoBehaviour.FindObjectOfTypeAndTag<CharacterMovementController>("Player");
		playerSpellController = UtilsMonoBehaviour.FindObjectOfTypeAndTag<SpellController>("Player");
		// Initialize spell description
		playerSpellController.onSelectedSpellChanged += ShowSpellDescription;
		if (tooltipStyle == null)
			tooltipStyle = Resources.Load<TooltipStyle>("DefaultTooltipStyle");
		spellDescriptionUI.ChangeAppearance(tooltipStyle);
		if (playerSpellController.HasEquippedSpells())
			ShowSpellDescription(playerSpellController.selectedSpell);
		// Initialize and show spells UI
		spellsUI.Initialize(playerMovementController.gameObject);
	}

	private void OnDestroy() {
		playerSpellController.onSelectedSpellChanged -= ShowSpellDescription;
	}

	private void Update() {
		if (InputManager.Instance.GetBoolValue("Pause"))
			ReturnBack();
		UpdatePlayerState();
	}

}
