using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component which can be used to scale associated object up or down.
/// </summary>
public class Scalable : MonoBehaviour {

    [Tooltip("Transform which should be scaled up/down. If null, the implicit 'transform' will be used.")]
    [SerializeField] Transform transformToScale;

    /// <summary>
    /// Sets object's local scale to the given value.
    /// </summary>
    /// <param name="scaleValue">Desired scale value.</param>
    public void SetScale(Vector3 scaleValue) {
        if (transformToScale != null)
            transformToScale.localScale = scaleValue;
        else
            transform.localScale = scaleValue;
    }

    /// <summary>
    /// Gets current local scale of the object.
    /// </summary>
    /// <returns>Current local scale.</returns>
    public Vector3 GetScale() {
        if (transformToScale != null)
            return transformToScale.localScale;
        else
            return transform.localScale;
    }

}
