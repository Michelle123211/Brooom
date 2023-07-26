using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AchievementSlotUI : MonoBehaviour {

	[Tooltip("Sprite which will be used instead od the achievement's icon if the achievement has not been achieved yet.")]
	[SerializeField] Sprite unknownAchievementIcon;
	[Tooltip("Color which will be used for an uknown achievement.")]
	[SerializeField] Color unknownAchievementColor;
	[Tooltip("Localization key with description of unknown achievement (is displayed in a tooltip).")]
	[SerializeField] string unknownAchievementLocKey;

	[Header("UI elements")]
	[SerializeField] Transform slotTransform;
	[SerializeField] Image iconImage;
	[SerializeField] Image backgroundImage;
	[SerializeField] GameObject newAchievementLabel;

	private SimpleTooltip tooltip;

	Color[] levelColors = new Color[] { // TODO: Use colors from a color palette (the same colors as for the place in HUD)
        Utils.ColorFromRBG256(243, 217, 81), // gold
        Utils.ColorFromRBG256(164, 164, 164), // silver
        Utils.ColorFromRBG256(203, 128, 83), // bronz
        Utils.ColorFromRBG256(126, 92, 80) };

	public void Initialize(AchievementProgress achievement) {
		if (tooltip == null) tooltip = GetComponent<SimpleTooltip>();
		if (achievement.currentLevel > 0) {
			// Set icon
			iconImage.sprite = achievement.achievement.icon;
			// Set background color according to the level
			int colorIndex = Mathf.Clamp(achievement.maximumLevel - achievement.currentLevel, 0, levelColors.Length);
			backgroundImage.color = levelColors[colorIndex];
			// Set tooltip content
			tooltip.text = "Achievement" + achievement.achievement.name + achievement.currentLevel.ToString();
		} else {
			// Achievement which is not known
			iconImage.sprite = unknownAchievementIcon;
			backgroundImage.color = unknownAchievementColor;
			tooltip.text = unknownAchievementLocKey;
		}
		// Highlight new achievement
		if (achievement.levelChanged) {
			HighlightNewAchievement();
		} else {
			newAchievementLabel.SetActive(false);
		}
	}

	private void HighlightNewAchievement() {
		newAchievementLabel.SetActive(true);
		// A short tween changing scale
		slotTransform.DOScale(1.3f, 0.2f).SetDelay(1f).OnComplete(() => slotTransform.DOScale(1f, 0.4f).SetEase(Ease.OutBounce));
	}
}
