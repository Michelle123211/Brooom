using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsUI : MonoBehaviour {
    [Tooltip("A prefab of an effect slot which is instantiated several times.")]
    [SerializeField] EffectSlotUI effectSlotPrefab;
    [Tooltip("A parent object of all the effect slots.")]
    [SerializeField] Transform effectSlotsParent;

    private void CreateNewEffectSlot(PlayerEffect effect) {
        EffectSlotUI slot = Instantiate<EffectSlotUI>(effectSlotPrefab, effectSlotsParent);
        slot.Initialize(effect);
    }

    void Start()
    {
        // Remove all existing slots
        for (int i = effectSlotsParent.childCount - 1; i >= 0; i--) {
            Destroy(effectSlotsParent.GetChild(i).gameObject);
        }
        // Register callback
        PlayerState.Instance.raceState.onNewEffectAdded += CreateNewEffectSlot;
    }

	private void OnDestroy() {
        // Unregister callback
        PlayerState.Instance.raceState.onNewEffectAdded -= CreateNewEffectSlot;
    }
}
