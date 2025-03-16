using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class PlayerCameraController : MonoBehaviour {
    [Tooltip("How long in seconds does it take to ease in the sensitivity after start (to prevent quick jump at the beginning).")]
    [SerializeField] float sensitivityEaseInDuration = 1f;

    [Header("Rotation limits")]
    [Tooltip("The maximum angle (in absolute value) the camera may rotate in the X axis.")]
    [SerializeField] float maxAngleX = 60;
    [Tooltip("The maximum angle (in absolute value) the camera may rotate in the Y axis.")]
    [SerializeField] float maxAngleY = 120;

    [Header("Zoom")]
    [Tooltip("Default FOV.")]
    [SerializeField] float defaultFOV = 60;
    [Tooltip("FOV used when zoomed in.")]
    [SerializeField] float zoomedInFOV = 55;
    [Tooltip("How long in seconds it takes to zoom in/out.")]
    [SerializeField] float zoomDuration = 1f;

    [Header("Reset")]
    [Tooltip("How long in seconds it takes for the camera to tween into its default orientation.")]
    [SerializeField] float resetDuration = 0.4f;

    [Header("Views")]
    [Tooltip("A virtual camera looking back behind the player.")]
    [SerializeField] CinemachineVirtualCamera backVirtualCamera;
    [Tooltip("Whether it is possible to switch between different views (except the back view which is always enabled).")]
    [SerializeField] bool enableViewSwitch = false;
    [Tooltip("A list of virtual cameras to switch between (in the exact order, index 0 is the default one).")]
    [SerializeField] List<CinemachineVirtualCamera> virtualCameras;


    // Current rotation of the camera
    private float rotationX = 0f;
    private float rotationY = 0f;

    // Current state of the sensitivity ease in at the start
    private float currentT = 0f;
    private float currentSensitivity = 0f;

    // Current camera view
    private CinemachineVirtualCamera currentCamera;
    private int currentCameraIndex = 0;
    private bool isBackViewOn = false;

    private bool isResetting = false;

    // Resets rotations of all cameras available
    public void ResetCameras(bool rotationOnly = false) {
        // Reset rotation
        rotationX = 0f;
        rotationY = 0f;
        foreach (var camera in virtualCameras) {
            camera.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
        // Back view camera (backVirtualCamera) cannot be rotated so no reset is needed
        // Reset zoom
        if (!rotationOnly)
            ZoomIn(false);
    }

    public void TweenToResetView() {
        // Reset rotation
        isResetting = true;
        currentSensitivity = 0; // disable camera movement while the tween is running
        foreach (var camera in virtualCameras) {
            DOTween.To(() => rotationX, x => rotationX = x, 0f, resetDuration).SetEase(Ease.InOutCubic);
            DOTween.To(() => rotationY, y => rotationY = y, 0f, resetDuration).SetEase(Ease.InOutCubic)
                .OnComplete(() => { currentSensitivity = SettingsUI.mouseSensitivity; isResetting = false; }); // enable camera movement again
        }
        // Back view camera (backVirtualCamera) cannot be rotated so no reset is needed

        // If the current camera is back camera, then switch back to front one
        if (isBackViewOn) SwitchToFrontCamera();
    }

    // Changes the camera's FOV to zoom in/out
    public void ZoomIn(bool zoomIn) {
        float fov = zoomIn ? zoomedInFOV : defaultFOV;
        foreach (var camera in virtualCameras) {
            DOTween.To(() => camera.m_Lens.FieldOfView, x => camera.m_Lens.FieldOfView = x, fov, zoomDuration).SetEase(Ease.InOutCubic);
        }
    }

    public void Shake(float duration, float intensity) {
        foreach (var camera in virtualCameras)
            ShakeCamera(camera, duration, intensity);
        ShakeCamera(backVirtualCamera, duration, intensity);
    }

    private void ShakeCamera(CinemachineVirtualCamera camera, float duration, float intensity) {
        CinemachineBasicMultiChannelPerlin cameraNoise = camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        DOTween.To(() => cameraNoise.m_AmplitudeGain, x => cameraNoise.m_AmplitudeGain = x, intensity, duration / 2f).SetEase(Ease.InOutCubic)
            .OnComplete(() => DOTween.To(() => cameraNoise.m_AmplitudeGain, x => cameraNoise.m_AmplitudeGain = x, 0, duration / 2f)).SetEase(Ease.InOutCubic);
    }

    private void SwitchCameraIfNecessary() {
        // Handle switching the view
        if (GamePause.PauseState != GamePauseState.Running) return;
        if (enableViewSwitch && InputManager.Instance.GetBoolValue("View")) {
            int previousCameraIndex = currentCameraIndex;
            currentCameraIndex++;
            if (currentCameraIndex == virtualCameras.Count)
                currentCameraIndex = 0;
            currentCamera = virtualCameras[currentCameraIndex];
            virtualCameras[currentCameraIndex].gameObject.SetActive(true);
            virtualCameras[previousCameraIndex].gameObject.SetActive(false);
        }
        // Handle switching to the back view
        if (InputManager.Instance.GetBoolValue("BackView")) {
            if (isBackViewOn) SwitchToFrontCamera();
            else SwitchToBackCamera();
        }
    }

    private void SwitchToBackCamera() {
        // Switch to back camera and reset the front one
        isBackViewOn = true;
        currentCamera = backVirtualCamera;
        backVirtualCamera.gameObject.SetActive(true);
        virtualCameras[currentCameraIndex].gameObject.SetActive(false);
        ResetCameras(true);
    }

    private void SwitchToFrontCamera() {
        // Switch to front camera
        isBackViewOn = false;
        currentCamera = virtualCameras[currentCameraIndex];
        virtualCameras[currentCameraIndex].gameObject.SetActive(true);
        backVirtualCamera.gameObject.SetActive(false);
    }

    private void ResetCameraWithTweenIfNecessary() {
        if (isResetting || !InputManager.Instance.GetBoolValue("ResetView"))
            return;
        TweenToResetView();
    }

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
                virtualCameras[i].gameObject.SetActive(true);
            } else {
                virtualCameras[i].gameObject.SetActive(false);
            }
            virtualCameras[i].m_Lens.FieldOfView = defaultFOV;
        }
        // Reset the rotation
        currentCamera.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);

    }

    void LateUpdate()
    {
        // React to player input
        SwitchCameraIfNecessary();
        ResetCameraWithTweenIfNecessary();

        // Back view camera cannot be rotated
        if (isBackViewOn) return;

        // Gradually increase the sensitivity over the first "sensitivityEaseInDuration" seconds
        //  - at first the camera is following the mouse very slowly so it prevents quick jump at the beginning
        if (currentSensitivity < SettingsUI.mouseSensitivity) {
            currentT = Mathf.Clamp(currentT + Time.deltaTime / sensitivityEaseInDuration, 0, 1);
            currentSensitivity = currentT * currentT * currentT * currentT * SettingsUI.mouseSensitivity; // EaseInQuart
        }

        // Mouse inputs are already framerate independent - multiplying with Time.deltaTime would make it framerate dependent
        float mouseX = Input.GetAxis("Mouse X") * currentSensitivity * Time.timeScale; // multiplied by Time.timeScale to support game pause
        float mouseY = Input.GetAxis("Mouse Y") * currentSensitivity * Time.timeScale;

        // Limiting the angle of rotation
        rotationX = Mathf.Clamp(rotationX - mouseY, -maxAngleX, maxAngleX);
        rotationY = Mathf.Clamp(rotationY + mouseX, -maxAngleY, maxAngleY);

        // Rotating the current virtual camera
        currentCamera.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);

    }
}
