using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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
        // Show pause menu
        if (showMenu) {
            isPauseMenuVisible = true;
            AudioManager.Instance.PauseGame(); // start pause menu audio
            animator.SetBool("ShowMenu", true); // timeScale changed in animation
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelOpen);
            // Enable cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0f, 0.25f).SetUpdate(true);
    }

    public void ResumeGame() {
        if (!canBePaused) return;
        // Hide pause menu
        if (isPauseMenuVisible) {
            isPauseMenuVisible = false;
            AudioManager.Instance.ResumeGame(); // stop pause menu audio
            animator.SetBool("ShowMenu", false); // timeScale changed in animation
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelClose);
            // Disable cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        // Resume - only if there is no more nested pause
        pauseCount--;
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
        RaceController.Instance.GiveUpRace();
    }

    public void SkipTutorial() {
        Exit();
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
        if (InputManager.Instance.GetBoolValue("Restart")) {
            if (PauseState == GamePauseState.Running || PauseState == GamePauseState.Paused) { // not in the middle of pausing/resuming
                if (PauseState == GamePauseState.Running) PauseGame();
                else ResumeGame();
            }
        }
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