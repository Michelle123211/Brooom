using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class NavigationGoal {
    public NavigationGoalType type;
    public Vector3 targetPosition;

    public abstract NavigationGoalType Type { get; }

    protected GameObject agent;

    // If the agent is at most at this distance from the goal, the goal is considered reached
    protected const float REACHED_DISTANCE_THRESHOLD = 0.5f; // TODO: Change this to a reasonable value

    public NavigationGoal(GameObject agent) {
        this.agent = agent;
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

    public override NavigationGoalType Type { get => NavigationGoalType.Hoop; }

    public HoopGoal(GameObject agent, int index) : base(agent, index) {
        this.trackPoint = RaceController.Instance.level.track[index];
        this.targetPosition = this.trackPoint.position;
    }

    public override bool IsReached() {
        // TODO
        throw new System.NotImplementedException();
    }
}

public class CheckpointGoal : HoopGoal {

    public override NavigationGoalType Type { get => NavigationGoalType.Checkpoint; }

    public CheckpointGoal(GameObject agent, int index) : base(agent, index) {
    }

    public override bool IsReached() {
        // TODO
        throw new System.NotImplementedException();
    }
}

public class BonusGoal : TrackElementGoal {
    public BonusSpot bonusSpot;

    private int instanceIndex = 0;
    private float currentDistance = float.MaxValue;


    public override NavigationGoalType Type { get => NavigationGoalType.Bonus; }

    public BonusGoal(GameObject agent, int index) : base(agent, index) {
        this.bonusSpot = RaceController.Instance.level.bonuses[index];
        // Choose the closest instance available
        ChooseClosestInstance();
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
                this.targetPosition = bonus.transform.position;
                this.instanceIndex = i;
            }
        }
    }
}

public class FinishNavigationGoal : NavigationGoal {
    public FinishLine finishObject;

    public override NavigationGoalType Type { get => NavigationGoalType.Finish; }

    public FinishNavigationGoal(GameObject agent, FinishLine finish) : base(agent) {
        this.finishObject = finish;
    }

    public override bool IsReached() {
        // TODO
        throw new System.NotImplementedException();
    }

    public override bool IsValid() {
        // TODO: Not valid if some checkpoints were missed
        throw new System.NotImplementedException();
    }
}
