using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementsUI : MonoBehaviour
{
    [Tooltip("A prefab of an achievement slot which is instantiated multiple times.")]
    [SerializeField] AchievementSlotUI achievementSlotPrefab;
    [Tooltip("A Transform which is a parent of all the achievements slots.")]
    [SerializeField] Transform achievementParent;

    public void UpdateUI() {
        // Remove all existing slots
        UtilsMonoBehaviour.RemoveAllChildren(achievementParent);
        // Get current achievements progress
        List<AchievementProgress> achievements = AchievementManager.Instance.GetAllAchievementsProgress();
        // Sort the achievements according to their level (first are the complete ones, then those having only one more level etc., unknown achievements are at the end)
        achievements.Sort((x, y) => {
            if (x.currentLevel == 0) return 1; // x is unknown, y should be first
            if (y.currentLevel == 0) return -1; // y is unknown, x should be first
            return (x.maximumLevel - x.currentLevel) - (y.maximumLevel - y.currentLevel); // the one with less levels remaining should be first
        });
        // Display the achievements
        foreach (var achievement in achievements) {
            AchievementSlotUI slot = Instantiate<AchievementSlotUI>(achievementSlotPrefab, achievementParent);
            slot.Initialize(achievement);
        }
    }
}
