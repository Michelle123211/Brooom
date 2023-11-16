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
	[Tooltip("The curve describes weight (between 0 and 1) of the direction to avoid collision based on the probability of mistake (average of dexterity and precision mistake probabilities).")]
	public AnimationCurve collisionAvoidanceWeightCurve;

	//[Header("Hoops and checkpoints")]
	// TODO: Skip hoop/checkpoint
	// TODO: Miss hoop

	//[Header("Bonuses")]
	// TODO: Skip speed bonus
	// TODO: Skip mana bonus
	// TODO: Skip bonus
	// TODO: Miss bonus

}
