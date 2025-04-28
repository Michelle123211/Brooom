using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// A component representing a single broom upgrade, recording its current level and providing methods for leveling up or down.
/// </summary>
public class BroomUpgrade : MonoBehaviour {
    // TODO: Show some warning that changing the name will cause problems with save files from previous game versions
    // TODO: Show some warning when length of List is not equal to MaxLevel + 1

    /// <summary>Name of the broom upgrade which is used as a key everywhere (e.g., save file, player's state).</summary>
    [field:Tooltip("Name of the broom upgrade which is used as a key everywhere (e.g., save file, player's state).")]
    [field: SerializeField]
    public string UpgradeName { get; private set; }

    /// <summary>How much it costs to upgrade to each level.</summary>
    [field:Tooltip("How much it costs to upgrade to each level.")]
    [field: SerializeField]
    public List<int> CoinsCostOfEachLevel { get; private set; }

    /// <summary>Maximum possible level of the broom upgrade.</summary>
    [field:Tooltip("Maximum possible level of the broom upgrade.")]
    [field: SerializeField]
    public int MaxLevel { get; private set; } = 3;

    /// <summary>Current level of the broom upgrade.</summary>
    public int CurrentLevel { get; private set; } = 0;

    [Tooltip("What should happen when the broom is upgraded to each level.")]
    [SerializeField]
    public List<UnityEvent> effectsForEachLevel;


    /// <summary>
    /// Increments the current level and invokes effects corresponding to this level.
    /// </summary>
    public void LevelUp() {
        if (CurrentLevel < MaxLevel) {
            CurrentLevel++;
            ApplyEffectForCurrentLevel();
        } 
    }

    /// <summary>
    /// Decrements the current level and invokes effects corresponding to this level.
    /// </summary>
    public void LevelDown() {
        if (CurrentLevel > 0) {
            CurrentLevel--;
            ApplyEffectForCurrentLevel();
        }
    }

    // Applies effects corresponding to the current level
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
