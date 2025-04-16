using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EffectsUI : MonoBehaviour {
    [Tooltip("A prefab of an effect slot which is instantiated several times.")]
    [SerializeField] EffectSlotUI effectSlotPrefab;
    [Tooltip("A parent object of all the effect slots.")]
    [SerializeField] Transform effectSlotsParent;
    [Tooltip("An Image which is used as a background. It is hidden when there are no effects so that there is no small square on the screen.")]
    [SerializeField] Image background;

    private EffectibleCharacter playerCharacter;
    private float originalBackgroundAlpha = 0f;
    private int currentEffectCount = 0;

    private void CreateNewEffectSlot(CharacterEffect effect) {
        EffectSlotUI slot = Instantiate<EffectSlotUI>(effectSlotPrefab, effectSlotsParent);
        slot.Initialize(effect);
        effect.onEffectEnd += OnEffectEnded;
        currentEffectCount++;
        // If this is the first effect added, show background
        if (currentEffectCount == 1) background.DOFade(originalBackgroundAlpha, 0.1f);
    }

    private void OnEffectEnded() {
        currentEffectCount--;
        // If there are no more effects, hide background
        background.DOFade(0f, 0.1f);
    }

    void Start()
    {
        // Hide effects background in the beginning
        originalBackgroundAlpha = background.color.a;
        background.color = background.color.WithA(0f);
        // Remove all existing slots
        UtilsMonoBehaviour.RemoveAllChildrenOfType<EffectSlotUI>(effectSlotsParent);
        // Find the player character and register callback
        playerCharacter = UtilsMonoBehaviour.FindObjectOfTypeAndTag<EffectibleCharacter>("Player");
        if (playerCharacter!= null)
            playerCharacter.onNewEffectAdded += CreateNewEffectSlot;
    }

	private void OnDestroy() {
        // Unregister callback
        if (playerCharacter != null)
            playerCharacter.onNewEffectAdded -= CreateNewEffectSlot;
    }
}
