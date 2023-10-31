using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class NavigationGoal {

    public abstract NavigationGoalType Type { get; }
    public abstract Vector3 TargetPosition { get; }

    protected GameObject agent;
    protected CharacterRaceState raceState;

    // If the agent is at most at this distance from the goal, the goal is considered reached
    protected const float REACHED_DISTANCE_THRESHOLD = 0.5f; // TODO: Change this to a reasonable value

    public NavigationGoal(GameObject agent) {
        this.agent = agent;
        this.raceState = agent.GetComponentInChildren<CharacterRaceState>();
    }

    public abstract bool IsReached();
    public abstract bool IsValid();

    public abstract float GetRationality();
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

	public override float GetRationality() {
        // Not reasonable at all, should be replaced by another goal
        return 0;
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
}

public class HoopGoal : TrackElementGoal {
    public TrackPoint trackPoint;

    public override NavigationGoalType Type => NavigationGoalType.Hoop;
	public override Vector3 TargetPosition => GetTargetPoint();

	public HoopGoal(GameObject agent, int index) : base(agent, index) {
        this.trackPoint = RaceController.Instance.level.track[this.index];
    }

    public override bool IsReached() {
        return this.raceState.trackPointToPassNext > this.index;
    }

    private Vector3 GetTargetPoint() {
        // Aim off-center according to the current agent position
        Vector3 agentPosition = this.trackPoint.assignedHoop.transform.InverseTransformPoint(agent.transform.position).WithZ(0);
        Scalable hoopScalable = this.trackPoint.assignedHoop.GetComponent<Scalable>();
        float scale = hoopScalable == null ? 1 : hoopScalable.GetScale().x;
        float radius = 2 * scale;
        Vector3 localTarget;
        if (agentPosition.magnitude > radius) localTarget = agentPosition.normalized * 2 * scale;
        else localTarget = agentPosition;
        return this.trackPoint.assignedHoop.transform.TransformPoint(localTarget);
    }

    public override float GetRationality() {
        // It is reasonable only if it is the next hoop
        if (this.index == this.raceState.trackPointToPassNext)
            return 1;
        else
            return 0;
    }
}

public class CheckpointGoal : HoopGoal {

    public override NavigationGoalType Type => NavigationGoalType.Checkpoint;

    public CheckpointGoal(GameObject agent, int index) : base(agent, index) {
    }
}

public class BonusGoal : TrackElementGoal {
    public BonusSpot bonusSpot;

    private int instanceIndex = 0;
    private float currentDistance = float.MaxValue;


    public override NavigationGoalType Type => NavigationGoalType.Bonus;
    public override Vector3 TargetPosition => this.bonusSpot.bonusInstances[instanceIndex].transform.position;

    public BonusGoal(GameObject agent, int index) : base(agent, index) {
        // Choose the closest instance available
        this.bonusSpot = RaceController.Instance.level.bonuses[this.index];
        ChooseClosestInstance();
    }

    public override bool IsReached() {
        // If the bonus instance is not available anymore
        return !bonusSpot.bonusInstances[instanceIndex].gameObject.activeInHierarchy;
    }

    public override bool IsValid() {
        // At least one instance must be available
        ChooseClosestInstance();
        return (instanceIndex >= 0);
    }

    public override float GetRationality() {
        // Not rational if behind the agent
        float angleYaw = Vector3.SignedAngle(this.agent.transform.forward, bonusSpot.position - this.agent.transform.position, Vector3.up);
        if (angleYaw > 90f) return 0;
        else return 1;
    }

    private void ChooseClosestInstance() {
        // Closest available instance of the bonus
        this.instanceIndex = -1;
        this.currentDistance = float.MaxValue;
        for (int i = 0; i < this.bonusSpot.bonusInstances.Count; i++) {
            BonusEffect bonus = this.bonusSpot.bonusInstances[i];
            if (!bonus.gameObject.activeInHierarchy) continue;
            float distance = Vector3.Distance(agent.transform.position, bonus.transform.position);
            if (distance < this.currentDistance) {
                this.currentDistance = distance;
                this.instanceIndex = i;
            }
        }
    }
}

public class FinishNavigationGoal : NavigationGoal {
    public FinishLine finishObject;

    public override NavigationGoalType Type => NavigationGoalType.Finish; 
    public override Vector3 TargetPosition => GetTargetPoint();

    public FinishNavigationGoal(GameObject agent) : base(agent) {
        this.finishObject = RaceController.Instance.level.finish;
    }

    public override bool IsReached() {
        return this.raceState.HasFinished;
    }

    public override bool IsValid() {
        return raceState.trackPointToPassNext >= raceState.hoopsPassedArray.Length;
    }

    public override float GetRationality() {
        // It is reasonable only if all hoops were passed/missed
        if (this.raceState.trackPointToPassNext >= this.raceState.hoopsPassedArray.Length)
            return 1;
        else
            return 0;
    }

    private Vector3 GetTargetPoint() {
        Vector3 target = this.finishObject.transform.position;
        // Find the direction from the last hoop to the finish - this is the ideal direction to take to the finish
        Vector3 lastHoopPosition = RaceController.Instance.level.track[RaceController.Instance.level.track.Count - 1].position;
        Vector3 targetDirection = (target - lastHoopPosition).WithY(0);
        // Find intersection between this direction from the agent and the finish's right vector
        if (Utils.TryGetLineIntersectionXZ(
            this.agent.transform.position, this.agent.transform.position + targetDirection,
            this.finishObject.transform.position, this.finishObject.transform.position + this.finishObject.transform.right,
            out Vector3 betterTarget)) {
            target = betterTarget;
        }
        // Keep the agent't Y coordinate
        return target.WithY(this.agent.transform.position.y);
    }
}
