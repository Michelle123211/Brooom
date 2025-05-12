using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// This class contains useful static methods which need to be in a class derived from <c>MonoBehaviour</c>
/// to have access to something from it (e.g. <c>Destroy()</c> method), or which are working with objects in some way.
/// It is not necessary to have an object with this script in a scene. It is enough that it is derived from <c>Monobehaviour</c>
/// and then the methods can be used everywhere.
/// </summary>
public class UtilsMonoBehaviour : MonoBehaviour {

    /// <summary>
    /// Activates/deactivates all objects in the list.
    /// </summary>
    /// <typeparam name="T">Type of objects to be activated/deactivated.</typeparam>
    /// <param name="objectsToActivate">A list of objects to be activated/deactivated.</param>
    /// <param name="activeValue"><c>true</c> if the objects should be activated, <c>false</c> if deactivated.</param>
    public static void SetActiveForAll<T>(List<T> objectsToActivate, bool activeValue) where T : Component {
        foreach (var objectToActivate in objectsToActivate)
            objectToActivate.gameObject.SetActive(activeValue);
    }

    /// <summary>
    /// Finds objects of the given type in the scene, including objects which are not active.
    /// </summary>
    /// <typeparam name="T">Type of the objects to find.</typeparam>
    /// <returns>A list of objects found in the scene.</returns>
    public static List<T> FindObjects<T>() where T : Component {
        List<T> result = new();
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects()) {
            AddHidden(root, result);
        }
        return result;
    }
    /// <summary>
    /// Finds an object of the given type in the scene, including objects which are not active.
    /// </summary>
    /// <typeparam name="T">Type of the object to find.</typeparam>
    /// <returns>An object found in the scene.</returns>
    public static T FindObject<T>() where T : Component {
        List<T> result = FindObjects<T>();
        if (result == null || result.Count == 0) return null;
        else return result[0];
    }
    // Adds all components of the given type from the whole hierarchy starting from the given root object into a list of results
    private static void AddHidden<T>(GameObject root, List<T> result) where T : Component {
        if (root == null) return;
        foreach (T component in root.GetComponents<T>()) {
            result.Add(component);
        }

        for (int i = 0; i < root.transform.childCount; i++) {
            AddHidden(root.transform.GetChild(i).gameObject, result);
        }
    }

    /// <summary>
    /// Finds an object of the given type and with the given tag in the scene.
    /// </summary>
    /// <typeparam name="T">Type of the object to find.</typeparam>
    /// <param name="tag">Tag to find.</param>
    /// <returns>An object found in the scene.</returns>
    public static T FindObjectOfTypeAndTag<T>(string tag) where T : Component {
        List<T> objectsOfType = FindObjects<T>();
        foreach (var obj in objectsOfType) {
            if (obj.CompareTag(tag)) return obj;
        }
        return null;
    }
    /// <summary>
    /// Finds objects of the given type and with the given tag in the scene.
    /// </summary>
    /// <typeparam name="T">Type of the objects to find.</typeparam>
    /// <param name="tag">Tag to find.</param>
    /// <returns>Objects found in the scene.</returns>
    public static List<T> FindObjectsOfTypeAndTag<T>(string tag) where T : Component {
        List<T> objectsOfType = FindObjects<T>();
        List<T> result = new();
        foreach (var obj in objectsOfType) {
            if (obj.CompareTag(tag))
                result.Add(obj);
        }
        return result;
    }

    /// <summary>
    /// Destroys all children of the given <c>Transform</c>.
    /// </summary>
    /// <param name="parent">Parent whose children will be destroyed.</param>
    /// <param name="startIndex">Child index from which to start.</param>
    public static void RemoveAllChildren(Transform parent, int startIndex = 0) {
        for (int i = parent.childCount - 1; i >= startIndex; i--) {
            Destroy(parent.GetChild(i).gameObject);
        }
    }
    /// <summary>
    /// Destroys all children of a particular type of the given <c>Transform</c>.
    /// </summary>
    /// <typeparam name="T">Type of the objects to destroy.</typeparam>
    /// <param name="parent">Parent whose children will be destroyed.</param>
    public static void RemoveAllChildrenOfType<T>(Transform parent) where T : MonoBehaviour {
        T[] children = parent.GetComponentsInChildren<T>();
        for (int i = children.Length - 1; i >= 0; i--)
            Destroy(children[i].gameObject);
    }
}
