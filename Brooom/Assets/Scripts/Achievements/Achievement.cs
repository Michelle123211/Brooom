using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Achievements / Achievement", fileName = "Achievement")]
public class Achievement : ScriptableObject {

	[Tooltip("Icon of the achievement on a transparent background.")]
	public Sprite icon;

	[Tooltip("What value is the achievement connected with.")]
	public AchievementType type = AchievementType.None;

	[Tooltip("What are the values necessary for each achievement level.")]
	public List<float> valuesForLevels;
}
