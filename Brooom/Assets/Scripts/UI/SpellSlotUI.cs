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
		bool isEmpty = spell == null || string.IsNullOrEmpty(spell.identifier);
		// Set icon
		if (isEmpty)
			spellImage.sprite = emptySlotSprite;
		else if (spell.icon == null)
			spellImage.sprite = missingIconSprite;
		else
			spellImage.sprite = spell.icon;
		// Set tooltip contents
		if (!isEmpty) {
			string keyPrefix = "Spell" + spell.identifier;
			tooltip.texts.topLeft = "~~" + LocalizationManager.Instance.GetLocalizedString(keyPrefix + "Name") + "~~";
			tooltip.texts.topRight = "==" + LocalizationManager.Instance.GetLocalizedString("SpellInfoMana") + ": " + spell.manaCost.ToString() + "==";
			tooltip.texts.mainTop = LocalizationManager.Instance.GetLocalizedString(keyPrefix + "Description");
			tooltip.texts.mainBottom = "**" + LocalizationManager.Instance.GetLocalizedString("SpellInfoTarget") + ":** " + LocalizationManager.Instance.GetLocalizedString(keyPrefix + "Target");
			tooltip.texts.bottomRight = "&&" + LocalizationManager.Instance.GetLocalizedString("SpellInfoCooldown") + " " + spell.cooldown.ToString() + " s&&";
			tooltip.isLocalized = false;
		} else { // if the spell is null, use default values
			tooltip.texts = defaultText;
			tooltip.isLocalized = defaultIsLocalized;
		}

	}
}
