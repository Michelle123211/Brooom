using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellSlotUI : MonoBehaviour
{
    [Tooltip("An Image used to display icon of the spell.")]
    [SerializeField] Image spellImage;
	[Tooltip("A Sprite which is used when the slot is empty (= the assigned spell is null).")]
	[SerializeField] Sprite emptySlotSprite;
	[Tooltip("A Sprite which is used when the spell does not have assigned icon.")]
	[SerializeField] Sprite missingIconSprite;

	[Tooltip("A Tooltip component displaying information about the assigned spell after hovering the mouse.")]
	[SerializeField] Tooltip tooltip;
	TooltipSectionsText defaultText;
	bool defaultIsLocalized;


	[HideInInspector] public Spell assignedSpell;

	private void Awake() {
		// Store default tooltip values (so they can be used later when null is assigned)
		defaultText = tooltip.texts;
		defaultIsLocalized = tooltip.isLocalized;
	}

	public void Initialize(Spell spell) {
		this.assignedSpell = spell;
		bool isEmpty = spell == null || string.IsNullOrEmpty(spell.Identifier);
		// Set icon
		if (isEmpty)
			spellImage.sprite = emptySlotSprite;
		else if (spell.Icon == null)
			spellImage.sprite = missingIconSprite;
		else
			spellImage.sprite = spell.Icon;
		// Set tooltip contents
		if (!isEmpty) {
			string keyPrefix = $"Spell{spell.Identifier}";
			tooltip.texts.topLeft = $"~~{spell.SpellName}~~";
			tooltip.texts.topRight = $"=={LocalizationManager.Instance.GetLocalizedString("SpellInfoMana")}: {spell.ManaCost}==";
			tooltip.texts.mainTop = LocalizationManager.Instance.GetLocalizedString($"{keyPrefix}Description");
			tooltip.texts.mainBottom = $"**{LocalizationManager.Instance.GetLocalizedString("SpellInfoTarget")}:** {LocalizationManager.Instance.GetLocalizedString($"{keyPrefix}Target")}";
			tooltip.texts.bottomRight = $"&&{LocalizationManager.Instance.GetLocalizedString("SpellInfoCooldown")} {spell.Cooldown} s&&";
			tooltip.isLocalized = false;
		} else { // if the spell is null, use default values
			tooltip.texts = defaultText;
			tooltip.isLocalized = defaultIsLocalized;
		}

	}
}
