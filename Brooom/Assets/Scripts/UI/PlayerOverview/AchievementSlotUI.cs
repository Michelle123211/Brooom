using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AchievementSlotUI : MonoBehaviour {

	[Tooltip("Sprites which will be used as backgrounds for different levels of achievement. Highest level should be on the lowest index.")]
	[SerializeField] List<Sprite> levelBackgrounds = new List<Sprite>();

	[Tooltip("Sprite which will be used instead od the achievement's icon if the achievement has not been achieved yet.")]
	[SerializeField] Sprite unknownAchievementIcon;
	[Tooltip("Sprite which will be used as a background if the achievement is not known yet.")]
	[SerializeField] Sprite unknownAchievementBackground;
	[Tooltip("Sprite which will be used for a known achievement which has no icon assigned.")]
	[SerializeField] Sprite missingAchievementIcon;

	[Header("UI elements")]
	[SerializeField] Transform slotTransform;
	[SerializeField] Image iconImage;
	[SerializeField] Image backgroundImage;
	[SerializeField] GameObject newAchievementLabel;

	private Tooltip tooltip;

	public void Initialize(AchievementProgress achievement) {
		if (tooltip == null) tooltip = GetComponent<Tooltip>();
		// Set icon, background color, tooltip content
		if (achievement.currentLevel > 0) InitializeKnownAchievement(achievement);
		else InitializeUnknownAchievement(achievement);
		// Highlight new achievement
		if (achievement.levelChanged) {
			HighlightNewAchievement();
		} else {
			newAchievementLabel.SetActive(false);
		}
	}

	private void InitializeKnownAchievement(AchievementProgress achievement) {
		// Set icon
		if (achievement.achievement.icon != null)
			iconImage.sprite = achievement.achievement.icon;
		else iconImage.sprite = missingAchievementIcon;
		// Set background color according to the level
		int backgroundIndex = Mathf.Clamp(achievement.maximumLevel - achievement.currentLevel, 0, levelBackgrounds.Count);
		backgroundImage.sprite = levelBackgrounds[backgroundIndex];
		// Set tooltip content
		string achievementID = achievement.achievement.type.ToString();
		tooltip.texts.topLeft = "~~" + LocalizationManager.Instance.GetLocalizedString("Achievement" + achievementID) + "~~";
		string description = LocalizationManager.Instance.GetLocalizedString("AchievementTooltip" + achievement.achievement.type.ToString());
		if (achievement.achievement.valuesForLevels != null && achievement.achievement.valuesForLevels.Count > 0)
			tooltip.texts.mainTop = string.Format(description, achievement.achievement.valuesForLevels[achievement.currentLevel - 1]);
		else
			tooltip.texts.mainTop = description;
	}

	private void InitializeUnknownAchievement(AchievementProgress achievement) {
		iconImage.sprite = unknownAchievementIcon;
		backgroundImage.sprite = unknownAchievementBackground;
		tooltip.texts.topLeft = "~~" + LocalizationManager.Instance.GetLocalizedString("AchievementUnknown") + "~~";
		tooltip.texts.mainTop = LocalizationManager.Instance.GetLocalizedString("AchievementTooltipUnknown");
	}

	private void HighlightNewAchievement() {
		newAchievementLabel.SetActive(true);
		// A short tween changing scale
		slotTransform.DOScale(1.3f, 0.2f).SetDelay(1f).OnComplete(() => slotTransform.DOScale(1f, 0.4f).SetEase(Ease.OutBounce));
	}
}
