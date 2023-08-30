using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObjectOnMouse : MonoBehaviour {
    [Tooltip("How quickly the object rotates.")]
    public float rotationSpeed = 2f;


    // Current rotation of the object
    private Vector3 currentRotation;

    // Start is called before the first frame update
    void Start()
    {
        currentRotation = transform.localEulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0)) {
            // Mouse inputs are already framerate independent - multiplying with Time.deltaTime would make it framerate dependent
            float mouseX = Input.GetAxis("Mouse X") * SettingsUI.mouseSensitivity;
            currentRotation.y -= (mouseX * rotationSpeed);
            transform.localRotation = Quaternion.Euler(currentRotation);
        }

    }
}
