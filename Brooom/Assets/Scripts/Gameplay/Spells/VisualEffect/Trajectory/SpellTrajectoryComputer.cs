using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A base class computing points on a spell's trajectory when being cast and moving towards its target.
/// It starts with a linear trajectory between the starting point and the target, 
/// then in each moment it offsets the reference point from this direct trajectory to create more interesting one.
/// Different derived classes implement different trajectory shapes.
/// </summary>
public abstract class SpellTrajectoryComputer : MonoBehaviour {

    /// <summary>
    /// Computes point on the spell's trajectory towards the target as an offset from a point on the linear trajectory between the start and the target.
    /// </summary>
    /// <param name="distanceFromStart">Distance of the reference point (on linear trajectory between the start and target) from the start.</param>
    /// <returns>A point on the spell's trajectory as an offset from the linear trajectory.</returns>
    public abstract SpellTrajectoryPoint GetNextTrajectoryPoint(float distanceFromStart);

    /// <summary>
    /// Resets the trajectory computer into its initial values.
    /// </summary>
    public abstract void ResetTrajectory();
}

/// <summary>
/// A struct describing a point on a spell's trajectory as an offset from the direct linear trajectory between start and target.
/// </summary>
[System.Serializable]
public struct SpellTrajectoryPoint {
    [Tooltip("How far from start on the linear trajectory the reference point is (this distance is used as a parameter to compute the offset).")]
    public float distanceFromStart;
    [Tooltip("What is the offset from the direct trajectory between start and target.")]
    public Vector2 offset;
}