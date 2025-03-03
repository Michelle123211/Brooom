using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GamePause : MonoBehaviour {

    public static GamePauseState pauseState = GamePauseState.Running;

    [Tooltip("Exposed to be used from animations to gradually decrease/increase.")]
    [SerializeField] float timeScale = 0f;

    private Animator animator;

    private bool menuVisible = false;

    public void PauseGame(bool showMenu = false) {
        // Pause
        pauseState = GamePauseState.Pausing;
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
        pauseState = GamePauseState.Resuming;
    }

    public void ExitGame() {
        // Change state to running
        pauseState = GamePauseState.Running;
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
            if (pauseState == GamePauseState.Running) {
                PauseGame(true);
            } else if (pauseState == GamePauseState.Paused) {
                ResumeGame();
            }
        }
        // Update time scale
        if (pauseState == GamePauseState.Pausing || pauseState == GamePauseState.Resuming) {
            Time.timeScale = timeScale;
        }
        // Update state
        if (pauseState == GamePauseState.Pausing && timeScale == 0)
            pauseState = GamePauseState.Paused;
        if (pauseState == GamePauseState.Resuming && timeScale == 1)
            pauseState = GamePauseState.Running;
    }
}

public enum GamePauseState { 
    Running,
    Pausing,
    Paused,
    Resuming
}