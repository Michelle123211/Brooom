using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;


public enum IterationDirection { 
    Decreasing = -1,
    Increasing = 1
}

public static class Utils
{

    // Returns the value wrapped into the given interval
    public static int Wrap(int value, int minInclusive, int maxInclusive) {
        while (value < minInclusive || value > maxInclusive) {
            if (value < minInclusive)
                value += (maxInclusive - minInclusive + 1);
            else if (value > maxInclusive)
                value -= (maxInclusive - minInclusive + 1);
        }
        return value;
    }

    // Remaps the value from one range to another
    public static float RemapRange(float value, float fromMin, float fromMax, float toMin, float toMax) {
        value = (value - fromMin) / (fromMax - fromMin); // from (fromMin, fromMax) to (0, 1)
        value = value * (toMax - toMin) + toMin; // from (0, 1) to (toMin, toMax)
        return value;
    }


    // Takes time in seconds and returns it formatted as minutes:seconds.miliseconds in a string
    public static string FormatTime(float timeInSeconds) {
        int minutes = (int)(timeInSeconds / 60);
        int seconds = (int)(timeInSeconds % 60);
        int miliseconds = (int)((timeInSeconds - (minutes * 60) - seconds) * 1000);
        if (minutes > 0)
            return $"{minutes}:{seconds.ToString("D2")}.{miliseconds.ToString("D3")}";
        else
            return $"{seconds}.{miliseconds}";
    } 


    public static Color WithA(this Color c, float a)
        => new Color(c.r, c.g, c.b, a);
    public static Color WithR(this Color c, float r)
        => new Color(r, c.g, c.b, c.a);
    public static Color WithG(this Color c, float g)
        => new Color(c.r, g, c.b, c.a);
    public static Color WithB(this Color c, float b)
        => new Color(c.r, c.g, b, c.a);

    public static Color ColorFromRBG256(int r, int g, int b)
        => new Color(r / 256f, g / 256f, b / 256f);

    public static string ToHex(this Color c) {
        // Convert from 0..1 to 0..255
        int r = (int)(c.r * 255);
        int g = (int)(c.g * 255);
        int b = (int)(c.b * 255);
        // Convert to hex
        return "#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
    }


    // Determines if two segments given as a + t * (b - a) and c + u * (d - c) are intersecting when projected to the XZ plane (Y = 0)
    public static bool AreSegmentsIntersectingXZ(Vector3 a, Vector3 b, Vector3 c, Vector3 d) {
        // Equations taken from: https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
        float denominator = (a.x - b.x) * (c.z - d.z) - (a.z - b.z) * (c.x - d.x);
        if (denominator == 0) return false; // there is no intersection
        float t = ((a.x - c.x) * (c.z - d.z) - (a.z - c.z) * (c.x - d.x)) / denominator;
        float u = ((a.x - c.x) * (a.z - b.z) - (a.z - c.z) * (a.x - b.x)) / denominator;
        return t >= 0 && t < 1 && u > 0 && u <= 1; // end point is shared, so t < 1 (not t <= 1) and u > 0 (not u >= 0)
    }

    // Finds intersection of two lines (given by their two points) projected to the XZ plane
    // First line ... a + t * (b - a)
    // Second line ... c + u * (d - c)
    public static bool TryGetLineIntersectionXZ(Vector3 a, Vector3 b, Vector3 c, Vector3 d, out Vector3 intersection) {
        intersection = Vector3.zero;
        // Equations taken from: https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
        float denominator = (a.x - b.x) * (c.z - d.z) - (a.z - b.z) * (c.x - d.x);
        if (denominator == 0) return false; // there is no intersection
        float t = ((a.x - c.x) * (c.z - d.z) - (a.z - c.z) * (c.x - d.x)) / denominator;
        float u = ((a.x - c.x) * (a.z - b.z) - (a.z - c.z) * (a.x - b.x)) / denominator;
        intersection = (a + t * (b - a)).WithY(0);
        return true;
    }


    public static Vector3 WithX(this Vector3 v, float x)
        => new Vector3(x, v.y, v.z);

    public static Vector3 WithY(this Vector3 v, float y)
        => new Vector3(v.x, y, v.z);

    public static Vector3 WithZ(this Vector3 v, float z)
        => new Vector3(v.x, v.y, z);


    public static List<T> FindObject<T>() where T : Component {
        List<T> result = new List<T>();
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects()) {
            AddHidden(root, result);
        }
        return result;
    }


    private static void AddHidden<T>(GameObject root, List<T> result) where T : Component {
        if (root == null) return;
        foreach (T component in root.GetComponents<T>()) {
            result.Add(component);
        }

        for (int i = 0; i < root.transform.childCount; i++) {
            AddHidden(root.transform.GetChild(i).gameObject, result);
        }
    }


    public static void TweenAwareEnable(this GameObject go) {
        GenericTween tween = go.GetComponent<GenericTween>();
        if (tween != null)
            tween.DoTween();
        else
            go.SetActive(true);
    }
    public static void TweenAwareDisable(this GameObject go) {
        GenericTween tween = go.GetComponent<GenericTween>();
        if (tween != null)
            tween.UndoTween();
        else
            go.SetActive(false);
    }


    public static bool IsNullEvent(UnityEvent unityEvent) {
        if (unityEvent == null) return true;
        for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++) {
            if (unityEvent.GetPersistentTarget(i) != null) {
                return false;
            }
        }
        return true;
    }
    public static bool IsNullEvent<T>(UnityEvent<T> unityEvent) {
        if (unityEvent == null) return true;
        for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++) {
            if (unityEvent.GetPersistentTarget(i) != null) {
                return false;
            }
        }
        return true;
    }
}
