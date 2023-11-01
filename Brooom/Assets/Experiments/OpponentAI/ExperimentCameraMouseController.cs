using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentCameraMouseController : MonoBehaviour
{
    public float speed = 50f;
    public float sensitivity = 5f;

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update() {
        // Move the camera forward
        if (Input.GetMouseButton(0)) {
            transform.position += transform.forward * speed * Time.unscaledDeltaTime;
        }
        // Rotate the camera based on the mouse movement
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        transform.eulerAngles += new Vector3(-mouseY * sensitivity, mouseX * sensitivity, 0);
    }
}
