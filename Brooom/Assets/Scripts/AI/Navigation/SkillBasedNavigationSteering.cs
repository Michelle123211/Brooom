using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBasedNavigationSteering : BasicNavigationSteering {

	protected AISkillLevel agentSkillLevel;

	protected override void InitializeDerivedType() {
		base.InitializeDerivedType();
		this.agentSkillLevel = agent.GetComponentInChildren<AISkillLevel>();
	}

	protected override CharacterMovementValues GetMovementToTargetPosition() {
		CharacterMovementValues movement = base.GetMovementToTargetPosition();
		// Slow down based on the probability of speed mistakes
		if (movement.forwardMotion == ForwardMotion.Forward) {
			movement.forwardValue *= agentSkillLevel.mistakesParameters.SpeedModifierCurve.Evaluate(agentSkillLevel.GetSpeedMistakeProbability());
		}
		return movement;
	}

	protected override CharacterMovementValues AdjustMovementToAvoidCollisions(CharacterMovementValues movement) {
		CollisionAvoidanceDirection direction = GetCollisionAvoidanceDirection();
		// If the direction to avoid collisions has no weight, simply continue in the original direction
		if (direction.weight == 0) return movement;
		// Get collision avoidance weight based on mistake probabilities
		float mistakeProbability = (agentSkillLevel.GetDexterityMistakeProbability() + agentSkillLevel.GetPrecisionMistakeProbability()) / 2f;
		float avoidanceWeight = agentSkillLevel.mistakesParameters.CollisionAvoidanceWeightCurve.Evaluate(mistakeProbability);
		// Combine direction to target with the direction to avoid collisions (weighted average)
		return CombineMovementWithCollisionAvoidance(movement, direction, avoidanceWeight);
	}
}
