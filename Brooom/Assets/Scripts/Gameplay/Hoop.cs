using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hoop : MonoBehaviour
{
    [Tooltip("Object containing the visual part of the hoop which can be scaled up/down.")]
    [SerializeField] Transform hoopVisual;

    public void SetScale(Vector3 scaleValue) {
        hoopVisual.localScale = scaleValue;
    }
}
