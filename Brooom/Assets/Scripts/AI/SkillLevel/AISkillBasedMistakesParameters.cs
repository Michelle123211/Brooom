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
			else return constantOneCurve; // 100 % of maximum speed
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
			else return constantZeroCurve; // 0 % probability of keeping a direction
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
			else return constantOneCurve; // considering direction to avoid collision with full weight (1)
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
			else return constantZeroCurve; // 0 % probability of skipping a hoop
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
			else return constantZeroCurve; // 0 % probability of missing a hoop
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
			else return constantZeroCurve; // 0 % probability of skipping a mana bonus
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
			else return constantZeroCurve; // 0 % probability of skipping a mana bonus
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
			else return constantZeroCurve; // 0 % probability of skipping a bonus
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
			else return constantZeroCurve; // 0 % probability of missing a bonus
		}
	}


	[Header("Spell casting")]
	// Spell cast decision interval duration
	[Tooltip("Whether the AI should adjust how often it makes decisions about spell casting.")]
	[SerializeField] bool variableSpellDecisionIntervalEnabled = true;
	[Tooltip("The curve describes duration (in seconds) between subsequent spell cast decisions based on the magic mistake probability.")]
	[SerializeField] AnimationCurve spellDecisionIntervalCurve;
	public AnimationCurve SpellDecisionIntervalCurve {
		get {
			if (variableSpellDecisionIntervalEnabled) return spellDecisionIntervalCurve;
			else return constantOneCurve; // interval duration 1 s
		}
	}
	// Casting a spell
	[Tooltip("Whether the AI should make a mistake of not casting a spell sometimes.")]
	[SerializeField] bool spellCastMistakeEnabled = true;
	[Tooltip("The curve describes probability of casting a spell (which is ready to be used) based on the magic mistake probability.")]
	[SerializeField] AnimationCurve spellCastCurve;
	public AnimationCurve SpellCastCurve {
		get {
			if (spellCastMistakeEnabled) return spellCastCurve;
			else return constantOneCurve; // casting a spell 100 % of time
		}
	}
	// Number of unique spells used
	[Tooltip("Whether the AI should make a mistake of using only a subset of all equipped spells.")]
	[SerializeField] bool notUsingAllSpellsMistakeEnabled = true;
	[Tooltip("The curve describes mapping from magic mistake probability to percentage of equipped spells used.")]
	[SerializeField] AnimationCurve spellUsedCountCurve;
	public AnimationCurve SpellUsedCountCurve {
		get {
			if (notUsingAllSpellsMistakeEnabled) return spellUsedCountCurve;
			else return constantOneCurve; // using 100 % of equipped spells
		}
	}

}
