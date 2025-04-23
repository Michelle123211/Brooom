using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;


/// <summary>
/// A class controlling camera based on player's input.
/// Is capable of rotating camera based on mouse movement, switching between multiple virtual cameras when corresponding key is pressed (and if enabled), 
/// switching to back view and resetting view back to the default virtual camera and default orientation.
/// </summary>
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

    private bool isResetting = false; // reset takes some time (because of tween), ths prevents invoking another reset when it is already happening

    private bool rotationEnabled = true;

    /// <summary>
    /// Immediately resets all virtual cameras to their default values (zoom, rotation).
    /// </summary>
    /// <param name="rotationOnly">Whether only rotation should be reset, or zoom too.</param>
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

    /// <summary>
    /// Gradually over time resets all virtual cameras to their default orientation, and also switches from back view to front view.
    /// While this is happening, camera movement on player's input is temporarily disabled.
    /// </summary>
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

	/// <summary>
	/// Changes the camera's FOV to zoom in, or out. Back view camera is excluded.
	/// </summary>
	/// <param name="zoomIn">Whether the camera should zoom in or out (to a predefined FOV).</param>
	public void ZoomIn(bool zoomIn) {
        float fov = zoomIn ? zoomedInFOV : defaultFOV;
        foreach (var camera in virtualCameras) {
            DOTween.To(() => camera.m_Lens.FieldOfView, x => camera.m_Lens.FieldOfView = x, fov, zoomDuration).SetEase(Ease.InOutCubic);
        }
    }

    /// <summary>
    /// Shakes the camera (whichever one is currently active) for a short duration of time.
    /// </summary>
    /// <param name="duration">For how many seconds the camera should be shaking.</param>
    /// <param name="intensity">The intensity of the camera shake.</param>
    public void Shake(float duration, float intensity) {
        foreach (var camera in virtualCameras)
            ShakeCamera(camera, duration, intensity);
        ShakeCamera(backVirtualCamera, duration, intensity);
    }

    /// <summary>
    /// Enables camera rotation based on player's input.
    /// </summary>
    public void EnableRotation() {
        rotationEnabled = true;
    }
    /// <summary>
    /// Disables camera rotation, so that player's input cannot affect it. It also resets all cameras
    /// (tweening them into their default orientation and switching from back view to front view if necessary).
    /// </summary>
    public void DisableRotation() {
        rotationEnabled = false;
        TweenToResetView();
    }

    // Shakes the given virtual camera for the given duration with the given intensity
    private void ShakeCamera(CinemachineVirtualCamera camera, float duration, float intensity) {
        CinemachineBasicMultiChannelPerlin cameraNoise = camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        DOTween.To(() => cameraNoise.m_AmplitudeGain, x => cameraNoise.m_AmplitudeGain = x, intensity, duration / 2f).SetEase(Ease.InOutCubic)
            .OnComplete(() => DOTween.To(() => cameraNoise.m_AmplitudeGain, x => cameraNoise.m_AmplitudeGain = x, 0, duration / 2f)).SetEase(Ease.InOutCubic);
    }

    // Switches between different virtual cameras based on the player's input
    private void SwitchCameraIfNecessary() {
        // Handle switching the view
        if (GamePause.PauseState != GamePauseState.Running) return; // only if the game is running
        if (enableViewSwitch && InputManager.Instance.GetBoolValue("View")) {
            int previousCameraIndex = currentCameraIndex;
            // Switch to the next camera in the list (loop over the end back to the start if necessary
            currentCameraIndex++;
            if (currentCameraIndex == virtualCameras.Count)
                currentCameraIndex = 0;
            currentCamera = virtualCameras[currentCameraIndex];
            virtualCameras[currentCameraIndex].gameObject.SetActive(true);
            virtualCameras[previousCameraIndex].gameObject.SetActive(false);
        }
        // Handle switching to the back view
        if (InputManager.Instance.GetBoolValue("BackView")) {
            if (isBackViewOn) {
                Analytics.Instance.LogEvent(AnalyticsCategory.Other, "Back view disabled.");
                SwitchToFrontCamera();
            } else {
                Analytics.Instance.LogEvent(AnalyticsCategory.Other, "Back view enabled.");
                SwitchToBackCamera();
            }
        }
    }

    // Switches to the virtual camera for back view (and resets all other cameras rotation)
    private void SwitchToBackCamera() {
        // Switch to back camera and reset the front one
        isBackViewOn = true;
        currentCamera = backVirtualCamera;
        backVirtualCamera.gameObject.SetActive(true);
        virtualCameras[currentCameraIndex].gameObject.SetActive(false);
        ResetCameras(true);
    }

    // Switches back to the virtual camera for front view (the one which was active before switching to back view)
    private void SwitchToFrontCamera() {
        // Switch to front camera
        isBackViewOn = false;
        currentCamera = virtualCameras[currentCameraIndex];
        virtualCameras[currentCameraIndex].gameObject.SetActive(true);
        backVirtualCamera.gameObject.SetActive(false);
    }

    // Tweens all virtual cameras to their default orientation if the player has pressed the corresponding key (and the cameras are not already being reset)
    // Also switches from back view to front view
    private void ResetCameraWithTweenIfNecessary() {
        if (isResetting || !InputManager.Instance.GetBoolValue("ResetView"))
            return;
        Analytics.Instance.LogEvent(AnalyticsCategory.Other, "View reset.");
        if (isBackViewOn) Analytics.Instance.LogEvent(AnalyticsCategory.Other, "Back view disabled.");
        TweenToResetView();
    }

    // Start is called before the first frame update
    void Start() {
        // Lock the cursor to the center of the screen and hide it
        Utils.DisableCursor();
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

    // React to player's input and switch/rotate cameras accordingly
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

        // Don't continue if camera rotation is disabled
        if (!rotationEnabled) return;

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
