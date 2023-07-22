using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilsMonoBehaviour : MonoBehaviour
{
    // Destroys all children of the given Transform
    public static void RemoveAllChildren(Transform parent) {
        for (int i = parent.childCount - 1; i >= 0; i--) {
            Destroy(parent.GetChild(i).gameObject);
        }
    }
}
