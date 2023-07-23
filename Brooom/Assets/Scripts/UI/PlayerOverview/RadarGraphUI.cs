using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(UIPathRenderer))]
public class RadarGraphUI : MonoBehaviour
{
    [Header("Parameters")]
    [Tooltip("Radius of the radar graph.")]
    public float radius = 145f;
    [Tooltip("Number of parameters depicted by the graph.")]
    public int parametersCount = 5;
    [Tooltip("Color used for the main lines in the graph.")]
    public Color mainColor = Color.black;
    [Tooltip("Thickness of the main lines in the graph.")]
    public float mainThickness = 4f;

    [Header("Secondary lines")]
    [Tooltip("Number of secondary lines in the background of the graph (excluding the main outline).")]
    public int secondaryLinesCount = 3;
    [Tooltip("Color used for the secondary lines in the graph.")]
    public Color secondaryColor = Color.gray;
    [Tooltip("Thickness of the secondary lines in the graph.")]
    public float secondaryThickness = 2f;

    private List<Vector3> axes = new List<Vector3>();

    void Start() {
        // Compute axes
        float angleIncrement = -360f / parametersCount; // clockwise with minus
        for (int i = 0; i < parametersCount; i++) {
            float angle = i * angleIncrement;
            axes.Add(Quaternion.Euler(0, 0, angle) * Vector2.up * radius);
        }

        UIPathRenderer lineRenderer = GetComponent<UIPathRenderer>();
        // Draw secondary lines
        for (int i = 1; i <= secondaryLinesCount; i++) {
            List<Vector3> path = new List<Vector3>();
            foreach (var axis in axes)
                path.Add(axis * i / (secondaryLinesCount + 1));
            lineRenderer.AddPath(path, secondaryColor, secondaryThickness, true);
        }
        // Draw axes
        foreach (var axis in axes)
            lineRenderer.AddPath(new List<Vector3> { Vector3.zero, axis }, mainColor, mainThickness);
        // Draw main line
        lineRenderer.AddPath(axes, mainColor, mainThickness, true);
    }
}
