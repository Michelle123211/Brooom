using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class GamePause : MonoBehaviour {

    public static GamePauseState PauseState { get; private set; } = GamePauseState.Running;
    public static bool isPauseMenuVisible = false;

    private static bool canBePaused = true;

    [Tooltip("Set to true if options specific for tutorial should be displayed.")]
    [SerializeField] bool isInTutorial = false;

    [Tooltip("Objects representing menu options which should be displayed when pausing the game in race.")]
    [SerializeField] List<GameObject> optionsInRace = new List<GameObject>();
    [Tooltip("Objects representing menu options which should be displayed when pausing the game in tutorial.")]
    [SerializeField] List<GameObject> optionsInTutorial = new List<GameObject>();

    private Animator animator;

    private ShowHidePanelUI settings;

    private int pauseCount = 0; // increment when paused, decrement when unpaused - allows for nested pauses (with correct unpausing)
    private bool wasCursorLocked = false; // before opening pause menu, rememeber cursor state so it can be restored correctly after closing the menu

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
        pauseCount++;
        // Show pause menu and enable cursor
        if (showMenu) {
            isPauseMenuVisible = true;
            AudioManager.Instance.PauseGame(); // start pause menu audio
            animator.SetBool("ShowMenu", true); // timeScale changed in animation
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelOpen);
            wasCursorLocked = Utils.IsCursorLocked();
            Utils.EnableCursor();
            Analytics.Instance.LogEvent(AnalyticsCategory.Race, "Game paused.");
        }
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0f, 0.25f).SetUpdate(true);
    }

    public void ResumeGame() {
        if (!canBePaused) return;
        // Hide pause menu and restore cursor
        if (isPauseMenuVisible) {
            isPauseMenuVisible = false;
            AudioManager.Instance.ResumeGame(); // stop pause menu audio
            animator.SetBool("ShowMenu", false); // timeScale changed in animation
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelClose);
            if (wasCursorLocked) Utils.DisableCursor();
            Analytics.Instance.LogEvent(AnalyticsCategory.Race, "Game resumed.");
        }
        // Resume - only if there is no more nested pause
        pauseCount = Math.Max(0, pauseCount - 1); // don't go below zero (allows for game being resumed speculatively, e.g. when skipping tutorial)
        if (pauseCount == 0) {
            PauseState = GamePauseState.Resuming;
            DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, 1.2f).SetDelay(0.4f).SetUpdate(true);
        }
    }

    public void OpenSettings() {
        DisableGamePause();
        settings.ShowPanel();
    }

    public void GiveUpRace() {
        Exit();
        RaceControllerBase.Instance.GiveUpRace();
    }

    public void SkipTutorial() {
        ResumeGame();
        Tutorial.Instance.SkipCurrentTutorialStage();
    }

    public void LeaveTutorial() {
        Tutorial.Instance.LeaveCurrentTutorialStage();
        Exit();
        SceneLoader.Instance.LoadScene(Scene.MainMenu);
    }

    public void SetupOptionsForRace() {
        isInTutorial = false;
        // Disable all other options
        foreach (var option in optionsInTutorial)
            option.SetActive(false);
        // Enable options for pause in race
        foreach (var option in optionsInRace)
            option.SetActive(true);
    }

    public void SetupOptionsForTutorial() {
        isInTutorial = true;
        // Disable all other options
        foreach (var option in optionsInRace)
            option.SetActive(false);
        // Enable options for pause in tutorial
        foreach (var option in optionsInTutorial)
            option.SetActive(true);
    }

    private void Exit() {
        // Change state to running
        PauseState = GamePauseState.Running;
        isPauseMenuVisible = false;
        Time.timeScale = 1f;
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelClose);
        AudioManager.Instance.ResumeGame(); // stop pause menu audio
    }

	private void Awake() {
        // Initialize all values (pause state is not kept between scenes)
        PauseState = GamePauseState.Running;
        Time.timeScale = 1f;
        pauseCount = 0;
        isPauseMenuVisible = false;
        // Initialize options
        if (isInTutorial) SetupOptionsForTutorial();
        else SetupOptionsForRace();
    }

	private void Start() {
        animator = GetComponent<Animator>();
        settings = UtilsMonoBehaviour.FindObject<SettingsUI>().GetComponent<ShowHidePanelUI>();
    }

	private void Update() {
        // Pause game if requested (with pause menu)
        if (InputManager.Instance.GetBoolValue("Pause")) {
            if (PauseState == GamePauseState.Running || PauseState == GamePauseState.Paused) { // not in the middle of pausing/resuming
                if (!isPauseMenuVisible) {
                    PauseGame(true);
                } else {
                    ResumeGame();
                }
            }
        }
        // Update state
        if (PauseState == GamePauseState.Pausing && Time.timeScale == 0)
            PauseState = GamePauseState.Paused;
        if (PauseState == GamePauseState.Resuming && Time.timeScale == 1)
            PauseState = GamePauseState.Running;
    }
}

public enum GamePauseState { 
    Running,
    Pausing,
    Paused,
    Resuming
}