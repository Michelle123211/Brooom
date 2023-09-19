using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsUI : MonoBehaviour {
    [Tooltip("A prefab of an effect slot which is instantiated several times.")]
    [SerializeField] EffectSlotUI effectSlotPrefab;
    [Tooltip("A parent object of all the effect slots.")]
    [SerializeField] Transform effectSlotsParent;

    private EffectibleCharacter playerCharacter;

    private void CreateNewEffectSlot(CharacterEffect effect) {
        EffectSlotUI slot = Instantiate<EffectSlotUI>(effectSlotPrefab, effectSlotsParent);
        slot.Initialize(effect);
    }

    void Start()
    {
        // Remove all existing slots
        UtilsMonoBehaviour.RemoveAllChildren(effectSlotsParent);
        // Find the player character and register callback
        EffectibleCharacter[] characters = FindObjectsOfType<EffectibleCharacter>();
        foreach (var character in characters) {
            if (character.CompareTag("Player")) {
                playerCharacter = character;
                playerCharacter.onNewEffectAdded += CreateNewEffectSlot;
                break;
            }
        }
    }

	private void OnDestroy() {
        // Unregister callback
        if (playerCharacter != null)
            playerCharacter.onNewEffectAdded -= CreateNewEffectSlot;
    }
}
