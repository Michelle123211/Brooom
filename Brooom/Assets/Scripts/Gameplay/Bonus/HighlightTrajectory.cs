using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HighlightTrajectory : MonoBehaviour
{
    [Tooltip("How many units the object travels in a second.")]
    public float speed = 20f;

    private List<Vector3> points;

    public void AddTrajectoryPoint(Vector3 point) {
        points.Add(point);
    }

	public void Play() {
        // Tween the position between the points
        Sequence movementSequence = DOTween.Sequence();
        for (int i = 1; i < points.Count; i++) {
            float duration = Vector3.Distance(points[i - 1], points[i]) / speed;
            movementSequence.Append(transform.DOMove(points[i], duration));
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
