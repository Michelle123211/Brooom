using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIPathRenderer))]
public class RadarGraphUI : MonoBehaviour
{
    [Header("Parameters")]
    [Tooltip("Radius of the radar graph.")]
    [SerializeField] float radius = 145f;
    [Tooltip("Number of parameters depicted by the graph.")]
    public int parametersCount = 5;
    [Tooltip("Color used for the main lines in the graph.")]
    [SerializeField] Color mainColor = Color.black;
    [Tooltip("Thickness of the main lines in the graph.")]
    [SerializeField] float mainThickness = 4f;

    [Header("Secondary lines")]
    [Tooltip("Number of secondary lines in the background of the graph (excluding the main outline).")]
    [SerializeField] int secondaryLinesCount = 3;
    [Tooltip("Color used for the secondary lines in the graph.")]
    [SerializeField] Color secondaryColor = Color.gray;
    [Tooltip("Thickness of the secondary lines in the graph.")]
    [SerializeField] float secondaryThickness = 2f;

    [Header("Polygons")]
    [Tooltip("A prefab of a radar graph polygon which is instantiated for each polygon added to the graph.")]
    [SerializeField] RadarGraphPolygonUI polygonPrefab;

    private List<Vector3> axes = new List<Vector3>();
    private List<RadarGraphPolygonUI> polygons = new List<RadarGraphPolygonUI>();

    private bool isInitialized = false;

    // Values should be from interval [0, 1]
    public void DrawGraphValues(List<float> values) {
        if (!isInitialized) Initialize();
        // Handle special cases
        if (values.Count < parametersCount) {
            Debug.LogWarning("Not enough values provided for the graph. The rest will be taken as 0.");
            for (int i = 0; i < parametersCount - values.Count; i++)
                values.Add(0f);
        }
        if (values.Count > parametersCount) Debug.LogWarning("Too many values provided for the graph (more then there are parameters).");
        // Compute actual points from values and radius
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < parametersCount; i++) {
            points.Add(axes[i] * Mathf.Clamp(values[i], 0, 1));
        }
        // Instantiate a new polygon
        RadarGraphPolygonUI polygon = Instantiate<RadarGraphPolygonUI>(polygonPrefab, transform);
        polygons.Add(polygon);
        polygon.DrawPolygon(points, true);
    }

    public void ResetGraph() {
        // Delete all polygons
        for (int i = polygons.Count - 1; i >= 0; i--) {
            Destroy(polygons[i].gameObject);
        }
    }

	void Initialize() {
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
