using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpellTrajectoryComputer : MonoBehaviour {
    public abstract SpellTrajectoryPoint GetNextTrajectoryPoint(float distanceFromStart);

    public abstract void ResetTrajectory();
}

[System.Serializable]
public struct SpellTrajectoryPoint {
    // How far from start the point is (this distance was used as a parameter to compute the offset)
    public float distanceFromStart;
    // What is the offset from the direct trajectory between
    public Vector2 offset;
}