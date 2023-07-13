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
	}
}
