using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component allowing to rotate an object on click-and-drag.
/// </summary>
public class RotateObjectOnMouse : MonoBehaviour {

    [Tooltip("How quickly the object rotates.")]
    public float rotationSpeed = 2f;

    // Current rotation of the object
    private Vector3 currentRotation;


    void Start() {
        currentRotation = transform.localEulerAngles;
    }

    void Update(){
        // Rotate the object around the Y axis when the left mouse button is pressed and the mouse moves in the X axis
        if (Input.GetMouseButton(0)) {
            // Mouse inputs are already framerate independent - multiplying with Time.deltaTime would make it framerate dependent
            float mouseX = Input.GetAxis("Mouse X") * SettingsUI.mouseSensitivity;
            currentRotation.y -= (mouseX * rotationSpeed);
            transform.localRotation = Quaternion.Euler(currentRotation);
        }

    }
}
