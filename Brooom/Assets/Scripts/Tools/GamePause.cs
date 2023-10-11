using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GamePause : MonoBehaviour {

    [Tooltip("Exposed to be used from animations to gradually decrease/increase.")]
    [SerializeField] float timeScale = 1;

    private Animator animator;
    public static GamePauseState pauseState = GamePauseState.Running;

    public void PauseGame() {
        // Pause
        pauseState = GamePauseState.Pausing;
        // Show pause menu
        animator.SetBool("ShowMenu", true);
        // Enable cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame() {
        // Disable cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Hide pause menu
        animator.SetBool("ShowMenu", false);
        // Resume
        pauseState = GamePauseState.Resuming;
    }

	private void Start() {
        animator = GetComponent<Animator>();
    }

	private void Update() {
        // Pause game if requested
        if (InputManager.Instance.GetBoolValue("Pause")) {
            if (pauseState == GamePauseState.Running) {
                PauseGame();
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