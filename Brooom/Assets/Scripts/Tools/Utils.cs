using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;


public static class Utils
{

    /// <summary>
    /// Wraps the given value into the given interval (i.e. looping over one edge to the other, until the value is within the interval).
    /// </summary>
    /// <param name="value">Original value to wrap.</param>
    /// <param name="minInclusive">Minimum value of the interval.</param>
    /// <param name="maxInclusive">Maximum value of the interval.</param>
    /// <returns>The value inside of the interval corresponding to the original value.</returns>
    public static int Wrap(int value, int minInclusive, int maxInclusive) {
        while (value < minInclusive || value > maxInclusive) {
            if (value < minInclusive)
                value += (maxInclusive - minInclusive + 1);
            else if (value > maxInclusive)
                value -= (maxInclusive - minInclusive + 1);
        }
        return value;
    }

    /// <summary>
    /// Remaps the given value from one range of values to another.
    /// </summary>
    /// <param name="value">Value from the original range to be mapped.</param>
    /// <param name="fromMin">Minimum value of the original range of values.</param>
    /// <param name="fromMax">Maximum value of the original range of values.</param>
    /// <param name="toMin">Minimum value of the new range of values.</param>
    /// <param name="toMax">Maximum value of the new range of values.</param>
    /// <returns>The original value mapped to a new range.</returns>
    public static int RemapRange(int value, int fromMin, int fromMax, int toMin, int toMax) {
        float newValue = (float)(value - fromMin) / (fromMax - fromMin); // from (fromMin, fromMax) to (0, 1)
        newValue = newValue * (toMax - toMin) + toMin; // from (0, 1) to (toMin, toMax)
        return Mathf.RoundToInt(newValue);
    }

    /// <summary>
    /// Remaps the given value from one range of values to another.
    /// </summary>
    /// <param name="value">Value from the original range to be mapped.</param>
    /// <param name="fromMin">Minimum value of the original range of values.</param>
    /// <param name="fromMax">Maximum value of the original range of values.</param>
    /// <param name="toMin">Minimum value of the new range of values.</param>
    /// <param name="toMax">Maximum value of the new range of values.</param>
    /// <returns>The original value mapped to a new range.</returns>
    public static float RemapRange(float value, float fromMin, float fromMax, float toMin, float toMax) {
        value = (value - fromMin) / (fromMax - fromMin); // from (fromMin, fromMax) to (0, 1)
        value = value * (toMax - toMin) + toMin; // from (0, 1) to (toMin, toMax)
        return value;
    }


    /// <summary>
    /// Converts the given time in seconds to a string with minutes:seconds.miliseconds format. 
    /// </summary>
    /// <param name="timeInSeconds">Time in seconds to format.</param>
    /// <returns>String formatted as minutes:seconds.miliseconds.</returns>
    public static string FormatTime(float timeInSeconds) {
        int minutes = (int)(timeInSeconds / 60);
        int seconds = (int)(timeInSeconds % 60);
        int miliseconds = (int)((timeInSeconds - (minutes * 60) - seconds) * 1000);
        if (minutes > 0)
            return $"{minutes}:{seconds.ToString("D2")}.{miliseconds.ToString("D3")}";
        else
            return $"{seconds}.{miliseconds.ToString("D3")}";
    }

    /// <summary>
    /// Creates a new <c>Color</c> instance based on the given color but with modified alpha component.
    /// </summary>
    /// <param name="c">Original color value.</param>
    /// <param name="a">New value of the alpha component.</param>
    /// <returns>Original color with modified alpha component.</returns>
    public static Color WithA(this Color c, float a)
        => new Color(c.r, c.g, c.b, a);
    /// <summary>
    /// Creates a new <c>Color</c> instance based on the given color but with modified red component.
    /// </summary>
    /// <param name="c">Original color value.</param>
    /// <param name="r">New value of the red component.</param>
    /// <returns>Original color with modified red component.</returns>
    public static Color WithR(this Color c, float r)
        => new Color(r, c.g, c.b, c.a);
    /// <summary>
    /// Creates a new <c>Color</c> instance based on the given color but with modified green component.
    /// </summary>
    /// <param name="c">Original color value.</param>
    /// <param name="g">New value of the green component.</param>
    /// <returns>Original color with modified green component.</returns>
    public static Color WithG(this Color c, float g)
        => new Color(c.r, g, c.b, c.a);
    /// <summary>
    /// Creates a new <c>Color</c> instance based on the given color but with modified blue component.
    /// </summary>
    /// <param name="c">Original color value.</param>
    /// <param name="b">New value of the blue component.</param>
    /// <returns>Original color with modified blue component.</returns>
    public static Color WithB(this Color c, float b)
        => new Color(c.r, c.g, b, c.a);

    /// <summary>
    /// Creates a new <c>Color</c> instance from values of individual components (between 0 and 256).
    /// </summary>
    /// <param name="r">Number between 0 and 256 for the red component.</param>
    /// <param name="g">Number between 0 and 256 for the green component.</param>
    /// <param name="b">Number between 0 and 256 for the blue component.</param>
    /// <returns></returns>
    public static Color ColorFromRBG256(int r, int g, int b)
        => new Color(r / 256f, g / 256f, b / 256f);

    /// <summary>
    /// Converts the given <c>Color</c> instance to a <c>string</c> containing its hex value.
    /// </summary>
    /// <param name="c">Color to convert to hex.</param>
    /// <returns>Hex value of the given color as a string.</returns>
    public static string ToHex(this Color c) {
        // Convert from 0..1 to 0..255
        int r = (int)(c.r * 255);
        int g = (int)(c.g * 255);
        int b = (int)(c.b * 255);
        // Convert to hex
        return "#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
    }


    /// <summary>
    /// Determines if two segments given as a + t * (b - a) and c + u * (d - c) are intersecting when projected to the XZ plane (Y = 0).
    /// </summary>
    /// <param name="a">First point of the first segment.</param>
    /// <param name="b">Second point of the first segment.</param>
    /// <param name="c">First point of the second segment.</param>
    /// <param name="d">Second point of the second segment.</param>
    /// <returns><c>true</c> if the two segments have an intersection, <c>false</c> otherwise.</returns>
    public static bool AreSegmentsIntersectingXZ(Vector3 a, Vector3 b, Vector3 c, Vector3 d) {
        // Equations taken from: https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
        float denominator = (a.x - b.x) * (c.z - d.z) - (a.z - b.z) * (c.x - d.x);
        if (System.Math.Abs(denominator) < 0.000001) return false; // there is no intersection
        float t = ((a.x - c.x) * (c.z - d.z) - (a.z - c.z) * (c.x - d.x)) / denominator;
        float u = ((a.x - c.x) * (a.z - b.z) - (a.z - c.z) * (a.x - b.x)) / denominator;
        return t >= 0 && t < 1 && u > 0 && u <= 1; // end point is shared, so t < 1 (not t <= 1) and u > 0 (not u >= 0)
    }

    /// <summary>
    /// Tries to find an intersection of two lines (given by their two points as a + t * (b - a) and c + u * (d - c)) projected to the XZ plane.
    /// </summary>
    /// <param name="a">First point of the first line.</param>
    /// <param name="b">Second point of the first line.</param>
    /// <param name="c">First point of the second line.</param>
    /// <param name="d">Second point of the second line.</param>
    /// <param name="intersection">Intersection of the two lines (if exists).</param>
    /// <returns><c>true</c> if an intersection exists, <c>false</c> otherwise.</returns>
    public static bool TryGetLineIntersectionXZ(Vector3 a, Vector3 b, Vector3 c, Vector3 d, out Vector3 intersection) {
        intersection = Vector3.zero;
        // Equations taken from: https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
        double x1 = a.x, y1 = a.z, x2 = b.x, y2 = b.z, x3 = c.x, y3 = c.z, x4 = d.x, y4 = d.z;
        double denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        if (System.Math.Abs(denominator) < 0.000001) return false; // there is no intersection
        intersection.x = (float)(((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) / denominator);
        intersection.z = (float)(((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) / denominator);
        return true;
    }


    /// <summary>
    /// Creates a new <c>Vector2</c> instance based on the given vector but with a modified X component.
    /// </summary>
    /// <param name="v">Original vector.</param>
    /// <param name="x">New value of the X component.</param>
    /// <returns>Original vector with modified X component.</returns>
    public static Vector2 WithX(this Vector2 v, float x)
        => new Vector2(x, v.y);
    /// <summary>
    /// Creates a new <c>Vector2</c> instance based on the given vector but with a modified Y component.
    /// </summary>
    /// <param name="v">Original vector.</param>
    /// <param name="y">New value of the Y component.</param>
    /// <returns>Original vector with modified Y component.</returns>
    public static Vector2 WithY(this Vector2 v, float y)
        => new Vector2(v.x, y);
    /// <summary>
    /// Creates a new <c>Vector3</c> instance based on the given vector but with a modified X component.
    /// </summary>
    /// <param name="v">Original vector.</param>
    /// <param name="x">New value of the X component.</param>
    /// <returns>Original vector with modified X component.</returns>
    public static Vector3 WithX(this Vector3 v, float x)
        => new Vector3(x, v.y, v.z);
    /// <summary>
    /// Creates a new <c>Vector3</c> instance based on the given vector but with a modified Y component.
    /// </summary>
    /// <param name="v">Original vector.</param>
    /// <param name="y">New value of the Y component.</param>
    /// <returns>Original vector with modified Y component.</returns>
    public static Vector3 WithY(this Vector3 v, float y)
        => new Vector3(v.x, y, v.z);
    /// <summary>
    /// Creates a new <c>Vector3</c> instance based on the given vector but with a modified Z component.
    /// </summary>
    /// <param name="v">Original vector.</param>
    /// <param name="z">New value of the Z component.</param>
    /// <returns>Original vector with modified Z component.</returns>
    public static Vector3 WithZ(this Vector3 v, float z)
        => new Vector3(v.x, v.y, z);


    /// <summary>
    /// If the target object has <c>GenericTween</c> component, then its <c>DoTween()</c> method is called to enable the object using the tween.
    /// Otherwise, the object is simply activated by calling <c>SetActive(true)</c>.
    /// </summary>
    /// <param name="go">Target object to be enabled.</param>
    public static void TweenAwareEnable(this GameObject go) {
        GenericTween tween = go.GetComponent<GenericTween>();
        if (tween != null)
            tween.DoTween();
        else
            go.SetActive(true);
    }
    /// <summary>
    /// If the target object has <c>GenericTween</c> component, then its <c>UndoTween()</c> method is called to disable the object using the tween.
    /// Otherwise, the object is simply deactivated by calling <c>SetActive(false)</c>.
    /// </summary>
    /// <param name="go">Target object to be disabled.</param>
    public static void TweenAwareDisable(this GameObject go) {
        GenericTween tween = go.GetComponent<GenericTween>();
        if (tween != null)
            tween.UndoTween();
        else
            go.SetActive(false);
    }


    /// <summary>
    /// Checks whether the given <c>UnityEvent</c> is empty (i.e. doesn't contain any listeners/callbacks).
    /// </summary>
    /// <param name="unityEvent"><c>UnityEvent</c> to check if empty.</param>
    /// <returns><c>true</c> if the given event is empty, <c>false</c> otherwise.</returns>
    public static bool IsNullEvent(UnityEvent unityEvent) {
        if (unityEvent == null) return true;
        for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++) {
            if (unityEvent.GetPersistentTarget(i) != null) {
                return false;
            }
        }
        return true;
    }
    /// <summary>
    /// Checks whether the given <c>UnityEvent</c> is empty (i.e. doesn't contain any listeners/callbacks).
    /// </summary>
    /// <typeparam name="T">Type of the <c>UnityEvent</c>'s parameter.</typeparam>
    /// <param name="unityEvent"><c>UnityEvent</c> to check if empty.</param>
    /// <returns><c>true</c> if the given event is empty, <c>false</c> otherwise.</returns>
    public static bool IsNullEvent<T>(UnityEvent<T> unityEvent) {
        if (unityEvent == null) return true;
        for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++) {
            if (unityEvent.GetPersistentTarget(i) != null) {
                return false;
            }
        }
        return true;
    }



    /// <summary>
    /// Unlocks the cursor and makes it visible.
    /// </summary>
    public static void EnableCursor() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    /// <summary>
    /// Locks the cursor to the centre of the screen and hides it.
    /// </summary>
    public static void DisableCursor() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    /// <summary>
    /// Check whether the cursor is currently locked.
    /// </summary>
    /// <returns><c>true</c> if the cursor is locked, <c>false</c> otherwise.</returns>
    public static bool IsCursorLocked() {
        return Cursor.lockState != CursorLockMode.None;
    }


    /// <summary>
    /// Captures a screenshot and saves it to a dedicated folder.
    /// </summary>
    public static void SaveScreenshot() {
        string screenshotsFolder = Path.Combine(Application.persistentDataPath, "Screenshots");
        if (!Directory.Exists(screenshotsFolder)) Directory.CreateDirectory(screenshotsFolder);
        string screenshotName = $"LevelScreenshot_{DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss")}.png";
        ScreenCapture.CaptureScreenshot(Path.Combine(screenshotsFolder, screenshotName));
        Debug.Log($"Screenshot captured and stored as {screenshotName}");
    }

}
