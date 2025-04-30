using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


/// <summary>
/// A component representing UI of the Main Menu.
/// It initializes language toggles and main menu buttons, and tweens the content in.
/// It also contains methods which are invoked when menu buttons are pressed.
/// </summary>
public class MainMenuUI : MonoBehaviour {

    [Tooltip("A prefab of a language toggle.")]
    [SerializeField] LanguageButtonUI languageTogglePrefab;
    [Tooltip("A parent object of all language toggles.")]
    [SerializeField] Transform languagesParent;

    [Tooltip("Text of the button to start playing the game which is hidden if previously saved state exists.")]
    [SerializeField] GameObject startButtonText;
    [Tooltip("Text of the button to continue playing from previously saved state which is hidden when no saved state exists.")]
    [SerializeField] GameObject continueButtonText;
    [Tooltip("Button to start the game again from the beginning (and not previously saved state) which is hidden when no saved state exists.")]
    [SerializeField] GameObject startAgainButton;

    [Tooltip("A list of UI elements in an order in which they should gradually appear (e.g. logo first, then Start button).")]
    [SerializeField] List<CanvasGroup> elementsToAppear;

    private bool saveExists = false;

    /// <summary>
    /// Starts the game, either by continuing from a previously saved state (if it exists), or by starting a new game (if there is no save).
    /// Makes sure correct scene is loaded (also considering tutorial in progress).
    /// </summary>
    public void StartOrContinueGame() { 
        // If there is any previously saved state, load it here
        if (saveExists) {
            // If there is a backup available (e.g. because quick race was run previously), first restore it
            if (SaveSystem.BackupExists()) SaveSystem.RestoreFromBackup();
            PlayerState.Instance.LoadSavedState();
            Analytics.Instance.LogEvent(AnalyticsCategory.Game, "Game continued.");
            Messaging.SendMessage("GameStarted");
            // If tutorial is enabled and first two stages haven't been finished yet, load appropriate scene
            if (SettingsUI.enableTutorial && Tutorial.Instance.CurrentStage == TutorialStage.Introduction)
                SceneLoader.Instance.LoadScene(Scene.Tutorial); // Introduction tutorial takes place in Tutorial scene
            else if (SettingsUI.enableTutorial && Tutorial.Instance.CurrentStage == TutorialStage.FirstRace)
                SceneLoader.Instance.LoadScene(Scene.Race); // First Race tutorial takes place in Race scene
            // Otherwise, load the PlayerOverview scene
            else SceneLoader.Instance.LoadScene(Scene.PlayerOverview);
        } else {
            // Load the CharacterCreation scene
            Analytics.Instance.LogEvent(AnalyticsCategory.Game, "New game started from menu.");
            SceneLoader.Instance.LoadScene(Scene.CharacterCreation);
        }
    }

    /// <summary>
    /// Starts a new game by loading the Character Creation scene.
    /// </summary>
    public void RestartGame() {
        // The saved state and current state will be reset after the new character is created
        //  - this gives the player an option to go back without losing the data.
        // Load the CharacterCreation scene
        Analytics.Instance.LogEvent(AnalyticsCategory.Game, "New game started from menu.");
        SceneLoader.Instance.LoadScene(Scene.CharacterCreation);
    }

    /// <summary>
    /// Saves the current game state and exits the game.
    /// </summary>
    public void ExitGame() {
        // Save the game state
        if (SaveSystem.BackupExists()) SaveSystem.RestoreFromBackup(); // quick race settings were opened, so the correct state is in backup
        else PlayerState.Instance.SaveCurrentState();
        // Load the Exit scene (so that all the GameObjects are unloaded except for these in DontDestroyOnLoad)
        //  - this way the singletons will be destroyed last
        Analytics.Instance.LogEvent(AnalyticsCategory.Game, "Game exited.");
        SceneLoader.Instance.LoadScene(Scene.Exit, true, false);
    }

    /// <summary>
    /// Logs analytics event saying that the About screen was opened or closed.
    /// </summary>
    /// <param name="opened"><c>true</c> if the About screen was opened, <c>false</c> if closed.</param>
    public void LogAboutGameOpenedOrClosed(bool opened = true) {
        Analytics.Instance.LogEvent(AnalyticsCategory.Game, $"About Game {(opened ? "opened" : "closed")}.");
    }

    // Gradually shows the Main Menu content after the Main Menu scene was loaded (ensuring it starts appearing when loading screen is no longer there)
    private void OnSceneLoaded(Scene scene) {
        if (scene == Scene.MainMenu) ShowMenuContent();
    }

    // Gradually shows the Main Menu content (in an order defined by elementsToAppear list)
    private void ShowMenuContent() {
        // Start tweening menu content (gradually show different UI elements)
        Sequence sequence = DOTween.Sequence();
        bool isFirst = true;
        foreach (var element in elementsToAppear) {
            if (isFirst) { // first element will take longer to appear because it is the most important one
                sequence.Append(element.DOFade(1, 0.5f));
                isFirst = false;
            }  else sequence.Append(element.DOFade(1, 0.2f));
        }
        sequence.Play();
    }

	private void Start() {
        // Initialize the language buttons according to the available languages
        string[] languages = LocalizationManager.Instance.GetAvailableLanguages();
        Toggle defaultLangToggle = null;
        foreach (var lang in languages) {
            // Create an instance of the language toggle
            LanguageButtonUI langButton = Instantiate<LanguageButtonUI>(languageTogglePrefab, languagesParent);
            // Initialize it 
            Toggle langToggle = langButton.GetComponent<Toggle>();
            langToggle.group = languagesParent.GetComponent<ToggleGroup>();
            Sprite flagSprite = Resources.Load<Sprite>($"Flags/{lang}");
            langButton.Initialize(lang, flagSprite);
            if (LocalizationManager.Instance.CurrentLanguage == lang)
                defaultLangToggle = langToggle;
        }
        // Activate the button corresponding to the current language
        defaultLangToggle.isOn = true;
        // Show correctly buttons to Start, Continue and/or Start again
        if (SaveSystem.SaveExists()) {  // there is previously saved game
            saveExists = true;
            startButtonText.SetActive(false);
            continueButtonText.SetActive(true);
            startAgainButton.SetActive(true);
        } else { // there is no previously saved game
            startButtonText.SetActive(true);
            continueButtonText.SetActive(false);
            startAgainButton.SetActive(false);
        }
        // Initialize settings values (from saved state)
        UtilsMonoBehaviour.FindObject<SettingsUI>().LoadSettingsValues();
        // Make sure menu content is tweened in
        SceneLoader.Instance.onSceneLoaded += OnSceneLoaded;
        foreach (var element in elementsToAppear) element.alpha = 0;
    }

	private void OnDestroy() {
        SceneLoader.Instance.onSceneLoaded -= OnSceneLoaded;
    }
}