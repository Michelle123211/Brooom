using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementsUI : MonoBehaviour
{
    [Tooltip("A prefab of an achievement slot which is instantiated multiple times.")]
    [SerializeField] AchievementSlotUI achievementSlotPrefab;
    [Tooltip("A Transform which is a parent of all the achievements slots.")]
    [SerializeField] Transform achievementParent;

    HashSet<string> newAchievements = new HashSet<string>(); // resets between scenes

    public void UpdateUI() {
        // Get current achievements progress
        List<AchievementProgress> achievements = AchievementManager.Instance.GetAllAchievementsProgress();
        // Whenever we update UI, note down which achievements are new at this moment (add them to those which have been new in previous updates)
        // This will ensure we highlight new achievements correctly, no matter where they were obtained (Race, Shop, Testing Track)
        // However this will highlight achievements which we have already seen highlighted, but only during the same scene session (e.g. after going to Shop and back)
        foreach (var achievement in achievements) {
            if (achievement.isNew) newAchievements.Add(achievement.achievement.name);
        }
        // Remove all existing slots
        UtilsMonoBehaviour.RemoveAllChildren(achievementParent);
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
            // Highlight any achievement which has been new during this session
            if (newAchievements.Contains(achievement.achievement.name))
                slot.HighlightNewAchievement();
        }
    }
}
