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
        // TODO: Sort the achievements according to their level
        // Display the achievements
        foreach (var achievement in achievements) {
            AchievementSlotUI slot = Instantiate<AchievementSlotUI>(achievementSlotPrefab, achievementParent);
            slot.Initialize(achievement);
        }
    }
}
