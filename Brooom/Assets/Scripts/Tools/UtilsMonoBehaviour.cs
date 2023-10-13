using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilsMonoBehaviour : MonoBehaviour
{
    // Find object of the given type and tag
    public static T FindObjectOfTypeAndTag<T>(string tag) where T : Component {
        List<T> objectsOfType = Utils.FindObject<T>();
        foreach (var obj in objectsOfType) {
            if (obj.CompareTag(tag)) return obj;
        }
        return null;
    }
    public static List<T> FindObjectsOfTypeAndTag<T>(string tag) where T : Component {
        List<T> objectsOfType = Utils.FindObject<T>();
        List<T> result = new List<T>();
        foreach (var obj in objectsOfType) {
            if (obj.CompareTag(tag))
                result.Add(obj);
        }
        return result;
    }

    // Destroys all children of the given Transform
    public static void RemoveAllChildren(Transform parent) {
        for (int i = parent.childCount - 1; i >= 0; i--) {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    public static void RemoveAllChildrenOfType<T>(Transform parent) where T : MonoBehaviour {
        T[] children = parent.GetComponentsInChildren<T>();
        for (int i = children.Length - 1; i >= 0; i--)
            Destroy(children[i].gameObject);
    }
}
