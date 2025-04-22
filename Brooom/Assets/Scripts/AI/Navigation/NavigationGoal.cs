using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A base class for a possible navigation goal in a goal-oriented navigation AI.
/// </summary>
public abstract class NavigationGoal {

    /// <summary>Type of the navigation goal.</summary>
    public abstract NavigationGoalType Type { get; }
    /// <summary>Target position based on the navigation goal.</summary>
    public abstract Vector3 TargetPosition { get; }

    /// <summary>Agent for whom this navigation goal is created.</summary>
    protected GameObject agent;
    /// <summary>Skill level of the agent whose navigation goal this is (allowing it to adjust to player in a certain way).</summary>
    protected AISkillLevel agentSkillLevel;
    /// <summary>Race state of the agent whose navigation goal this is.</summary>
    protected CharacterRaceState raceState;

    /// <summary>If an agent is at most at this distance from the goal, the goal is considered reached.</summary>
    protected const float REACHED_DISTANCE_THRESHOLD = 0.5f;

    public NavigationGoal(GameObject agent) {
        this.agent = agent;
        this.agentSkillLevel = agent.GetComponentInChildren<AISkillLevel>();
        this.raceState = agent.GetComponentInChildren<CharacterRaceState>();
    }

    /// <summary>
    /// Determines if the agent whose goal this is has reached this goal.
    /// </summary>
    /// <returns><c>true</c> if reached, <c>false</c> oterhwise.</returns>
    public abstract bool IsReached();
    /// <summary>
    /// Determines if this navigation goal is still valid for the agent.
    /// </summary>
    /// <returns><c>true</c> if valid, <c>false</c> oterhwise.</returns>
    public abstract bool IsValid();

    /// <summary>
    /// Determines if this navigation goal should be skipped. This is decided based on the agent's skill level.
    /// </summary>
    /// <returns><c>true</c> if should be skipped, <c>false</c> oterhwise.</returns>
    public abstract bool ShouldBeSkipped();

    /// <summary>
    /// Determines if this navigation goal should be pursued but failed. This is decided based on the agent's skill level.
    /// If it should fail, the target position is adjusted accordingly (to miss the goal if necessary).
    /// </summary>
    /// <returns><c>true</c> if should be failed, <c>false</c> oterhwise.</returns>
    public abstract bool DetermineIfShouldFail();

    /// <summary>
    /// Computes a value denoting how rational it is for the agent to pursue this navigation goal based on current situation. 
    /// </summary>
    /// <returns>Number between 0 (not rational) and 1 (rational).</returns>
    public abstract float GetRationality();

    /// <summary>
    /// Compares this navigation goal to the given one and determines if they represent the same goal.
    /// </summary>
    /// <param name="other">Other navigation goal to compare this one to.</param>
    /// <returns><c>true</c> of both represent the same goal, <c>false</c> otherwise.</returns>
    public bool IsSameAs(NavigationGoal other) {
        if (other == null) return false;
        // Check if it is the same instance
        if (System.Object.ReferenceEquals(this, other)) return true;
        // Check runtime types
        if (this.GetType() != other.GetType() || this.Type != other.Type) return false;
        // Type-specific comparison (e.g. based on fields)
        return IsSameAs_SameType(other);
    }

    /// <summary>
    /// Compares this navigation goal to the given one and determines if they represent the same goal.
    /// Already presuming both navigation goals are of the same type.
    /// </summary>
    /// <param name="other">Other navigation goal to compare this one to.</param>
    /// <returns><c>true</c> of both represent the same goal, <c>false</c> otherwise.</returns>
    protected abstract bool IsSameAs_SameType(NavigationGoal other);

}

/// <summary>
/// Possible types of navigation goals in goal-oriented navigation.
/// </summary>
public enum NavigationGoalType {
    None,
    Checkpoint,
    Hoop,
    Finish,
    Bonus
}

/// <summary>
/// A class representing an empty navigation goal, which is always valid and never reached, skipped or failed.
/// Can be used when the race has already finished and there are no more goals to be navigated to.
/// </summary>
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
        // Not reasonable at all, should be replaced by another goal if possible
        return 0;
	}

	protected override bool IsSameAs_SameType(NavigationGoal other) {
        return true; // two empty goals are always the same
	}
}

/// <summary>
/// A base class representing navigation goal for a track element (e.g., hoop, bonus).
/// </summary>
public abstract class TrackElementGoal : NavigationGoal {

    /// <summary>Element's index within the track.</summary>
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

/// <summary>
/// A class representing navigation goal for flying through a hoop.
/// </summary>
public class HoopGoal : TrackElementGoal {

    /// <summary><c>TrackPoint</c> representing the related hoop in the track.</summary>
    public TrackPoint trackPoint;

    public override NavigationGoalType Type => NavigationGoalType.Hoop;
	public override Vector3 TargetPosition => GetTargetPoint();

    /// <summary>Offset from the original target position based on agent's skill level.</summary>
    protected float targetPositionMistakeOffset = 0f;
    /// <summary>Plane determined by the hoop's position and orientation.</summary>
    protected Plane trackPointPlane;

	public HoopGoal(GameObject agent, int index) : base(agent, index) {
        this.trackPoint = RaceControllerBase.Instance.Level.Track[this.index];
        this.trackPointPlane = new Plane(this.trackPoint.assignedHoop.transform.forward, this.trackPoint.position);
    }

    public override bool IsReached() {
        // Based on agent's race state, which hoop is next
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
        // Determine target point offset (from the hoop's centre)
        targetPositionMistakeOffset = agentSkillLevel.mistakesParameters.HoopMissCurve.Evaluate(mistakeProbability);
        return (targetPositionMistakeOffset == 0);
    }

    public override float GetRationality() {
        // It is reasonable only if it is the next hoop
        if (this.index != this.raceState.trackPointToPassNext)
            return 0;
        // And in a reasonable position relative to the agent
        if (Mathf.Abs(Vector3.SignedAngle(this.agent.transform.forward, RaceControllerBase.Instance.Level.Track[index].position - this.agent.transform.position, Vector3.up)) < 70f)
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
            if (intersectionDistance < 0) { // opposite direction
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

/// <summary>
/// A class representing navigation goal for flying through a checkpoint.
/// </summary>
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

/// <summary>
/// A class representing navigation goal for picking up a bonus.
/// </summary>
public class BonusGoal : TrackElementGoal {

    /// <summary><c>BonusSpot</c> representing the related bonus in the track.</summary>
    public BonusSpot bonusSpot;

    /// <summary>Offset from the original target position based on agent's skill level.</summary>
    protected float targetPositionMistakeOffset = 0f;

    /// <summary>Index of a particular bonus instance in a single bonus spot.</summary>
    private int instanceIndex = -1;


    public override NavigationGoalType Type => NavigationGoalType.Bonus;
    public override Vector3 TargetPosition => GetTargetPoint();

    public BonusGoal(GameObject agent, int index) : base(agent, index) {
        // Choose a random instance available
        this.bonusSpot = RaceControllerBase.Instance.Level.bonuses[this.index];
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
        // Determine target point offset (from bonus' centre)
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
        for (int i = 0; i < this.bonusSpot.bonusInstances.Count; i++)
            if (this.bonusSpot.bonusInstances[i].gameObject.activeInHierarchy) availableInstanceCount++;
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

/// <summary>
/// A class representing navigation goal for flying through a finish line.
/// </summary>
public class FinishNavigationGoal : NavigationGoal {

    /// <summary>A <c>FinishLine</c> object placed in the level.</summary>
    public FinishLine finishObject;

    public override NavigationGoalType Type => NavigationGoalType.Finish; 
    public override Vector3 TargetPosition => GetTargetPoint();

    public FinishNavigationGoal(GameObject agent) : base(agent) {
        this.finishObject = RaceControllerBase.Instance.Level.finish;
    }

    public override bool IsReached() {
        // If the agent has finished the race
        return this.raceState.HasFinished;
    }

    public override bool IsValid() {
        // If there are no remaining hoops/checkpoints to fly through
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
        // It is reasonable only if there are no hoops/checkpoints remaining to fly through
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
        Vector3 lastHoopPosition = RaceControllerBase.Instance.Level.Track[RaceControllerBase.Instance.Level.Track.Count - 1].position;
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
        float terrainHeight = RaceControllerBase.Instance.Level.GetNearestTerrainPoint(target).position.y;
        if (target.y < terrainHeight) target.y = terrainHeight + 2; // a bit higher, just to be safe
        return target;
    }
}
