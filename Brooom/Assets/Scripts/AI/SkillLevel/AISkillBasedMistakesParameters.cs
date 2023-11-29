using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "AI / AI Mistakes Parameters", fileName = "AIMistakesParameters")]
public class AISkillBasedMistakesParameters : ScriptableObject {

	private AnimationCurve constantOneCurve = AnimationCurve.Constant(0f, 1f, 1f);
	private AnimationCurve constantZeroCurve = AnimationCurve.Constant(0f, 1f, 0f);

	[Header("Movement")]
	// Speed mistake
	[Tooltip("Whether the AI should make speed mistake (flying in only a fraction of the maximum speed).")]
	[SerializeField] bool speedMistakeEnabled = true;
	[Tooltip("The curve describes mapping from speed mistake probability to percentage of maximum speed which is used to go forward.")]
	[SerializeField] AnimationCurve speedModifierCurve;
	public AnimationCurve SpeedModifierCurve {
		get {
			if (speedMistakeEnabled) return speedModifierCurve;
			else return constantOneCurve;
		}
	}
	[Space(5)]
	// Keeping direction
	[Tooltip("Whether the AI should make mistake of keeping a direction for a short duration after goal change.")]
	[SerializeField] bool keepDirectionMistakeEnabled = true;
	[Tooltip("The curve describes mapping from dexterity mistake probability to duration of not changing the direction to a new goal (the original direction is kept for this amount of seconds).")]
	[SerializeField] AnimationCurve keepDirectionDurationCurve;
	public AnimationCurve KeepDirectionDurationCurve {
		get {
			if (keepDirectionMistakeEnabled) return keepDirectionDurationCurve;
			else return constantZeroCurve;
		}
	}
	[Space(5)]
	// Collision avoidance
	[Tooltip("Whether the AI should make mistake of not fully avoiding collisions.")]
	[SerializeField] bool collisionAvoidanceMistakeEnabled = true;
	[Tooltip("The curve describes weight (between 0 and 1) of the direction to avoid collision based on the probability of corresponding mistake (average of dexterity and precision mistake probabilities).")]
	[SerializeField] AnimationCurve collisionAvoidanceWeightCurve;
	public AnimationCurve CollisionAvoidanceWeightCurve {
		get {
			if (collisionAvoidanceMistakeEnabled) return collisionAvoidanceWeightCurve;
			else return constantOneCurve;
		}
	}

	[Header("Hoops and checkpoints")]
	// Skipping hoops
	[Tooltip("Whether the AI should make mistake of skipping hoops.")]
	[SerializeField] bool hoopSkipMistakeEnabled = true;
	[Tooltip("The curve describes probability of skipping a hoop (checkpoint cannot be skipped) based on the precision mistake probability.")]
	[SerializeField] AnimationCurve hoopSkipCurve;
	public AnimationCurve HoopSkipCurve {
		get {
			if (hoopSkipMistakeEnabled) return hoopSkipCurve;
			else return constantZeroCurve;
		}
	}
	[Space(5)]
	// Missing hoops
	[Tooltip("Whether the AI should make mistake of missing hoops.")]
	[SerializeField] bool hoopMissMistakeEnabled = true;
	[Tooltip("The curve describes distance from the ideal target position of a hoop based on the dexterity mistake probability.")]
	[SerializeField] AnimationCurve hoopMissCurve;
	public AnimationCurve HoopMissCurve {
		get {
			if (hoopMissMistakeEnabled) return hoopMissCurve;
			else return constantZeroCurve;
		}
	}


	[Header("Bonuses")]
	// Skipping speed bonus
	[Tooltip("Whether the AI should make mistake of skipping speed bonus.")]
	[SerializeField] bool speedBonusSkipMistakeEnabled = true;
	[Tooltip("The curve describes probability of skipping a speed bonus based on the probability of corresponding mistake (average of speed and precision mistake probabilities).")]
	[SerializeField] AnimationCurve speedBonusSkipCurve;
	public AnimationCurve SpeedBonusSkipCurve {
		get {
			if (speedBonusSkipMistakeEnabled) return speedBonusSkipCurve;
			else return constantZeroCurve;
		}
	}
	[Space(5)]
	// Skipping mana bonus
	[Tooltip("Whether the AI should make mistake of skipping mana bonus.")]
	[SerializeField] bool manaBonusSkipMistakeEnabled = true;
	[Tooltip("The curve describes probability of skipping a mana bonus based on the probability of corresponding mistake (average of precision and magic mistake probabilities).")]
	[SerializeField] AnimationCurve manaBonusSkipCurve;
	public AnimationCurve ManaBonusSkipCurve {
		get {
			if (manaBonusSkipMistakeEnabled) return manaBonusSkipCurve;
			else return constantZeroCurve;
		}
	}
	[Space(5)]
	// Skipping bonus
	[Tooltip("Whether the AI should make mistake of skipping a bonus (other than speed and mana).")]
	[SerializeField] bool bonusSkipMistakeEnabled = true;
	[Tooltip("The curve describes probability of skipping a bonus (other than speed and mana) based on the precision mistake probability.")]
	[SerializeField] AnimationCurve bonusSkipCurve;
	public AnimationCurve BonusSkipCurve {
		get {
			if (bonusSkipMistakeEnabled) return bonusSkipCurve;
			else return constantZeroCurve;
		}
	}
	[Space(5)]
	// Missing bonus
	[Tooltip("Whether the AI should make mistake of missing a bonus (an arbitrary type).")]
	[SerializeField] bool bonusMissMistakeEnabled = true;
	[Tooltip("The curve describes distance from the bonus centre which is used as the target position based on the dexterity mistake probability.")]
	[SerializeField] AnimationCurve bonusMissCurve;
	public AnimationCurve BonusMissCurve {
		get {
			if (bonusMissMistakeEnabled) return bonusMissCurve;
			else return constantZeroCurve;
		}
	}
}
