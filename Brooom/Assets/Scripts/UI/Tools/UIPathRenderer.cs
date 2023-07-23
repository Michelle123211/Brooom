using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// Draws paths composed of line segments
[RequireComponent(typeof(CanvasRenderer))]
public class UIPathRenderer : Graphic
{
	[Tooltip("Default thickness of all the lines/paths drawn by this component.")]
	[SerializeField] float defaultLineThickness = 2f;

	private List<PathData> paths;

	private Color currentColor;
	private float currentThickness;

	public void AddPath(List<Vector3> points, bool loop = false) {
		AddPathData(points, color, defaultLineThickness, loop);
	}

	// Allows to set color and thickness for each path separately
	public void AddPath(List<Vector3> points, Color color, float thickness, bool loop = false) {
		AddPathData(points, color, thickness, loop);
	}

	public void ResetAll() {
		paths.Clear();
		SetVerticesDirty();
	}

	protected override void OnPopulateMesh(VertexHelper vh) {
		vh.Clear();

		if (paths == null) return;

		// Draw all paths
		foreach (var path in paths) {
			if (path.points == null || path.points.Count < 2) continue; // not enough points to draw a segment
			// Set parameters
			currentColor = path.color;
			currentThickness = path.thickness;
			// Add 4 vertices for each segment
			for (int i = 0; i < path.points.Count - 1; i++) {
				AddPathSegment(vh, path.points[i], path.points[i + 1]);
			}
			// Handle loop
			if (path.loop)
				AddPathSegment(vh, path.points[path.points.Count - 1], path.points[0]);
		}
	}

	private void AddPathData(List<Vector3> points, Color color, float thickness, bool loop) {
		if (paths == null) paths = new List<PathData>();
		paths.Add(new PathData { points = points, color = color, thickness = thickness, loop = loop });
		SetVerticesDirty();
	}

	// Creates 4 vertices which are then connected into 2 triangles to make the segment of given thickness
	private void AddPathSegment(VertexHelper vh, Vector3 point1, Vector3 point2) {
		AddPathSegmentVertices(vh, point1, point2);
		AddPathSegmentTriangles(vh);
	}

	// Creates 4 vertices for the segment
	private void AddPathSegmentVertices(VertexHelper vh, Vector3 point1, Vector3 point2) {
		// Get angle of the segment, add to start to the left
		float angle = GetAngleOfSegment(point1, point2) + 90f;
		float halfThickness = currentThickness / 2;

		UIVertex vertex = UIVertex.simpleVert;
		vertex.color = currentColor;
		// 2 vertices for the first point
		vertex.position = Quaternion.Euler(0, 0, angle) * new Vector3(-halfThickness, 0);
		vertex.position += point1;
		vh.AddVert(vertex);
		vertex.position = Quaternion.Euler(0, 0, angle) * new Vector3(halfThickness, 0);
		vertex.position += point1;
		vh.AddVert(vertex);
		// 2 vertices for the second point
		vertex.position = Quaternion.Euler(0, 0, angle) * new Vector3(-halfThickness, 0);
		vertex.position += point2;
		vh.AddVert(vertex);
		vertex.position = Quaternion.Euler(0, 0, angle) * new Vector3(halfThickness, 0);
		vertex.position += point2;
		vh.AddVert(vertex);
	}

	// Connects the last 4 vertices into triangles
	private void AddPathSegmentTriangles(VertexHelper vh) {
		int firstIndex = vh.currentVertCount - 4;
		vh.AddTriangle(firstIndex, firstIndex + 1, firstIndex + 2);
		vh.AddTriangle(firstIndex + 1, firstIndex + 3, firstIndex + 2);
	}

	private float GetAngleOfSegment(Vector2 point1, Vector2 point2) {
		// Angle in radians converted to degrees
		return Mathf.Atan2(point2.y - point1.y, point2.x - point1.x) * 180 / Mathf.PI;
	}
}


public struct PathData {
    public List<Vector3> points;
    public bool loop;
	public Color color;
	public float thickness;
}