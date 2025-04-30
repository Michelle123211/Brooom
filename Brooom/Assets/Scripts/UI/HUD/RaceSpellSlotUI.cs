using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;


/// <summary>
/// A component responsible for displaying a single spell slot in the HUD during race.
/// It uses <c>SpellSlotUI</c> for the basic spell slot functionality,
/// but adds behaviour for indicating whether the spell is available, whether the spell is currently selected, what is the mana cost, etc.
/// </summary>
public class RaceSpellSlotUI : MonoBehaviour {

	[Header("Parameters")]
	[Tooltip("When the spell is unavailable (not charged, not enough mana) the slot becomes smaller.")]
	[SerializeField] float unavailableScale = 0.75f;
	[Tooltip("When the spell is unavailable (not charged, not enough mana) the slot becomes transparent with this alpha value.")]
	[SerializeField] float unavailableAlpha = 0.8f;

	[Header("UI elements")]
	[Tooltip("Game Object containing everything related to the spell's mana cost. Is deactivated when no spell is assigned.")]
	[SerializeField] GameObject manaPartParent;
	[Tooltip("A TextMesh Pro displaying mana cost.")]
	[SerializeField] TextMeshProUGUI manaCostLabel;
	[Tooltip("An Image used as a background of the mana cost label.")]
	[SerializeField] Image manaCostBackground;
	[Tooltip("An Image containing spell icon, which is used to indicate how charged the spell is.")]
	[SerializeField] Image spellIconFillImage;
	[Tooltip("An Image containing background, which is used to indicate how charged the spell is.")]
	[SerializeField] Image backgroundFillImage;
	[Tooltip("An Image used as a spell icon.")]
	[SerializeField] Image spellIconImage;
	[Tooltip("An Image used as a background of the spell slot.")]
	[SerializeField] Image backgroundImage;
	[Tooltip("Canvas Group used to set alpha value for the whole slot.")]
	[SerializeField] CanvasGroup contentCanvasGroup;

	[Header("Related objects")]
	[Tooltip("SpellSlotUI used to display the basic spell slot.")]
	[SerializeField] SpellSlotUI spellSlotUI;

	/// <summary>Spell assigned to the slot, its icon is displayed in the slot.</summary>
	[HideInInspector] public SpellInRace assignedSpell;
	/// <summary>Whether the slot is empty (i.e. there is no assigned spell).</summary>
	[HideInInspector] public bool isEmpty = false;

	private bool isInitialized = false;

	/// <summary>
	/// Initializes this spell slot with the given spell (i.g., sets the icon and the mana cost).
	/// </summary>
	/// <param name="spell">Spell to be assigned to the slot.</param>
	public void Initialize(SpellInRace spell) {
		assignedSpell = spell;
		isEmpty = (spell == null || spell.Spell == null || string.IsNullOrEmpty(spell.Spell.Identifier));
		// Register callbacks
		if (!isEmpty) {
			assignedSpell.onBecomesAvailable += ChangeToAvailable;
			assignedSpell.onBecomesUnavailable += ChangeToUnavailable;
		}
		// Set mana cost
		if (isEmpty) manaPartParent.SetActive(false);
		else manaCostLabel.text = spell.Spell.ManaCost.ToString();
		// Change icon
		spellSlotUI.Initialize(isEmpty ? null : spell.Spell);
		spellIconFillImage.sprite = spellIconImage.sprite;
		backgroundFillImage.sprite = backgroundImage.sprite;
		backgroundFillImage.color = backgroundImage.color;
		if (!isEmpty) { // images under fill images should be a little transparent
			spellIconImage.color = spellIconImage.color.WithA(unavailableAlpha);
			backgroundImage.color = backgroundImage.color.WithA(unavailableAlpha);
		}
		// Initialize image fill, scale and alpha of the overlay
		spellIconFillImage.fillAmount = 0;
		backgroundFillImage.fillAmount = 0;
		transform.localScale = Vector3.one * unavailableScale;
		contentCanvasGroup.alpha = unavailableAlpha;
		// Change to available if necessary
		if (!isEmpty && assignedSpell.IsAvailable)
			ChangeToAvailable(assignedSpell.Spell); // the parameter is there just so it is compatible with callback in SpellInRace

		isInitialized = true;
	}

	/// <summary>
	/// Called when the spell slot has been selected. It rotates the spell slot a little for a very short time as an indication.
	/// </summary>
	public void Select() {
		// Rotate for a very short time
		transform.DORotate(Vector3.forward * 10, 0.04f)
			.OnComplete(() => transform.DORotate(Vector3.forward * -10, 0.08f)
				.OnComplete(() => transform.DORotate(Vector3.zero, 0.04f)));
	}

	/// <summary>
	/// Called when the spell slot has been deselected. It doesn't to anything right now.
	/// </summary>
	public void Deselect() {
	}

	// Tweens scale and alpha value of the spell slot to indicate it is available
	private void ChangeToAvailable(Spell _) { // the parameter is there just so it is compatible with callback in SpellInRace
		// If the spell becomes available (is charged and enough mana), tween its scale and alpha of the unavailable overlay
		transform.DOKill();
		transform.DOScale(1f, 0.3f)
			.OnComplete(() => transform.DOScale(0.98f, 0.5f).SetEase(Ease.InQuad).SetLoops(-1, LoopType.Yoyo));
		contentCanvasGroup.DOKill();
		contentCanvasGroup.DOFade(1f, 0.3f);
	}

	// Tweens scale and alpha value of the spell slot to indicate it is unavailable
	private void ChangeToUnavailable(Spell _) { // the parameter is there just so it is compatible with callback in SpellInRace
		// If the spell becomes unavailable (not enough mana, not charged) tween its scale and alpha of the unavailable overlay
		transform.DOKill();
		transform.DOScale(unavailableScale, 0.3f);
		contentCanvasGroup.DOKill();
		contentCanvasGroup.DOFade(unavailableAlpha, 0.3f);
	}

	private void Update() {
		if (!isEmpty) {
			// Change image fill based on charge
			spellIconFillImage.fillAmount = assignedSpell.Charge;
			backgroundFillImage.fillAmount = assignedSpell.Charge;
			// Change color of mana cost
			if (assignedSpell.HasEnoughMana) manaCostBackground.color = ColorPalette.Instance.GetColor(ColorFromPalette.MainUI_PositiveColor);
			else manaCostBackground.color = ColorPalette.Instance.GetColor(ColorFromPalette.MainUI_NegativeColor);
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
