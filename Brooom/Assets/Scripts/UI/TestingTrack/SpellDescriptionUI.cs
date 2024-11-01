using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpellDescriptionUI : MonoBehaviour {

	[Header("Text")]
	[Tooltip("Default color of the text (when no custom tags are applied).")]
	[SerializeField] Color textColor = Color.white;
	[Tooltip("Font used for all the tooltip text.")]
	[SerializeField] TMP_FontAsset font;

	[Header("Colors")] // TODO: take colors from a color palette
	[SerializeField] Color spellNameColor;
	[SerializeField] Color manaColor;
	[SerializeField] Color cooldownColor;

	[Header("UI elements")]
	[SerializeField] TextMeshProUGUI spellNameLabel;
	[SerializeField] TextMeshProUGUI manaCostLabel;
	[SerializeField] TextMeshProUGUI spellDescriptionLabel;
	[SerializeField] TextMeshProUGUI targetLabel;
	[SerializeField] TextMeshProUGUI cooldownLabel;

	private Spell spell;

	public void ShowSpellDescription(Spell spell) {
		this.spell = spell;
		if (spell == null || string.IsNullOrEmpty(spell.Identifier)) { 
			if (gameObject.activeSelf) HideSpellDescription();
			return;
		}
		if (gameObject.activeSelf) {
			HideSpellDescription();
			Invoke(nameof(SetSpellDescriptionContent), 0.15f);
		} else {
			SetSpellDescriptionContent();
		}
	}

	public void HideSpellDescription() {
		gameObject.TweenAwareDisable();
	}

	private void SetSpellDescriptionContent() {
		string keyPrefix = $"Spell{spell.Identifier}";
		// Use TMPro tags to format the text
		spellNameLabel.text = $"<color={spellNameColor.ToHex()}><b>{spell.SpellName}</b></color>"; // color and bold
		manaCostLabel.text = $"<color={manaColor.ToHex()}><b>{LocalizationManager.Instance.GetLocalizedString("SpellInfoMana")}: {spell.ManaCost}</b></color>"; // color and bold
		spellDescriptionLabel.text = LocalizationManager.Instance.GetLocalizedString($"{keyPrefix}Description");
		targetLabel.text = $"<b>{LocalizationManager.Instance.GetLocalizedString("SpellInfoTarget")}:</b> {LocalizationManager.Instance.GetLocalizedString($"{keyPrefix}Target")}"; // bold
		cooldownLabel.text = $"<color={cooldownColor.ToHex()}><b>{LocalizationManager.Instance.GetLocalizedString("SpellInfoCooldown")} {spell.Cooldown} s</b></color>"; // color and bold
		gameObject.TweenAwareEnable();
	}

	private void Start() {
		// Set default font and text color
		spellNameLabel.font = font;
		spellNameLabel.color = textColor;
		manaCostLabel.font = font;
		manaCostLabel.color = textColor;
		spellDescriptionLabel.font = font;
		spellDescriptionLabel.color = textColor;
		targetLabel.font = font;
		targetLabel.color = textColor;
		cooldownLabel.font = font;
		cooldownLabel.color = textColor;
	}

}
