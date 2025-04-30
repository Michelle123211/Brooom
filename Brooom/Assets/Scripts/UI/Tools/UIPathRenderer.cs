using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// An element derived from <c>Graphic</c> which draws paths composed of line segments.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class UIPathRenderer : Graphic {

	[Tooltip("Default thickness of all the lines/paths drawn by this component.")]
	[SerializeField] float defaultLineThickness = 2f;

	private List<PathData> paths; // a list of all paths which should be drawn

	private Color currentColor;
	private float currentThickness;

	/// <summary>
	/// Adds a new path (with default color and thickness) to the list of paths to be drawn, and initiates a redraw.
	/// </summary>
	/// <param name="points">A list of points on the path.</param>
	/// <param name="loop">Whether the path should be closed to create a loop.</param>
	public void AddPath(List<Vector3> points, bool loop = false) {
		AddPathData(points, color, defaultLineThickness, loop);
	}

	/// <summary>
	/// Adds a new path to the list of paths to be drawn, and initiates a redraw.
	/// It allows to set color and thickness separately for each path.
	/// </summary>
	/// <param name="points">A list of points on the path.</param>
	/// <param name="color">Color to be used when drawing the path.</param>
	/// <param name="thickness">Thickness of individual line segments creating the path.</param>
	/// <param name="loop">Whether the path should be closed to create a loop.</param>
	public void AddPath(List<Vector3> points, Color color, float thickness, bool loop = false) {
		AddPathData(points, color, thickness, loop);
	}

	/// <summary>
	/// Resets everything, deleting all previously drawn paths.
	/// </summary>
	public void ResetAll() {
		paths?.Clear();
		SetVerticesDirty();
	}

	// Draws all paths from the list
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

	// Adds a path with the given parameters into a list of paths to be drawn, and initiates a redraw
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
		// Get angle of the segment, add 90° to start to the left
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

	// Gets angle of the segment in degrees
	private float GetAngleOfSegment(Vector2 point1, Vector2 point2) {
		// Angle in radians converted to degrees
		return Mathf.Atan2(point2.y - point1.y, point2.x - point1.x) * 180 / Mathf.PI;
	}
}


/// <summary>
/// A data structure containing parameters of a path which can be drawn by <c>UIPathRenderer</c>, e.g. a list of points, color.
/// </summary>
public struct PathData {
	/// <summary>A list of points on the path to be drawn.</summary>
    public List<Vector3> points;
	/// <summary>Whether the path should be closed to create a loop.</summary>
    public bool loop;
	/// <summary>Color to be used when drawing the path.</summary>
	public Color color;
	/// <summary>Thickness of individual line segments creating the path.</summary>
	public float thickness;
}