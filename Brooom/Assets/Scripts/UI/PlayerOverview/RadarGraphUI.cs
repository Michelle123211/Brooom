using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component responsible for drawing a radar graph.
/// It uses <c>UIPathRenderer</c> for drawing main and secondary lines, <c>RadarGraphPolygonUI</c> for polygons and <c>RadarGraphLabelUI</c> for axes' labels.
/// </summary>
[RequireComponent(typeof(UIPathRenderer))]
public class RadarGraphUI : MonoBehaviour {

    [Header("Parameters")]
    [Tooltip("Radius of the radar graph.")]
    [SerializeField] float radius = 145f;
    [Tooltip("Number of parameters depicted by the graph.")]
    public int parametersCount = 5;
    [Tooltip("Maximum value on all axes.")]
    [SerializeField] float maxValue = 1;
    [Tooltip("Labels of the graph axes.")]
    [SerializeField] List<string> graphAxisLabels = new();
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

    [Header("Labels")]
    [Tooltip("A Transform which is a parent of all the radar graph labels.")]
    [SerializeField] Transform graphLabelsParent;
    [Tooltip("A prefab of a radar graph label placed at the end of each axis.")]
    [SerializeField] RadarGraphLabelUI graphLabelPrefab;
    [Tooltip("The offset (percentage in interval [0, 1]) denotes how much further along the axis tha label is placed.")]
    [SerializeField] float labelsOffset = 0.15f;

    [Header("Polygons")]
    [Tooltip("A Transform which is a parent of all the radar graph polygons.")]
    [SerializeField] Transform polygonsParent;
    [Tooltip("A prefab of a radar graph polygon which is instantiated for each polygon added to the graph.")]
    [SerializeField] RadarGraphPolygonUI polygonPrefab;
    [Tooltip("A polygon may be tweened to grow into its final size. This parameter sets duration of such tween.")]
    [SerializeField] float polygonTweenDuration = 0.5f;

    private List<Vector3> axes = new(); // uniformly placed in a circle, based on number of parameters
    private List<RadarGraphPolygonUI> polygons = new();
    private List<RadarGraphLabelUI> labels = new();

    private bool isInitialized = false;

    // Current polygon settings
    private bool colorChanged = false; // whether a custom fill color was set, or the default one should be used
    private Color fillColor;
    private bool hasBorder;
    private Color borderColor;
    private float borderThickness;

    /// <summary>
    /// Sets color which will be used from now on when drawing polygons.
    /// </summary>
    /// <param name="fillColor">Color which will be used to fill the polygon's area.</param>
    public void SetPolygonColor(Color fillColor) {
        this.colorChanged = true;
        this.fillColor = fillColor;
    }

    /// <summary>
    /// Sets parameters of polygon's border which will be used from now on when drawing polygons.
    /// </summary>
    /// <param name="hasBorder"><c>true</c> if the polygon's border should be rendered, <c>false</c> otherwise.</param>
    /// <param name="borderColor">Color which will be used to draw the polygon's border.</param>
    /// <param name="borderThickness">Thickness of the border.</param>
    public void SetPolygonBorder(bool hasBorder, Color borderColor = default, float borderThickness = 2) {
        this.hasBorder = hasBorder;
        if (hasBorder) {
            this.borderColor = borderColor;
            this.borderThickness = borderThickness;
        }
    }

    /// <summary>
    /// Draws a radar graph polygon from the given parameter values.
    /// </summary>
    /// <param name="values">A list of values, one for each graph parameter (between 0 and 1).</param>
    public void DrawGraphValues(List<float> values) {
        List<Vector3> points = GetPolygonPointsFromValues(values);
        InstantiatePolygon(points);
    }

    // Will be tweened from zero if initial values are not provided
    /// <summary>
    /// Draws a radar graph polygon from initial values, and tweens it so that it transforms into polygon from target values over time.
    /// </summary>
    /// <param name="values">A list of target values, one for each graph parameter (between 0 and 1).</param>
    /// <param name="initialValues">A list of initial values, one for each graph parameter (between 0 and 1).</param>
    /// <param name="showChangeInLabels"><c>true</c> if labels with changes in values should be displayed, <c>false</c> otherwise.</param>
    /// <param name="numberFormat">Format in which the values and changes should be displayed.</param>
    public void DrawGraphValuesTweened(List<float> values, List<float> initialValues = null, bool showChangeInLabels = false, string numberFormat = "N1") {
        List<Vector3> points = GetPolygonPointsFromValues(values);
        List<Vector3> initialPoints = GetPolygonPointsFromValues(initialValues);
        if (showChangeInLabels) {
            for (int i = 0; i < parametersCount; i++)
                labels[i].SetValueChange(values[i], values[i] - initialValues[i], polygonTweenDuration, numberFormat);
        }
        InstantiatePolygon(points, true, initialPoints);
    }

    /// <summary>
    /// Initializes the radar graph based on the given parameters. Axes are computed, primary and secondary lines are drawn, labels with tooltips are added.
    /// </summary>
    /// <param name="parametersCount">Number of parameters in the radar graph (i.e. number of axes).</param>
    /// <param name="maxValue">Maximum value on all axes.</param>
    /// <param name="axisLabels">A list of axes' labels which are displayed at the end of each axis.</param>
    /// <param name="axisTooltipDescriptions">A list of axes' descriptions which are displayed in tooltips.</param>
    public void Initialize(int parametersCount = -1, float maxValue = -1, List<string> axisLabels = null, List<string> axisTooltipDescriptions = null) {
        // Set parameters
        if (parametersCount > 0) this.parametersCount = parametersCount;
        if (maxValue > 0) this.maxValue = maxValue;
        if (axisLabels != null) this.graphAxisLabels = axisLabels;
        if (this.graphAxisLabels == null) this.graphAxisLabels = new List<string>();
        while (this.graphAxisLabels.Count < this.parametersCount) this.graphAxisLabels.Add(string.Empty);
        // Compute axes
        float angleIncrement = -360f / this.parametersCount; // clockwise with minus
        for (int i = 0; i < this.parametersCount; i++) {
            float angle = i * angleIncrement;
			axes.Add(Quaternion.Euler(0, 0, angle) * Vector2.up * radius);
        }
        // Draw background lines
        DrawBackgroundLines();
        // Add labels
        AddGraphAxisLabels(axisTooltipDescriptions);

        isInitialized = true;
    }

    /// <summary>
    /// Completely resets the radar graph, deleting all lines, polygons and labels.
    /// </summary>
    public void ResetGraph() {
        // Delete background lines
        UIPathRenderer lineRenderer = GetComponent<UIPathRenderer>();
        lineRenderer.ResetAll();
        // Delete polygons and labels
        ResetValues();
        ResetLabels();
    }

    /// <summary>
    /// Completely deletes all radar graph polygons.
    /// </summary>
    public void ResetValues() {
        // Delete all polygons
        UtilsMonoBehaviour.RemoveAllChildrenOfType<RadarGraphPolygonUI>(polygonsParent);
        polygons.Clear();
    }

    /// <summary>
    /// Completely deletes all axes' labels.
    /// </summary>
    public void ResetLabels() {
        // Delete all labels
        foreach (var label in graphLabelsParent.GetComponentsInChildren<RadarGraphLabelUI>())
            label.DOComplete();
        UtilsMonoBehaviour.RemoveAllChildrenOfType<RadarGraphLabelUI>(graphLabelsParent);
        labels.Clear();
    }

    // Draws primary and secondary lines in the rada graph's background
    private void DrawBackgroundLines() {
        UIPathRenderer lineRenderer = GetComponent<UIPathRenderer>();
        // Draw secondary lines
        for (int i = 1; i <= secondaryLinesCount; i++) {
            List<Vector3> path = new();
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

    // Instantiates and initializes labels at the end of each axis
    private void AddGraphAxisLabels(List<string> axisDescriptions) {
        if (graphLabelsParent == null) {
            Debug.LogWarning("Parent Transform for the radar graph labels was not set. The graph's Transform is used instead.");
            graphLabelsParent = transform;
        }
        ResetLabels();
        for (int i = 0; i < parametersCount; i++) {
            RadarGraphLabelUI label = Instantiate<RadarGraphLabelUI>(graphLabelPrefab, graphLabelsParent);
            label.GetComponent<RectTransform>().anchoredPosition = axes[i] * (1f + labelsOffset);
            if (axisDescriptions == null || i >= axisDescriptions.Count)
                label.Initialize(graphAxisLabels[i]);
            else
                label.Initialize(graphAxisLabels[i], axisDescriptions[i]); // Add axis description into a tooltip
            labels.Add(label);
        }
    }

    // Instantiates a new polygon with the given parameters
    private void InstantiatePolygon(List<Vector3> points, bool isTweened = false, List<Vector3> initialPoints = null) {
        if (polygonsParent == null) {
            Debug.LogWarning("Parent Transform for the radar graph polygons was not set. The graph's Transform is used instead.");
            polygonsParent = transform;
        }
        RadarGraphPolygonUI polygon = Instantiate<RadarGraphPolygonUI>(polygonPrefab, polygonsParent);
        polygons.Add(polygon);
        if (!isTweened) {
            if (colorChanged) polygon.DrawPolygon(points, fillColor, hasBorder, borderColor, borderThickness);
            else polygon.DrawPolygon(points, hasBorder);
        } else {
            if (colorChanged) polygon.DrawAndTweenPolygon(points, initialPoints, polygonTweenDuration,fillColor, hasBorder, borderColor, borderThickness);
            else polygon.DrawAndTweenPolygon(points, initialPoints, polygonTweenDuration, hasBorder);
        }
    }

    // Ensures there is enough values (adds 0s) for the parameters
    private List<float> ResolveNumberOfValues(List<float> values) {
        // Warnings
        if (values != null) {
            if (values.Count < parametersCount) {
                Debug.LogWarning("Not enough values provided for the graph. The rest will be filled with 0s.");
            }
            if (values.Count > parametersCount)
                Debug.LogWarning("Too many values provided for the graph (more than there are parameters).");
        }
        // Ensure minimum count
        if (values != null && values.Count >= parametersCount) return values;
        List<float> newValues = new();
        for (int i = 0; i < parametersCount; i++) {
            if (values != null && i < values.Count) newValues.Add(values[i]);
            else newValues.Add(0f);
        }
        return newValues;
    }

    // Computes actual points from values and radius
    private List<Vector3> GetPolygonPointsFromValues(List<float> values) {
        if (!isInitialized) Initialize();
        // Handle special cases
        if (values == null) values = new List<float>();
        values = ResolveNumberOfValues(values);
        // Compute actual points from values and radius
        List<Vector3> points = new();
        for (int i = 0; i < parametersCount; i++) {
            points.Add(axes[i] * Mathf.Clamp(values[i], 0, maxValue) / maxValue);
        }
        return points;
    }

}
