using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// An element derived from <c>Graphic</c> which draws filled polygons.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class UIGraphPolygonRenderer : Graphic {

	private List<PolygonData> polygons; // a list of all polygons to be drawn

	private Color currentColor;

	/// <summary>
	/// Adds a new polygon (with default fill color) to the list of polygons to be drawn, and initiates a redraw.
	/// </summary>
	/// <param name="points">A list of points defining the polygon.</param>
	public void AddPolygon(List<Vector3> points) {
		AddPolygonData(points, color);
	}

	/// <summary>
	/// Adds a new polygon to the list of polygons to be drawn, and initiates a redraw.
	/// It allows to set fill color separately for each polygon.
	/// </summary>
	/// <param name="points">A list of points defining the polygon.</param>
	/// <param name="fillColor">Color to be used when drawing the polygon.</param>
	public void AddPolygon(List<Vector3> points, Color fillColor) {
		AddPolygonData(points, fillColor);
	}

	/// <summary>
	/// Resets everything, deleting all previously drawn polygons.
	/// </summary>
	public void ResetAll() {
		polygons?.Clear();
		SetVerticesDirty();
	}

	// Draws all polygons from the list
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

	// Adds a polygon with the given parameters into a list of polygons to be drawn, and initiates a redraw
	private void AddPolygonData(List<Vector3> points, Color fillColor) {
		if (polygons == null) polygons = new List<PolygonData>();
		polygons.Add(new PolygonData { points = points, fillColor = fillColor });
		SetVerticesDirty();
	}
}

/// <summary>
/// A data structure containing parameters of a polygon which can be drawn by <c>UIGraphPolygonRenderer</c>, e.g. a list of points, fill color.
/// </summary>
public struct PolygonData {
	/// <summary>A list of points defining the polygon.</summary>
	public List<Vector3> points;
	/// <summary>Color to be used when drawing the polygon (it will be filled).</summary>
	public Color fillColor;
}