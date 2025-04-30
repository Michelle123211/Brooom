using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


/// <summary>
/// A component representing a single polygon of a radar graph (used from <c>RadarGraphUI</c>).
/// It uses <c>UIGraphPolygonRenderer</c> to draw a filled polygon and <c>UIPathRenderer</c> to draw the polygon's border.
/// </summary>
public class RadarGraphPolygonUI : MonoBehaviour {

    [Header("Parameters")]
    [Tooltip("Default color used to fill the area of the polygon.")]
    public Color fillColor = Color.blue;
    [Tooltip("Whether the polygon should have a border rendered.")]
    public bool hasBorder = true;
    [Tooltip("Default color for the border.")]
    public Color borderColor = Color.black;
    [Tooltip("Default thickness of the border.")]
    public float borderThickness = 4f;

    [Header("Other components")]
    [Tooltip("Component used for rendering the area of the polygon.")]
    [SerializeField] UIGraphPolygonRenderer polygonRenderer;
    [Tooltip("Component used for rendering the border of the polygon (as a closed path).")]
    [SerializeField] UIPathRenderer borderRenderer;

    private bool isTweened = false; // it will be tweened from initial values to target values
    private List<Vector3> currentPoints; // points during the tweening
    private int completeTweens = 0; // how many tweens were completed (indicates whether it is completely done), each polygon point is tweened separately


    /// <summary>
    /// Draws a radar graph polygon with a default color and with the given parameters.
    /// </summary>
    /// <param name="points">A list of points defining the polygon.</param>
    /// <param name="hasBorder"><c>true</c> if the polygon's border should be rendered (with a default color and thickness), <c>false</c> otherwise.</param>
    public void DrawPolygon(List<Vector3> points, bool hasBorder = false) {
        DrawPolygon(points, fillColor, hasBorder, borderColor, borderThickness);
    }

    /// <summary>
    /// Draws a radar graph polygon with the given parameters.
    /// </summary>
    /// <param name="points">A list of points defining the polygon.</param>
    /// <param name="fillColor">Color which will be used to fill the polygon's area.</param>
    /// <param name="hasBorder"><c>true</c> if the polygon's border should be rendered (with a default color and thickness), <c>false</c> otherwise.</param>
    public void DrawPolygon(List<Vector3> points, Color fillColor, bool hasBorder = false) {
        DrawPolygon(points, fillColor, hasBorder, borderColor, borderThickness);
    }

    /// <summary>
    /// Draws a radar graph polygon with the given parameters.
    /// </summary>
    /// <param name="points">A list of points defining the polygon.</param>
    /// <param name="fillColor">Color which will be used to fill the polygon's area.</param>
    /// <param name="borderColor">Color which will be used to draw the polygon's border.</param>
    /// <param name="borderThickness">Thickness of the border.</param>
    public void DrawPolygon(List<Vector3> points, Color fillColor, Color borderColor, float borderThickness) {
        DrawPolygon(points, fillColor, true, borderColor, borderThickness);
    }

    /// <summary>
    /// Draws a radar graph polygon with the given parameters.
    /// </summary>
    /// <param name="points">A list of points defining the polygon.</param>
    /// <param name="fillColor">Color which will be used to fill the polygon's area.</param>
    /// <param name="hasBorder"><c>true</c> if the polygon's border should be rendered, <c>false</c> otherwise.</param>
    /// <param name="borderColor">Color which will be used to draw the polygon's border.</param>
    /// <param name="borderThickness">Thickness of the border.</param>
    public void DrawPolygon(List<Vector3> points, Color fillColor, bool hasBorder, Color borderColor, float borderThickness) {
        this.fillColor = fillColor;
        this.hasBorder = hasBorder;
        this.borderColor = borderColor;
        this.borderThickness = borderThickness;
        polygonRenderer.AddPolygon(points, fillColor);
        if (hasBorder)
            borderRenderer.AddPath(points, borderColor, borderThickness, true);
    }

    /// <summary>
    /// Draws a radar graph polygon defined by the initial points, and tweens it so that it transforms into polygon defined by the target points over time. 
    /// </summary>
    /// <param name="points">A list of points defining the target polygon.</param>
    /// <param name="initialPoints">A list of points defining the initial polygon.</param>
    /// <param name="duration">Duration of the tween in seconds.</param>
    /// <param name="hasBorder"><c>true</c> if the polygon's border should be rendered (with a default color and thickness), <c>false</c> otherwise.</param>
    public void DrawAndTweenPolygon(List<Vector3> points, List<Vector3> initialPoints, float duration, bool hasBorder = false) {
        DrawAndTweenPolygon(points, initialPoints, duration, fillColor, hasBorder, borderColor, borderThickness);
    }

    /// <summary>
    /// Draws a radar graph polygon defined by the initial points, and tweens it so that it transforms into polygon defined by the target points over time.
    /// </summary>
    /// <param name="points">A list of points defining the target polygon.</param>
    /// <param name="initialPoints">A list of points defining the initial polygon.</param>
    /// <param name="duration">Duration of the tween in seconds.</param>
    /// <param name="fillColor">Color which will be used to fill the polygon's area.</param>
    /// <param name="hasBorder"><c>true</c> if the polygon's border should be rendered (with a default color and thickness), <c>false</c> otherwise.</param>
    public void DrawAndTweenPolygon(List<Vector3> points, List<Vector3> initialPoints, float duration, Color fillColor, bool hasBorder = false) {
        DrawAndTweenPolygon(points, initialPoints, duration, fillColor, hasBorder, borderColor, borderThickness);
    }

    /// <summary>
    /// Draws a radar graph polygon defined by the initial points, and tweens it so that it transforms into polygon defined by the target points over time.
    /// </summary>
    /// <param name="points">A list of points defining the target polygon.</param>
    /// <param name="initialPoints">A list of points defining the initial polygon.</param>
    /// <param name="duration">Duration of the tween in seconds.</param>
    /// <param name="fillColor">Color which will be used to fill the polygon's area.</param>
    /// <param name="borderColor">Color which will be used to draw the polygon's border.</param>
    /// <param name="borderThickness">Thickness of the border.</param>
    public void DrawAndTweenPolygon(List<Vector3> points, List<Vector3> initialPoints, float duration, Color fillColor, Color borderColor, float borderThickness) {
        DrawAndTweenPolygon(points, initialPoints, duration, fillColor, true, borderColor, borderThickness);
    }

    /// <summary>
    /// Draws a radar graph polygon defined by the initial points, and tweens it so that it transforms into polygon defined by the target points over time.
    /// </summary>
    /// <param name="points">A list of points defining the target polygon.</param>
    /// <param name="initialPoints">A list of points defining the initial polygon.</param>
    /// <param name="duration">Duration of the tween in seconds.</param>
    /// <param name="fillColor">Color which will be used to fill the polygon's area.</param>
    /// <param name="hasBorder"><c>true</c> if the polygon's border should be rendered, <c>false</c> otherwise.</param>
    /// <param name="borderColor">Color which will be used to draw the polygon's border.</param>
    /// <param name="borderThickness">Thickness of the border.</param>
    public void DrawAndTweenPolygon(List<Vector3> points, List<Vector3> initialPoints, float duration, Color fillColor, bool hasBorder, Color borderColor, float borderThickness) {
        this.isTweened = true;
        // Draw initial polygon
        this.currentPoints = new List<Vector3>();
        foreach (var point in initialPoints) currentPoints.Add(point);
        DrawPolygon(currentPoints, fillColor, hasBorder, borderColor, borderThickness);
        // Start the tween
        for (int i = 0; i < currentPoints.Count; i++) {
            int index = i;
            DOTween.To(() => currentPoints[index], x => currentPoints[index] = x, points[index], duration).OnComplete(() => completeTweens++); 
        }
    }

    private void Update() {
        // Handle tweening
        if (isTweened) {
            // Remove previously rendered polygon
            polygonRenderer.ResetAll();
            borderRenderer.ResetAll();
            // Render a new one (using the current points computed during the tween)
            DrawPolygon(currentPoints, fillColor, hasBorder, borderColor, borderThickness);
            if (completeTweens >= currentPoints.Count) isTweened = false;
        }
	}
}