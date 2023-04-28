using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float mouseSensitivity = 3f;
    [Tooltip("How long in seconds does it take to ease in the sensitivity after start (to prevent quick jump at the beginning).")]
    public float sensitivityEaseInDuration = 1f;
    [Tooltip("The maximum angle (in absolute value) the camera may rotate in the X axis.")]
    public float maxAngleX = 60;
    [Tooltip("The maximum angle (in absolute value) the camera may rotate in the Y axis.")]
    public float maxAngleY = 120;

    [Tooltip("A list of virtual cameras to switch between (in the exact order, index 0 is the default one).")]
    public List<GameObject> virtualCameras;


    // Current rotation of the camera
    private float rotationX = 0f;
    private float rotationY = 0f;

    // Current state of the sensitivity ease in at the start
    private float currentT = 0f;
    private float currentSensitivity = 0f;

    // Current camera view
    private int currentCameraIndex = 0;
    private GameObject currentCamera;


    // Start is called before the first frame update
    void Start() {
        // Lock the cursor to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Enable only the first virtual camera
        if (virtualCameras.Count == 0)
            Debug.LogError("There are no virtual cameras assigned.");
        for (int i = 0; i < virtualCameras.Count; i++) {
            if (i == 0) {
                currentCamera = virtualCameras[i];
                virtualCameras[i].SetActive(true);
            } else {
                virtualCameras[i].SetActive(false);
            }
        }
        // Reset the rotation
        currentCamera.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);

    }

    // Update is called once per frame
    void Update()
    {
        // Handle switching the view
        if (Input.GetKeyDown(KeyCode.V)) {
            int previousCameraIndex = currentCameraIndex;
            currentCameraIndex++;
            if (currentCameraIndex == virtualCameras.Count)
                currentCameraIndex = 0;
            currentCamera = virtualCameras[currentCameraIndex];
            virtualCameras[currentCameraIndex].SetActive(true);
            virtualCameras[previousCameraIndex].SetActive(false);
        }

        // Gradually increase the sensitivity over the first "sensitivityEaseInDuration" seconds
        //  - at first the camera is following the mouse very slowly so it prevents quick jump at the beginning
        if (currentSensitivity < mouseSensitivity) {
            currentT = Mathf.Clamp(currentT + Time.deltaTime / sensitivityEaseInDuration, 0, 1);
            currentSensitivity = currentT * currentT * currentT * currentT * mouseSensitivity; // EaseInQuart
        }

        // Mouse inputs are already framerate independent - multiplying with Time.deltaTime would make it framerate dependent
        float mouseX = Input.GetAxis("Mouse X") * currentSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * currentSensitivity;

        // Limiting the angle of rotation
        rotationX = Mathf.Clamp(rotationX - mouseY, -maxAngleX, maxAngleX);
        rotationY = Mathf.Clamp(rotationY + mouseX, -maxAngleY, maxAngleY);

        // Rotating the current virtual camera
        currentCamera.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);

    }
}
