using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


/// <summary>
/// A component for an object which is moving along a path of points and is destroyed after some time upon reaching the end.
/// It is used by <c>NavigationBonusEffect</c> to highlight path between next few hoops, where the object also leaves trail behind.
/// </summary>
public class HighlightTrajectory : MonoBehaviour
{
    [Tooltip("How many units the object travels in a second.")]
    public float speed = 20f;

    private List<Vector3> points;

    /// <summary>
    /// Adds a point for the object to go through.
    /// </summary>
    /// <param name="point">Point to be added to the path.</param>
    public void AddTrajectoryPoint(Vector3 point) {
        points.Add(point);
    }

    /// <summary>
    /// Starts moving the object along a defined path. When the object reaches the end, it is destroyed after a while.
    /// </summary>
	public void Play() {
        // Tween the position between the points
        Sequence movementSequence = DOTween.Sequence();
        for (int i = 1; i < points.Count; i++) {
            float duration = Vector3.Distance(points[i - 1], points[i]) / speed;
            movementSequence.Append(transform.DOMove(points[i], duration)).SetEase(Ease.OutCubic);
        }
        // Destroy the object after some time
        movementSequence.OnComplete(() => Invoke(nameof(DestroySelf), 7f));
	}

    private void DestroySelf() {
        Destroy(gameObject);
    }

	// Start is called before the first frame update
	void Awake()
    {
        points = new List<Vector3>();
        points.Add(transform.position);
    }
}
