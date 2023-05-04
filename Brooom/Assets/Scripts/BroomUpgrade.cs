using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BroomUpgrade : MonoBehaviour {
    // TODO: Show some warning that changing the name will cause problems with save files from previous game versions
    // TODO: Show some warning when length of List is not equal to MaxLevel


    [field: SerializeField]
    public string Name { get; private set; }

    [field: SerializeField]
    public int MaxLevel { get; private set; } = 3;

    public int CurrentLevel { get; private set; } = 0;

    [SerializeField]
    public List<UnityEvent> effectsForEachLevel;

    // Increases the current level and invokes corresponding effects
    public void LevelUp() {
        if (CurrentLevel < MaxLevel) {
            if (CurrentLevel >= effectsForEachLevel.Count) {
                Debug.LogWarning($"No effect of the {Name} upgrade specified for the {CurrentLevel + 1} level!");
            } else {
                if (!Utils.IsNullEvent(effectsForEachLevel[CurrentLevel]))
                    effectsForEachLevel[CurrentLevel].Invoke();
            }
            CurrentLevel++;
        } 
    }
}
