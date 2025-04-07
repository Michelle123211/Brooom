using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BroomUpgrade : MonoBehaviour {
    // TODO: Show some warning that changing the name will cause problems with save files from previous game versions
    // TODO: Show some warning when length of List is not equal to MaxLevel + 1


    [field: SerializeField]
    public string UpgradeName { get; private set; }

    [field: SerializeField]
    public List<int> CoinsCostOfEachLevel { get; private set; }

    [field: SerializeField]
    public int MaxLevel { get; private set; } = 3;

    public int CurrentLevel { get; private set; } = 0;

    [SerializeField]
    public List<UnityEvent> effectsForEachLevel;

    // Increases the current level and invokes corresponding effects
    public void LevelUp() {
        if (CurrentLevel < MaxLevel) {
            CurrentLevel++;
            ApplyEffectForCurrentLevel();
        } 
    }

    // Decreases the current level and invokes corresponding effects
    public void LevelDown() {
        if (CurrentLevel > 0) {
            CurrentLevel--;
            ApplyEffectForCurrentLevel();
        }
    }

    private void ApplyEffectForCurrentLevel() {
        if (CurrentLevel > effectsForEachLevel.Count - 1) {
            Debug.LogWarning($"No effect of the {UpgradeName} upgrade specified for the {CurrentLevel} level!");
        } else {
            if (!Utils.IsNullEvent(effectsForEachLevel[CurrentLevel]))
                effectsForEachLevel[CurrentLevel].Invoke();
        }
    }

	private void Start() {
        // Initialize using events for initial level
        ApplyEffectForCurrentLevel();
	}
}
