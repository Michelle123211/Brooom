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

	[HideInInspector] public Spell assignedSpell;

	public void Initialize(Spell spell) {
		this.assignedSpell = spell;
		// Set icon
		if (spell == null)
			spellImage.sprite = emptySlotSprite;
		else if (spell.icon == null)
			spellImage.sprite = missingIconSprite;
		else
			spellImage.sprite = spell.icon;
		// Set tooltip contents (if the spell is null, default values are kept; may be used for empty slots)
		if (spell != null) {
			string keyPrefix = "Spell" + spell.identifier;
			tooltip.texts.topLeft = "~~" + LocalizationManager.Instance.GetLocalizedString(keyPrefix + "Name") + "~~";
			tooltip.texts.topRight = "&mana" + LocalizationManager.Instance.GetLocalizedString("SpellInfoMana") + ": " + spell.manaCost.ToString() + "&mana";
			tooltip.texts.mainTop = LocalizationManager.Instance.GetLocalizedString(keyPrefix + "Description");
			tooltip.texts.mainBottom = "**" + LocalizationManager.Instance.GetLocalizedString("SpellInfoTarget") + ":** " + LocalizationManager.Instance.GetLocalizedString(keyPrefix + "Target");
			tooltip.texts.bottomRight = "&cooldown" + LocalizationManager.Instance.GetLocalizedString("SpellInfoCooldown") + " " + spell.cooldown.ToString() + " s&cooldown";
		}

	}
}
