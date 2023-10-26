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
}

public enum NavigationGoalType {
    Checkpoint,
    Hoop,
    Finish,
    Bonus
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
        return raceState.trackPointToPassNext > this.index;
    }

    private Vector3 GetTargetPoint() {
        // Aim off-center according to the current agent position
        Vector3 target = this.trackPoint.position;
        target += (agent.transform.position - target).WithZ(0).normalized * 2;
        return target;
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
        ChooseClosestInstance();
        this.bonusSpot = RaceController.Instance.level.bonuses[this.index];
    }

    public override bool IsReached() {
        // Simply based on distance (we have no better information)
        return (currentDistance < REACHED_DISTANCE_THRESHOLD);
    }

    public override bool IsValid() {
        // At least one instance must be available
        ChooseClosestInstance();
        return (instanceIndex >= 0);
    }

    private void ChooseClosestInstance() {
        // Closest available instance of the bonus
        this.instanceIndex = -1;
        this.currentDistance = float.MaxValue;
        for (int i = 0; i < this.bonusSpot.bonusInstances.Count; i++) {
            BonusEffect bonus = this.bonusSpot.bonusInstances[i];
            if (!bonus.isActiveAndEnabled) continue;
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
    public override Vector3 TargetPosition => this.finishObject.transform.position;

    public FinishNavigationGoal(GameObject agent) : base(agent) {
        this.finishObject = RaceController.Instance.level.finish;
    }

    public override bool IsReached() {
        return this.raceState.HasFinished;
    }

    public override bool IsValid() {
        return raceState.trackPointToPassNext >= raceState.hoopsPassedArray.Length;
    }
}
