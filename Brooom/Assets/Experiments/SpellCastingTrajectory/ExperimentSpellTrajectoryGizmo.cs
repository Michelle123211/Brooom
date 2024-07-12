using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentSpellTrajectoryGizmo : MonoBehaviour
{
    [SerializeField] Transform source;
    [SerializeField] Transform target;

    [SerializeField] float distance;

    [SerializeField] SpellTrajectoryPoint point;

    // Update is called once per frame
    void Update() {
    }

	private void OnDrawGizmos() {
        // Line between source and target
        Gizmos.color = Color.black;
        Gizmos.DrawLine(source.position, target.position);
        // Point on the trajectory
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, (target.position - source.position));
        Vector3 currentPosition = new Vector3(point.offset.x, point.offset.y, point.distanceFromStart);
        currentPosition = source.position + (rotation * currentPosition);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(currentPosition, 0.1f);
    }

}
