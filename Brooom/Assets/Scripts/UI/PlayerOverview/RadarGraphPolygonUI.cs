using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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
    [SerializeField] UIGraphPolygonRenderer polygonRenderer;
    [SerializeField] UIPathRenderer borderRenderer;

    private bool isTweened = false;
    private List<Vector3> currentPoints; // points during the tweening
    private int completeTweens = 0; // how many tweens were completed (indicates whether it is completely done)


    public void DrawPolygon(List<Vector3> points, bool hasBorder = false) {
        DrawPolygon(points, fillColor, hasBorder, borderColor, borderThickness);
    }

    public void DrawPolygon(List<Vector3> points, Color fillColor, bool hasBorder = false) {
        DrawPolygon(points, fillColor, hasBorder, borderColor, borderThickness);
    }

    public void DrawPolygon(List<Vector3> points, Color fillColor, Color borderColor, float borderThickness) {
        DrawPolygon(points, fillColor, true, borderColor, borderThickness);
    }

    public void DrawPolygon(List<Vector3> points, Color fillColor, bool hasBorder, Color borderColor, float borderThickness) {
        this.fillColor = fillColor;
        this.hasBorder = hasBorder;
        this.borderColor = borderColor;
        this.borderThickness = borderThickness;
        polygonRenderer.AddPolygon(points, fillColor);
        if (hasBorder)
            borderRenderer.AddPath(points, borderColor, borderThickness, true);
    }


    public void DrawAndTweenPolygon(List<Vector3> points, List<Vector3> initialPoints, float duration, bool hasBorder = false) {
        DrawAndTweenPolygon(points, initialPoints, duration, fillColor, hasBorder, borderColor, borderThickness);
    }

    public void DrawAndTweenPolygon(List<Vector3> points, List<Vector3> initialPoints, float duration, Color fillColor, bool hasBorder = false) {
        DrawAndTweenPolygon(points, initialPoints, duration, fillColor, hasBorder, borderColor, borderThickness);
    }

    public void DrawAndTweenPolygon(List<Vector3> points, List<Vector3> initialPoints, float duration, Color fillColor, Color borderColor, float borderThickness) {
        DrawAndTweenPolygon(points, initialPoints, duration, fillColor, true, borderColor, borderThickness);
    }

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
            // Render a new one
            DrawPolygon(currentPoints, fillColor, hasBorder, borderColor, borderThickness);
            if (completeTweens >= currentPoints.Count) isTweened = false;
        }
	}
}