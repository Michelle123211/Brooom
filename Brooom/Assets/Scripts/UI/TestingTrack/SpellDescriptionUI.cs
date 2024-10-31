using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpellDescriptionUI : MonoBehaviour {

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
		spellNameLabel.text = $"{spell.SpellName}";
		manaCostLabel.text = $"{LocalizationManager.Instance.GetLocalizedString("SpellInfoMana")}: {spell.ManaCost}";
		spellDescriptionLabel.text = LocalizationManager.Instance.GetLocalizedString($"{keyPrefix}Description");
		targetLabel.text = $"{LocalizationManager.Instance.GetLocalizedString("SpellInfoTarget")}: {LocalizationManager.Instance.GetLocalizedString($"{keyPrefix}Target")}";
		cooldownLabel.text = $"{LocalizationManager.Instance.GetLocalizedString("SpellInfoCooldown")} {spell.Cooldown} s";
		gameObject.TweenAwareEnable();
	}

}
