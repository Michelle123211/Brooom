using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class RaceSpellSlotUI : MonoBehaviour {

	[Header("Parameters")]
	[Tooltip("When the spell is unavailable (not charged, not enough mana) the slot becomes smaller.")]
	[SerializeField] float unavailableScale = 0.75f;
	[Tooltip("When the spell is unavailable (not charged, not enough mana) the slot becomes transparent with this alpha value.")]
	[SerializeField] float unavailableAlpha = 0.8f;

	[Header("UI elements")]
	[Tooltip("A TMP displaying mana cost.")]
	[SerializeField] TextMeshProUGUI manaCostLabel;
	[Tooltip("An Image used to indicate how charged the spell is.")]
	[SerializeField] Image fillImage;
	[Tooltip("An Image used as a background of the spell slot.")]
	[SerializeField] Image backgroundImage;
	[Tooltip("Canvas Group used to set alpha value for the whole slot.")]
	[SerializeField] CanvasGroup contentCanvasGroup;

	[Header("Related objects")]
	[SerializeField] SpellSlotUI spellSlotUI;

	[HideInInspector] public SpellInRace assignedSpell;
	[HideInInspector] public bool isEmpty = false;

	private bool isInitialized = false;

	public void Initialize(SpellInRace spell) {
		assignedSpell = spell;
		isEmpty = (spell == null || spell.Spell == null || string.IsNullOrEmpty(spell.Spell.Identifier));
		// Register callbacks
		if (!isEmpty) {
			assignedSpell.onBecomesAvailable += ChangeToAvailable;
			assignedSpell.onBecomesUnavailable += ChangeToUnavailable;
		}
		// Set mana cost
		if (isEmpty) manaCostLabel.gameObject.SetActive(false);
		else manaCostLabel.text = spell.Spell.ManaCost.ToString();
		// Change icon
		spellSlotUI.Initialize(isEmpty ? null : spell.Spell);
		fillImage.sprite = backgroundImage.sprite;
		// Initialize image fill, scale and alpha of the overlay
		fillImage.fillAmount = 0;
		transform.localScale = Vector3.one * unavailableScale;
		contentCanvasGroup.alpha = unavailableAlpha;
		// Change to available if necessary
		if (!isEmpty && assignedSpell.IsAvailable)
			ChangeToAvailable();

		isInitialized = true;
	}

	public void Select() {
		// Rotate for a very short time
		transform.DORotate(Vector3.forward * 10, 0.04f)
			.OnComplete(() => transform.DORotate(Vector3.forward * -10, 0.08f)
				.OnComplete(() => transform.DORotate(Vector3.zero, 0.04f)));
	}

	public void Deselect() {
	}

	private void ChangeToAvailable() {
		// If the spell becomes available (is charged and enough mana), tween its scale and alpha of the unavailable overlay
		transform.DOKill();
		transform.DOScale(1f, 0.3f)
			.OnComplete(() => transform.DOScale(0.98f, 0.5f).SetEase(Ease.InQuad).SetLoops(-1, LoopType.Yoyo));
		contentCanvasGroup.DOKill();
		contentCanvasGroup.DOFade(1f, 0.3f);
	}

	private void ChangeToUnavailable() {
		// If the spell becomes unavailable (not enough mana, not charged) tween its scale and alpha of the unavailable overlay
		transform.DOKill();
		transform.DOScale(unavailableScale, 0.3f);
		contentCanvasGroup.DOKill();
		contentCanvasGroup.DOFade(unavailableAlpha, 0.3f);
	}

	private void Update() {
		if (!isEmpty) {
			// Change image fill based on charge
			fillImage.fillAmount = assignedSpell.Charge;
			// Change color of mana cost
			if (assignedSpell.HasEnoughMana) manaCostLabel.color = Color.green; // TODO: Use color from color palette
			else manaCostLabel.color = Color.red; // TODO: Use color from color palette
		}
	}

	private void OnDestroy() {
		if (isInitialized && !isEmpty) {
			// Unregister callbacks
			assignedSpell.onBecomesAvailable -= ChangeToAvailable;
			assignedSpell.onBecomesUnavailable -= ChangeToUnavailable;
		}	
	}
}
