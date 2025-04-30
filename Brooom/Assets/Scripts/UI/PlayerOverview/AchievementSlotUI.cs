using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


/// <summary>
/// A component representing a single achievement displayed in UI as an icon with a border around it.
/// Background color is determined by the achievement's level reached.
/// A separate set of sprites is used when the associated achievement is not known to the player yet.
/// There is also a notification dot to indicate a new achievement (or a new level).
/// </summary>
public class AchievementSlotUI : MonoBehaviour {

	[Header("Sprites")]

	[Tooltip("Sprites which will be used as backgrounds for different levels of achievement. Highest level should be on the lowest index.")]
	[SerializeField] List<Sprite> levelBackgrounds = new List<Sprite>();

	[Tooltip("A Sprite which is used as border of a known achievement.")]
	[SerializeField] Sprite slotBorderSprite;
	[Tooltip("A Sprite which is used as border of an unknown achievement.")]
	[SerializeField] Sprite unknownSlotBorderSprite;
	[Tooltip("Color which is used for an unknown achievement border.")]
	[SerializeField] Color unknownSlotBorderColor;
	[Tooltip("Sprite which will be used instead of the achievement's icon if the achievement has not been achieved yet.")]
	[SerializeField] Sprite unknownAchievementIcon;
	[Tooltip("Sprite which will be used as a background if the achievement is not known yet.")]
	[SerializeField] Sprite unknownAchievementBackground;
	[Tooltip("Sprite which will be used for a known achievement which has no icon assigned.")]
	[SerializeField] Sprite missingAchievementIcon;

	[Header("UI elements")]
	[Tooltip("Transform containing all UI elements belonging to this achievement slot. It is scaled up briefly to indicate a new achievement.")]
	[SerializeField] Transform slotTransform;
	[Tooltip("Image displaying the achievement's icon.")]
	[SerializeField] Image iconImage;
	[Tooltip("Image displaying the achievement's background.")]
	[SerializeField] Image backgroundImage;
	[Tooltip("Image displaying the achievement slot's border.")]
	[SerializeField] Image borderImage;
	[Tooltip("Visual notification which is activated to indicate a new achievement.")]
	[SerializeField] GameObject newAchievementNotification;

	private AchievementProgress assignedAchievement;
	private Tooltip tooltip; // displaying the achievement's description

	/// <summary>
	/// Initializes the achievement slot with the given achievement (i.e. sets the icon, background color and tooltip content accordingly).
	/// </summary>
	/// <param name="achievement">Achievement to be assigned to the slot.</param>
	public void Initialize(AchievementProgress achievement) {
		this.assignedAchievement = achievement;
		if (tooltip == null) tooltip = GetComponent<Tooltip>();
		// Set icon, background color, tooltip content
		if (achievement.currentLevel > 0) InitializeKnownAchievement(achievement);
		else InitializeUnknownAchievement(achievement);
	}

	/// <summary>
	/// Shows visual notification of a new achievement and briefly scales the achievement slot up.
	/// </summary>
	public void HighlightNewAchievement() {
		newAchievementNotification.SetActive(true);
		// A short tween changing scale
		slotTransform.DOScale(1.3f, 0.2f).SetDelay(1f).OnComplete(() => slotTransform.DOScale(1f, 0.4f).SetEase(Ease.OutBounce));
	}

	// Initializes the slot to show current level of the given achievement
	private void InitializeKnownAchievement(AchievementProgress achievement) {
		// Set icon
		if (achievement.achievement.icon != null)
			iconImage.sprite = achievement.achievement.icon;
		else iconImage.sprite = missingAchievementIcon;
		// Set border
		borderImage.sprite = slotBorderSprite;
		borderImage.color = Color.black;
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

	// Initializes the slot for an achievement which is not known for the player yet
	private void InitializeUnknownAchievement(AchievementProgress achievement) {
		iconImage.sprite = unknownAchievementIcon;
		backgroundImage.sprite = unknownAchievementBackground;
		borderImage.sprite = unknownSlotBorderSprite;
		borderImage.color = unknownSlotBorderColor;
		tooltip.texts.topLeft = "~~" + LocalizationManager.Instance.GetLocalizedString("AchievementUnknown") + "~~";
		tooltip.texts.mainTop = LocalizationManager.Instance.GetLocalizedString("AchievementTooltipUnknown");
	}
}
