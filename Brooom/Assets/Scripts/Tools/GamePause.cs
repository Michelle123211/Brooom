using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GamePause : MonoBehaviour {

    public static GamePauseState PauseState { get; private set; } = GamePauseState.Running;

    private static bool canBePaused = true;

    [Tooltip("Exposed to be used from animations to gradually decrease/increase.")]
    [SerializeField] float timeScale = 0f;

    private Animator animator;

    private bool menuVisible = false;


    // The following two methods may be used to disable pause when not desirable - e.g. when loading screen is on or after a race is finished
    public static void EnableGamePause() {
        canBePaused = true;
    }
    public static void DisableGamePause() {
        canBePaused = false;
    }

    public void PauseGame(bool showMenu = false) {
        if (!canBePaused) return;
        // Pause
        PauseState = GamePauseState.Pausing;
        // Show pause menu
        if (showMenu) {
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelOpen);
            AudioManager.Instance.PauseGame(); // start pause menu audio
            animator.SetBool("ShowMenu", true); // timeScale changed in animation
        } else timeScale = 0;
        menuVisible = true;
        // Enable cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame() {
        if (!canBePaused) return;
        // Disable cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Hide pause menu
        if (menuVisible) {
            animator.SetBool("ShowMenu", false); // timeScale changed in animation
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelClose);
            AudioManager.Instance.ResumeGame(); // stop pause menu audio
        } else timeScale = 1f;
        // Resume
        PauseState = GamePauseState.Resuming;
    }

    public void ExitGame() {
        // Change state to running
        PauseState = GamePauseState.Running;
        Time.timeScale = 1f;
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelClose);
        AudioManager.Instance.ResumeGame(); // stop pause menu audio
    }

	private void Start() {
        animator = GetComponent<Animator>();
    }

	private void Update() {
        // Pause game if requested
        if (InputManager.Instance.GetBoolValue("Pause")) {
            if (PauseState == GamePauseState.Running) {
                PauseGame(true);
            } else if (PauseState == GamePauseState.Paused) {
                ResumeGame();
            }
        }
        // Update time scale
        if (PauseState == GamePauseState.Pausing || PauseState == GamePauseState.Resuming) {
            Time.timeScale = timeScale;
        }
        // Update state
        if (PauseState == GamePauseState.Pausing && timeScale == 0)
            PauseState = GamePauseState.Paused;
        if (PauseState == GamePauseState.Resuming && timeScale == 1)
            PauseState = GamePauseState.Running;
    }
}

public enum GamePauseState { 
    Running,
    Pausing,
    Paused,
    Resuming
}