using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(CanvasRenderer))]
public class UIGraphPolygonRenderer : Graphic {

	private List<PolygonData> polygons;

	private Color currentColor;

	public void AddPolygon(List<Vector3> points) {
		AddPolygonData(points, color);
	}

	public void AddPolygon(List<Vector3> points, Color fillColor) {
		AddPolygonData(points, fillColor);
	}

	public void ResetAll() {
		polygons.Clear();
		SetVerticesDirty();
	}

	protected override void OnPopulateMesh(VertexHelper vh) {
		vh.Clear();

		if (polygons == null) return;

		// Draw all polygons
		foreach (var polygon in polygons) {
			if (polygon.points == null || polygon.points.Count < 2) continue; // not enough points to draw a segment
			// Set parameters
			currentColor = polygon.fillColor;
			// Draw filled polygon
			UIVertex vertex = UIVertex.simpleVert;
			vertex.color = currentColor;
			for (int i = 0; i < polygon.points.Count; i++) {
				vertex.position = polygon.points[i];
				vh.AddVert(vertex);
			}
			vertex.position = Vector3.zero; // origin
			vh.AddVert(vertex);
			for (int i = 0; i < polygon.points.Count; i++) {
				vh.AddTriangle((i + 1) % polygon.points.Count, i, polygon.points.Count);
			}
		}
	}

	private void AddPolygonData(List<Vector3> points, Color fillColor) {
		if (polygons == null) polygons = new List<PolygonData>();
		polygons.Add(new PolygonData { points = points, fillColor = fillColor });
		SetVerticesDirty();
	}
}

public struct PolygonData {
    public List<Vector3> points;
	public Color fillColor;
}