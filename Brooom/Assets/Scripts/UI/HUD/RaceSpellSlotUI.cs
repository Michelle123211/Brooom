using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RaceSpellSlotUI : MonoBehaviour {

	[Header("Parameters")]
	[Tooltip("When the spell is unavailable (not charged, not enough mana) the slot becomes smaller.")]
	[SerializeField] float unavailableScale = 0.75f;
	[Tooltip("When the spell is unavailable (not charged, not enough mana) an overlay with an alpha value appears.")]
	[SerializeField] float unavailableOverlayAlpha = 0.3f;

	[Header("UI elements")]
	[Tooltip("An Image used to indicate how charged the spell is.")]
	[SerializeField] Image fillImage;
	[Tooltip("An Image used as a background of the spell slot.")]
	[SerializeField] Image backgroundImage;
	[Tooltip("An Image used as an overlay when the spell is unavailable.")]
	[SerializeField] Image unavailableOverlay;

	[HideInInspector] public EquippedSpell assignedSpell;
	[HideInInspector] public bool isEmpty = false;

	private SpellSlotUI spellSlotUI;

	private bool isInitialized = false;

	public void Initialize(EquippedSpell spell) {
		assignedSpell = spell;
		isEmpty = (spell == null || spell.spell == null);
		// Register callbacks
		if (!isEmpty) {
			assignedSpell.onBecomesAvailable += ChangeToAvailable;
			assignedSpell.onBecomesUnavailable += ChangeToUnavailable;
		}
		// Change icon
		spellSlotUI.Initialize(spell == null ? null : spell.spell);
		fillImage.sprite = backgroundImage.sprite;
		// Initialize image fill, scale and alpha of the overlay
		if (isEmpty) InitializeEmptySlot();
		else InitializeSpellSlot();
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

	private void InitializeEmptySlot() {
		// Initialize image fill, scale and alpha of the overlay
		fillImage.fillAmount = 1;
		transform.localScale = Vector3.one;
		unavailableOverlay.color = unavailableOverlay.color.WithA(0);
	}

	private void InitializeSpellSlot() {
		// Initialize image fill, scale and alpha of the overlay
		fillImage.fillAmount = 0;
		transform.localScale = Vector3.one * unavailableScale;
		unavailableOverlay.color = unavailableOverlay.color.WithA(unavailableOverlayAlpha);
	}

	private void ChangeToAvailable() {
		// If the spell becomes available (is charged and enough mana), tween its scale and alpha of the unavailable overlay
		transform.DOKill();
		transform.DOScale(1f, 0.3f)
			.OnComplete(() => transform.DOScale(0.98f, 0.5f).SetEase(Ease.InQuad).SetLoops(-1, LoopType.Yoyo));
		unavailableOverlay.DOKill();
		unavailableOverlay.DOFade(0f, 0.3f);
	}

	private void ChangeToUnavailable() {
		// If the spell becomes unavailable (not enough mana, not charged) tween its scale and alpha of the unavailable overlay
		transform.DOKill();
		transform.DOScale(unavailableScale, 0.3f);
		unavailableOverlay.DOKill();
		unavailableOverlay.DOFade(unavailableOverlayAlpha, 0.3f);
	}

	private void Update() {
		if (!isEmpty) {
			// Change image fill based on charge
			fillImage.fillAmount = assignedSpell.charge;
		}
	}

	private void Awake() {
		spellSlotUI = GetComponentInChildren<SpellSlotUI>();
	}

	private void OnDestroy() {
		if (isInitialized && !isEmpty) {
			// Unregister callbacks
			assignedSpell.onBecomesAvailable -= ChangeToAvailable;
			assignedSpell.onBecomesUnavailable -= ChangeToUnavailable;
		}	
	}
}
