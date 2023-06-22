using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

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
}
