using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellSlotUI : MonoBehaviour
{
	[Header("UI components")]
    [Tooltip("An Image used to display icon of the spell.")]
    [SerializeField] Image spellImage;
	[Tooltip("An Image used as a background of the spell icon, is coloured according to the spell category.")]
	[SerializeField] Image spellBackground;
	[Tooltip("An Image used as a slot border.")]
	[SerializeField] Image spellBorder;

	[Header("Sprites")]
	[Tooltip("A Sprite which is used as border when the slot is not empty.")]
	[SerializeField] Sprite slotBorderSprite;
	[Tooltip("A sprite which is used as a border when the slot is empty.")]
	[SerializeField] Sprite emptySlotBorderSprite;
	[Tooltip("A Sprite which is used when the slot is empty (= the assigned spell is null).")]
	[SerializeField] Sprite emptySlotSprite;
	[Tooltip("Color which is used for the empty slot icon.")]
	[SerializeField] Color emptySlotColor;
	[Tooltip("A Sprite which is used when the spell does not have assigned icon.")]
	[SerializeField] Sprite missingIconSprite;

	[Tooltip("A Tooltip component displaying information about the assigned spell after hovering the mouse.")]
	[SerializeField] Tooltip tooltip;
	TooltipSectionsText? defaultText = null;
	bool defaultIsLocalized;


	[HideInInspector] public Spell assignedSpell;
	private bool isEmpty = true;

	public void Initialize(Spell spell) {
		this.assignedSpell = spell;
		this.isEmpty = spell == null || string.IsNullOrEmpty(spell.Identifier);
		SetIcon();
		SetTooltipContent();
	}

	private void SetIcon() {
		// Spell icon
		spellImage.color = Color.white;
		if (isEmpty) {
			spellImage.sprite = emptySlotSprite;
			spellImage.color = emptySlotColor;
		} else if (assignedSpell.Icon == null)
			spellImage.sprite = missingIconSprite;
		else
			spellImage.sprite = assignedSpell.Icon;
		// Background color
		if (!isEmpty) {
			spellBackground.color = ColorPalette.Instance.GetColor(assignedSpell.Category switch {
				SpellCategory.SelfCast => ColorFromPalette.Spells_BackgroundSelfCastSpell,
				SpellCategory.OpponentCurse => ColorFromPalette.Spells_BackgroundOpponentCurse,
				SpellCategory.EnvironmentManipulation => ColorFromPalette.Spells_BackgroundEnvironmentManipulation,
				SpellCategory.ObjectApparition => ColorFromPalette.Spells_BackgroundObjectApparition,
				_ => ColorFromPalette.None
			});
		} else {
			// For empty slot, make it semi-transparent
			spellBackground.color = Color.white.WithA(0.1f);
		}
		// Border
		if (isEmpty) {
			spellBorder.sprite = emptySlotBorderSprite;
			spellBorder.color = emptySlotColor;
		} else {
			spellBorder.sprite = slotBorderSprite;
			spellBorder.color = Color.black;
		}
	}

	private void SetTooltipContent() {
		// Store default tooltip values (so they can be used later when null is assigned)
		if (!defaultText.HasValue) {
			defaultText = tooltip.texts;
			defaultIsLocalized = tooltip.isLocalized;
		}
		// Set tooltip
		if (!isEmpty) {
			string keyPrefix = $"Spell{assignedSpell.Identifier}";
			tooltip.texts.topLeft = $"~~{assignedSpell.SpellName}~~";
			tooltip.texts.topRight = $"=={LocalizationManager.Instance.GetLocalizedString("SpellInfoMana")}: {assignedSpell.ManaCost}==";
			tooltip.texts.mainTop = LocalizationManager.Instance.GetLocalizedString($"{keyPrefix}Description");
			tooltip.texts.mainBottom = $"**{LocalizationManager.Instance.GetLocalizedString("SpellInfoTarget")}:** {LocalizationManager.Instance.GetLocalizedString($"{keyPrefix}Target")}";
			tooltip.texts.bottomRight = $"&&{LocalizationManager.Instance.GetLocalizedString("SpellInfoCooldown")} {assignedSpell.Cooldown} s&&";
			tooltip.isLocalized = false;
		} else { // if the spell is null, use default values
			tooltip.texts = defaultText.Value;
			tooltip.isLocalized = defaultIsLocalized;
		}
	}

}
