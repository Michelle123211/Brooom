using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UtilsMonoBehaviour : MonoBehaviour
{
    // Activate/deactivate all objects in the list
    public static void SetActiveForAll<T>(List<T> objectsToActivate, bool activeValue) where T : Component {
        foreach (var objectToActivate in objectsToActivate)
            objectToActivate.gameObject.SetActive(activeValue);
    }

    // Find objects including not active ones
    public static List<T> FindObjects<T>() where T : Component {
        List<T> result = new List<T>();
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects()) {
            AddHidden(root, result);
        }
        return result;
    }
    public static T FindObject<T>() where T : Component {
        List<T> result = FindObjects<T>();
        if (result == null || result.Count == 0) return null;
        else return result[0];
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

    // Find object of the given type and tag
    public static T FindObjectOfTypeAndTag<T>(string tag) where T : Component {
        List<T> objectsOfType = FindObjects<T>();
        foreach (var obj in objectsOfType) {
            if (obj.CompareTag(tag)) return obj;
        }
        return null;
    }
    public static List<T> FindObjectsOfTypeAndTag<T>(string tag) where T : Component {
        List<T> objectsOfType = FindObjects<T>();
        List<T> result = new List<T>();
        foreach (var obj in objectsOfType) {
            if (obj.CompareTag(tag))
                result.Add(obj);
        }
        return result;
    }

    // Destroys all children of the given Transform
    public static void RemoveAllChildren(Transform parent, int startIndex = 0) {
        for (int i = parent.childCount - 1; i >= startIndex; i--) {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    public static void RemoveAllChildrenOfType<T>(Transform parent) where T : MonoBehaviour {
        T[] children = parent.GetComponentsInChildren<T>();
        for (int i = children.Length - 1; i >= 0; i--)
            Destroy(children[i].gameObject);
    }
}
