using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Taken from: https://medium.com/@mikeyoung_97230/creating-a-simple-camera-controller-in-unity3d-using-c-ec1a79584687
public class ExperimentCameraController : MonoBehaviour
{
    public float speed = 5f;
    public float sensitivity = 5f;

	private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
	}

	void Update()
    {
        // Move the camera forward, backward, left, and right
        transform.position += transform.forward * Input.GetAxis("Vertical") * speed * Time.deltaTime;
        transform.position += transform.right * Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        // Rotate the camera based on the mouse movement
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        transform.eulerAngles += new Vector3(-mouseY * sensitivity, mouseX * sensitivity, 0);
    }
}
