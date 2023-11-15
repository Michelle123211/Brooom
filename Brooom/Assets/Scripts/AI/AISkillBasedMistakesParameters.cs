using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "AI / AI Mistakes Parameters", fileName = "AIMistakesParameters")]
public class AISkillBasedMistakesParameters : ScriptableObject {

	[Header("Movement")]
	[Tooltip("The curve describes mapping from speed mistake probability to percentage of maximum speed which is used to go forward.")]
	public AnimationCurve speedBasedOnMistakeProbability;
	[Tooltip("The curve describes mapping from dexterity mistake probability to duration of not changing the direction to a new goal (the original direction is kept for this amount of seconds).")]
	public AnimationCurve keepDirectionDurationCurve;
	// TODO: Avoiding collisions

	//[Header("Hoops and checkpoints")]
	// TODO: Skip hoop/checkpoint
	// TODO: Miss hoop

	//[Header("Bonuses")]
	// TODO: Skip speed bonus
	// TODO: Skip bonus
	// TODO: Miss bonus

}
