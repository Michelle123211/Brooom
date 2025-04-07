using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class NavigationGoal {

    public abstract NavigationGoalType Type { get; }
    public abstract Vector3 TargetPosition { get; }

    protected GameObject agent;
    protected AISkillLevel agentSkillLevel;
    protected CharacterRaceState raceState;

    // If the agent is at most at this distance from the goal, the goal is considered reached
    protected const float REACHED_DISTANCE_THRESHOLD = 0.5f;

    public NavigationGoal(GameObject agent) {
        this.agent = agent;
        this.agentSkillLevel = agent.GetComponentInChildren<AISkillLevel>();
        this.raceState = agent.GetComponentInChildren<CharacterRaceState>();
    }

    public abstract bool IsReached();
    public abstract bool IsValid();

    // Whether it was decided based on the racer's skills that this goal should be skipped
    public abstract bool ShouldBeSkipped();

    // Adjustment to whether it was decided based on the racer's skills that this goal should be pursued but failed
    public abstract bool DetermineIfShouldFail();

    public abstract float GetRationality();

    public bool IsSameAs(NavigationGoal other) {
        if (other == null) return false;
        // Check if it is the same instance
        if (System.Object.ReferenceEquals(this, other)) return true;
        // Check runtime types
        if (this.GetType() != other.GetType() || this.Type != other.Type) return false;
        // Type-specific comparison (e.g. based on fields)
        return IsSameAs_SameType(other);
    }

    // It may be presumed the "other" goal can be cast into the same type as the defining type
    protected abstract bool IsSameAs_SameType(NavigationGoal other);

}

public enum NavigationGoalType {
    None,
    Checkpoint,
    Hoop,
    Finish,
    Bonus
}

public class EmptyGoal : NavigationGoal {
	public override NavigationGoalType Type => NavigationGoalType.None;
	public override Vector3 TargetPosition => this.agent.transform.position;


    public EmptyGoal(GameObject agent) : base(agent) { 
    }

	public override bool IsReached() {
        return false;
	}

	public override bool IsValid() {
        return true;
    }

    public override bool ShouldBeSkipped() {
        return false;
    }

    public override bool DetermineIfShouldFail() {
        return false;
    }

    public override float GetRationality() {
        // Not reasonable at all, should be replaced by another goal
        return 0;
	}

	protected override bool IsSameAs_SameType(NavigationGoal other) {
        return true; // two empty goals are always the same
	}
}

public abstract class TrackElementGoal : NavigationGoal {
    public int index;

    public TrackElementGoal(GameObject agent, int index) : base(agent) {
        this.index = index;
    }

	public override bool IsValid() {
        return true; // all hoops and checkpoints are valid, bonuses override this method
	}

	protected override bool IsSameAs_SameType(NavigationGoal other) {
        return this.index == (other as TrackElementGoal).index; // the same type and the same index of the element
	}
}

public class HoopGoal : TrackElementGoal {
    public TrackPoint trackPoint;

    public override NavigationGoalType Type => NavigationGoalType.Hoop;
	public override Vector3 TargetPosition => GetTargetPoint();

    protected float targetPositionMistakeOffset = 0f;
    protected Plane trackPointPlane;

	public HoopGoal(GameObject agent, int index) : base(agent, index) {
        this.trackPoint = RaceController.Instance.Level.Track[this.index];
        this.trackPointPlane = new Plane(this.trackPoint.assignedHoop.transform.forward, this.trackPoint.position);
    }

    public override bool IsReached() {
        return this.raceState.trackPointToPassNext > this.index;
    }

    public override bool ShouldBeSkipped() {
        // Based on Precision stat
        float mistakeProbability = agentSkillLevel.GetPrecisionMistakeProbability();
        float skipProbability = agentSkillLevel.mistakesParameters.HoopSkipCurve.Evaluate(mistakeProbability);
        return (UnityEngine.Random.value < skipProbability);
    }

    public override bool DetermineIfShouldFail() {
        // Based on Dexterity stat
        float mistakeProbability = agentSkillLevel.GetDexterityMistakeProbability();
        // Determine target point offset
        targetPositionMistakeOffset = agentSkillLevel.mistakesParameters.HoopMissCurve.Evaluate(mistakeProbability);
        return (targetPositionMistakeOffset == 0);
    }

    public override float GetRationality() {
        // It is reasonable only if it is the next hoop
        if (this.index != this.raceState.trackPointToPassNext)
            return 0;
        // And in a reasonable position relative to the agent
        if (Mathf.Abs(Vector3.SignedAngle(this.agent.transform.forward, RaceController.Instance.Level.Track[index].position - this.agent.transform.position, Vector3.up)) < 70f)
            return 1;
        else
            return 0;
    }

    private Vector3 GetTargetPoint() {
        // Aim off-center according to the current agent position and orientation

        // Find intersection of the agent's forward vector and the track point plane
        Ray agentForwardRay = new Ray(this.agent.transform.position, this.agent.transform.forward);
        float intersectionDistance;
        Vector3 localTarget;
        if (trackPointPlane.Raycast(agentForwardRay, out intersectionDistance)) {
            // There is an intersection
            localTarget = this.trackPoint.assignedHoop.transform.InverseTransformPoint(agentForwardRay.GetPoint(intersectionDistance)).WithZ(0);
        } else {
            // Either no intersection or opposite direction (https://docs.unity3d.com/ScriptReference/Plane.Raycast.html)
            if (intersectionDistance < 0) { // oppoiste direction
                // Take the intersection in local coordinates and invert it
                localTarget = this.trackPoint.assignedHoop.transform.InverseTransformPoint(agentForwardRay.GetPoint(intersectionDistance)).WithZ(0);
                localTarget *= -1f;
            } else { 
                // Use only the agent's position
                localTarget = this.trackPoint.assignedHoop.transform.InverseTransformPoint(agent.transform.position).WithZ(0);
            }
        }
        // Choose target point inside of the hoop
        Scalable hoopScalable = this.trackPoint.assignedHoop.GetComponent<Scalable>();
        float scale = hoopScalable == null ? 1 : hoopScalable.GetScale().x;
        float radius = 2 * scale;
        if (localTarget.magnitude > radius) localTarget = localTarget.normalized * 2 * scale;
        // Add offset based on mistake
        localTarget += localTarget.normalized * targetPositionMistakeOffset;
        return this.trackPoint.assignedHoop.transform.TransformPoint(localTarget);
    }
}

public class CheckpointGoal : HoopGoal {

    public override NavigationGoalType Type => NavigationGoalType.Checkpoint;

    public CheckpointGoal(GameObject agent, int index) : base(agent, index) {
    }

    public override bool ShouldBeSkipped() {
        // Checkpoint cannot be skipped
        return false;
    }

    public override bool DetermineIfShouldFail() {
        // Checkpoints are never missed (to make it easier)
        targetPositionMistakeOffset = 0f;
        return false;
    }

    public override float GetRationality() {
        // It is reasonable only if it is the next hoop
        if (this.index == this.raceState.trackPointToPassNext)
            return 1;
        else
            return 0;
    }
}

public class BonusGoal : TrackElementGoal {
    public BonusSpot bonusSpot;

    protected float targetPositionMistakeOffset = 0f;

    private int instanceIndex = -1;


    public override NavigationGoalType Type => NavigationGoalType.Bonus;
    public override Vector3 TargetPosition => GetTargetPoint();

    public BonusGoal(GameObject agent, int index) : base(agent, index) {
        // Choose the closest instance available
        this.bonusSpot = RaceController.Instance.Level.bonuses[this.index];
        ChooseRandomInstance();
    }

    public override bool IsReached() {
        // If the bonus instance is not available anymore
        return !bonusSpot.bonusInstances[instanceIndex].gameObject.activeInHierarchy;
    }

    public override bool IsValid() {
        // The current instance must be available
        if (instanceIndex != -1 && bonusSpot.bonusInstances[instanceIndex].gameObject.activeInHierarchy) return true;
        // Or at least one instance must be available
        ChooseRandomInstance();
        return (instanceIndex >= 0);
    }

    public override bool ShouldBeSkipped() {
        BonusEffect bonus = bonusSpot.bonusInstances[0];
        float mistakeProbability, skipProbability;
        // Based on the bonus type...
        // ... speed bonus - based on average of Speed and Precision stats
        if (bonus.GetType() == typeof(SpeedBonusEffect)) {
            mistakeProbability = (agentSkillLevel.GetSpeedMistakeProbability() + agentSkillLevel.GetPrecisionMistakeProbability()) / 2f;
            skipProbability = agentSkillLevel.mistakesParameters.SpeedBonusSkipCurve.Evaluate(mistakeProbability);
        }
        // ... mana bonus and spell recharge bonus - based on average of Magic and Precision stats
        else if (bonus.GetType() == typeof(ManaBonusEffect) || bonus.GetType() == typeof(RechargeSpellsBonusEffect)) {
            mistakeProbability = (agentSkillLevel.GetMagicMistakeProbability() + agentSkillLevel.GetPrecisionMistakeProbability()) / 2f;
            skipProbability = agentSkillLevel.mistakesParameters.SpellBonusSkipCurve.Evaluate(mistakeProbability);
        }
        // ... other - based on Precision stat
        else {
            mistakeProbability = agentSkillLevel.GetPrecisionMistakeProbability();
            skipProbability = agentSkillLevel.mistakesParameters.BonusSkipCurve.Evaluate(mistakeProbability);
        }
        return (UnityEngine.Random.value < skipProbability);
    }

    public override bool DetermineIfShouldFail() {
        // Based on Dexterity stat
        float mistakeProbability = agentSkillLevel.GetDexterityMistakeProbability();
        // Determine target point offset
        targetPositionMistakeOffset = agentSkillLevel.mistakesParameters.BonusMissCurve.Evaluate(mistakeProbability);
        return (targetPositionMistakeOffset == 0);
    }

    public override float GetRationality() {
        // Not rational if too far from the current direction of the agent
        float angleYaw = Vector3.SignedAngle(this.agent.transform.forward, bonusSpot.position - this.agent.transform.position, Vector3.up);
        if (bonusSpot.bonusInstances[0].GetType() == typeof(ManaBonusEffect)) {
            // Treat mana bonus differently
            SpellController spellController = this.agent.GetComponentInChildren<SpellController>();
            if (spellController.CurrentMana == spellController.MaxMana) return 0; // not rational if racer has max mana
            if (Mathf.Abs(angleYaw) <= 90 && ((float)spellController.CurrentMana / (float)spellController.MaxMana) < 0.3f) // tolerate even larger angle if low on mana
                return 1;
        }
        if (Mathf.Abs(angleYaw) <= 60f) return 1;
        else return 0;
    }

    private void ChooseRandomInstance() {
        this.instanceIndex = -1;
        // Count all available instances
        int availableInstanceCount = 0;
        for (int i = 0; i < this.bonusSpot.bonusInstances.Count; i++) {
            BonusEffect bonus = this.bonusSpot.bonusInstances[i];
            if (bonus.gameObject.activeInHierarchy) availableInstanceCount++;
        }
        // Select a random one
        int randomIndex = UnityEngine.Random.Range(0, availableInstanceCount);
        // Choose the instance index accordingly
        for (int i = 0; i < this.bonusSpot.bonusInstances.Count; i++) {
            BonusEffect bonus = this.bonusSpot.bonusInstances[i];
            if (bonus.gameObject.activeInHierarchy) {
                if (randomIndex == 0) { // the current instance was chosen
                    this.instanceIndex = i;
                    return;
                } else randomIndex--; // move to the next one
            }
        }
    }

    private Vector3 GetTargetPoint() {
        // Aim off-center based on mistake
        Vector3 offset = UnityEngine.Random.onUnitSphere * targetPositionMistakeOffset;
        return (this.bonusSpot.bonusInstances[instanceIndex].transform.position + offset);
    }
}

public class FinishNavigationGoal : NavigationGoal {
    public FinishLine finishObject;

    public override NavigationGoalType Type => NavigationGoalType.Finish; 
    public override Vector3 TargetPosition => GetTargetPoint();

    public FinishNavigationGoal(GameObject agent) : base(agent) {
        this.finishObject = RaceController.Instance.Level.finish;
    }

    public override bool IsReached() {
        return this.raceState.HasFinished;
    }

    public override bool IsValid() {
        return raceState.trackPointToPassNext >= raceState.hoopsPassedArray.Length;
    }

    public override bool ShouldBeSkipped() {
        // Finish cannot be skipped
        return false;
    }

    public override bool DetermineIfShouldFail() {
        // Finish cannot be failed
        return false;
    }

    public override float GetRationality() {
        // It is reasonable only if all hoops were passed/missed
        if (this.raceState.trackPointToPassNext >= this.raceState.hoopsPassedArray.Length)
            return 1;
        else
            return 0;
    }

    protected override bool IsSameAs_SameType(NavigationGoal other) {
        return true; // there is only one finish so it is always the same goal
    }

    private Vector3 GetTargetPoint() {
        Vector3 target = this.finishObject.transform.position;
        // Find the direction from the last hoop to the finish - this is the ideal direction to take to the finish
        Vector3 lastHoopPosition = RaceController.Instance.Level.Track[RaceController.Instance.Level.Track.Count - 1].position;
        Vector3 targetDirection = (target - lastHoopPosition).WithY(0);
        // Find intersection between this direction from the agent and the finish's right vector
        if (Utils.TryGetLineIntersectionXZ(
                this.agent.transform.position, this.agent.transform.position + targetDirection,
                this.finishObject.transform.position, this.finishObject.transform.position + this.finishObject.transform.right,
                out Vector3 betterTarget)) {
            target = betterTarget;
        }
        // Keep the agent't Y coordinate
        target.y = this.agent.transform.position.y;
        // Adjust Y based on terrain underneath
        float terrainHeight = RaceController.Instance.Level.GetNearestTerrainPoint(target).position.y;
        if (target.y < terrainHeight) target.y = terrainHeight + 2; // a bit higher, just to be safe
        return target;
    }
}
