using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GamePause : MonoBehaviour {

    public static GamePauseState PauseState { get; private set; } = GamePauseState.Running;

    private static bool canBePaused = true;

    [Tooltip("Set to true if options specific for tutorial should be displayed.")]
    [SerializeField] bool isInTutorial = false;

    [Tooltip("Objects representing menu options which should be displayed when pausing the game in race.")]
    [SerializeField] List<GameObject> optionsInRace = new List<GameObject>();
    [Tooltip("Objects representing menu options which should be displayed when pausing the game in tutorial.")]
    [SerializeField] List<GameObject> optionsInTutorial = new List<GameObject>();

    [Tooltip("Exposed to be used from animations to gradually decrease/increase.")]
    [SerializeField] float timeScale = 0f;

    private Animator animator;

    private ShowHidePanelUI settings;

    private bool menuVisible = false;
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
        AudioManager.Instance.PauseGame(); // start pause menu audio
        // Show pause menu
        if (showMenu) {
            menuVisible = true;
            animator.SetBool("ShowMenu", true); // timeScale changed in animation
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelOpen);
            // Enable cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } else timeScale = 0;
    }

    public void ResumeGame() {
        if (!canBePaused) return;
        // Hide pause menu
        if (menuVisible) {
            menuVisible = false;
            animator.SetBool("ShowMenu", false); // timeScale changed in animation
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelClose);
            // Disable cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        } else timeScale = 1f;
        // Resume - only if there is no more nested pause
        pauseCount--;
        if (pauseCount == 0) {
            PauseState = GamePauseState.Resuming;
            AudioManager.Instance.ResumeGame(); // stop pause menu audio
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
        Time.timeScale = 1f;
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.Events.GUI.PanelClose);
        AudioManager.Instance.ResumeGame(); // stop pause menu audio
    }

	private void Awake() {
        // Initialize all values (pause state is not kept between scenes)
        PauseState = GamePauseState.Running;
        Time.timeScale = 1f;
        timeScale = 1f;
        pauseCount = 0;
        menuVisible = false;
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
                if (!menuVisible) {
                    PauseGame(true);
                } else {
                    ResumeGame();
                }
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