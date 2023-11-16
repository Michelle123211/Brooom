using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "AI / AI Mistakes Parameters", fileName = "AIMistakesParameters")]
public class AISkillBasedMistakesParameters : ScriptableObject {

	[Header("Movement")]
	[Tooltip("The curve describes mapping from speed mistake probability to percentage of maximum speed which is used to go forward.")]
	public AnimationCurve speedModifierCurve;
	[Tooltip("The curve describes mapping from dexterity mistake probability to duration of not changing the direction to a new goal (the original direction is kept for this amount of seconds).")]
	public AnimationCurve keepDirectionDurationCurve;
	[Tooltip("The curve describes weight (between 0 and 1) of the direction to avoid collision based on the probability of corresponding mistake (average of dexterity and precision mistake probabilities).")]
	public AnimationCurve collisionAvoidanceWeightCurve;

	[Header("Hoops and checkpoints")]
	[Tooltip("The curve describes probability of skipping a hoop (checkpoint cannot be skipped) based on the precision mistake probability.")]
	public AnimationCurve hoopSkipCurve;
	[Tooltip("The curve describes distance (normalized) from the ideal target position of a hoop/checkpoint based on the dexterity mistake probability.")]
	public AnimationCurve hoopCheckpointMissCurve;

	[Header("Bonuses")]
	[Tooltip("The curve describes probability of skipping a speed bonus based on the probability of corresponding mistake (average of speed and precision mistake probabilities).")]
	public AnimationCurve speedBonusSkipCurve;
	[Tooltip("The curve describes probability of skipping a mana bonus based on the probability of corresponding mistake (average of precision and magic mistake probabilities).")]
	public AnimationCurve manaBonusSkipCurve;
	[Tooltip("The curve describes probability of skipping a bonus (other than speed and mana) based on the precision mistake probability.")]
	public AnimationCurve bonusSkipCurve;
	[Tooltip("The curve describes distance (normalized) from the bonus centre which is used as the target position based on the dexterity mistake probability.")]
	public AnimationCurve bonusMissCurve;

}
