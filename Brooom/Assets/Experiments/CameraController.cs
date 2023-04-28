using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float mouseSensitivity = 3f;
    [Tooltip("How long in seconds does it take to ease in the sensitivity after start (to prevent quick jump at the beginning).")]
    public float sensitivityEaseInDuration = 2f;

    // Current rotation of the camera
    private float rotationX = 0f;
    private float rotationY = 0f;

    // Current state of the sensitivity ease in at the start
    private float currentT = 0f;
    private float currentSensitivity = 0f;

    // Start is called before the first frame update
    void Start()
    {
        // lock the cursor to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        // Gradually increase the sensitivity over the first "" seconds
        //  - at first the camera is following the mouse very slowly so it prevents quick jump at the beginning
        if (currentSensitivity < mouseSensitivity) {
            currentT = Mathf.Clamp(currentT + Time.deltaTime / sensitivityEaseInDuration, 0, 1);
            currentSensitivity = currentT * currentT * currentT * currentT * mouseSensitivity; // EaseInQuart
        }

        // Mouse inputs are already framerate independent - multiplying with Time.deltaTime would make it framerate dependent
        float mouseX = Input.GetAxis("Mouse X") * currentSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * currentSensitivity;

        // Limiting the angle of rotation
        rotationX = Mathf.Clamp(rotationX - mouseY, -89f, 89f);
        rotationY = Mathf.Clamp(rotationY + mouseX, -179f, 179f);

        transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }
}
