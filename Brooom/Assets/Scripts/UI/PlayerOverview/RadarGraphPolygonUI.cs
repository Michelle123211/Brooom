using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void DrawPolygon(List<Vector3> points, bool hasBorder = false) {
        DrawPolygon(points, fillColor, hasBorder, borderColor, borderThickness);
    }

    public void DrawPolygon(List<Vector3> points, Color fillColor, bool hasBorder = false) {
        DrawPolygon(points, fillColor, hasBorder, borderColor, borderThickness);
    }

    public void DrawPolygon(List<Vector3> points, Color fillColor, Color borderColor, float borderThickness) {
        DrawPolygon(points, fillColor, true, borderColor, borderThickness);
    }

    private void DrawPolygon(List<Vector3> points, Color fillColor, bool hasBorder, Color borderColor, float borderThickness) {
        polygonRenderer.AddPolygon(points, fillColor);
        if (hasBorder)
            borderRenderer.AddPath(points, borderColor, borderThickness, true);
    }
}
