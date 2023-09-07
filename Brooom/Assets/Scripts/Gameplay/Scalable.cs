using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scalable : MonoBehaviour
{
    [Tooltip("Transform which should be scaled up/down. If null, the implicit 'transform' will be used.")]
    [SerializeField] Transform transformToScale;

    public void SetScale(Vector3 scaleValue) {
        if (transformToScale != null)
            transformToScale.localScale = scaleValue;
        else
            transform.localScale = scaleValue;
    }
}
