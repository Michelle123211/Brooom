using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquippedSpellSlotUI : MonoBehaviour
{
	[Tooltip("An Image which is used to display the assigned spell icon.")]
	[SerializeField] Image spellImage;
	[Tooltip("A Sprite which is used in an ampty slot instead of a spell icon.")]
	[SerializeField] Sprite emptySlotSprite;
	[Tooltip("A Sprite which is used when the spell does not have assigned icon.")]
	[SerializeField] Sprite missingIconSprite;

	[HideInInspector] public Spell assignedSpell;

	public void Initialize(Spell spell) {
		assignedSpell = spell;
		if (spell == null)
			spellImage.sprite = emptySlotSprite;
		else if (spell.icon == null)
			spellImage.sprite = missingIconSprite;
		else
			spellImage.sprite = spell.icon;
	}
}
