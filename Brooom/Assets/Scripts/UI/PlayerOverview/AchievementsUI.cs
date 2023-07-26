using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementsUI : MonoBehaviour
{

    public void UpdateUI() {
        AchievementManager.Instance.GetAllAchievementsProgress();
        // TODO: Sort the achievements and display them
    }
}
