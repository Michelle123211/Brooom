using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] LanguageButtonUI languageTogglePrefab;
    [SerializeField] Transform languagesParent;

    [Tooltip("Text of the button to start playing the game which is hidden if previously saved state exists.")]
    [SerializeField] GameObject startButtonText;
    [Tooltip("Text of the button to continue playing from previously saved state which is hidden when no saved state exists.")]
    [SerializeField] GameObject continueButtonText;
    [Tooltip("Button to start the game again from the beginning (and not previously saved state) which is hidden when no saved state exists.")]
    [SerializeField] GameObject startAgainButton;

    private bool saveExists = false;

    public void StartOrContinueGame() { 
        // If there is any previously saved state, load it here
        if (saveExists) {
            PlayerState.Instance.LoadSavedState();
            Messaging.SendMessage("GameStarted");
            // If tutorial is enabled and some stages haven't been finished yet, load appropriate scene
            if (SettingsUI.enableTutorial && Tutorial.Instance.CurrentStage == TutorialStage.Introduction)
                SceneLoader.Instance.LoadScene(Scene.Tutorial); // Introduction tutorial takes place in Tutorial scene
            else if (SettingsUI.enableTutorial && Tutorial.Instance.CurrentStage == TutorialStage.FirstRace)
                SceneLoader.Instance.LoadScene(Scene.Race); // First Race tutorial takes place in Race scene
            else if (SettingsUI.enableTutorial && Tutorial.Instance.CurrentStage == TutorialStage.CastSpells)
                SceneLoader.Instance.LoadScene(Scene.Tutorial); // CastSpells tutorial takes place in Tutorial scene
            // Otherwise, load the PlayerOverview scene
            else SceneLoader.Instance.LoadScene(Scene.PlayerOverview);
        } else {
            // Load the CharacterCreation scene
            SceneLoader.Instance.LoadScene(Scene.CharacterCreation);
        }
    }

    public void RestartGame() {
        // The saved state and current state will be reset after the new character is created
        //      This gives the player an option to go back without losing the data
        // Load the CharacterCreation scene
        SceneLoader.Instance.LoadScene(Scene.CharacterCreation);
    }

    public void ExitGame() {
        // Save the game state
        PlayerState.Instance.SaveCurrentState();
        // Load the Exit scene (so that all the GameObjects are unloaded except for these in DontDestroyOnLoad)
        // This way the singletons will be destroyed last
        SceneLoader.Instance.LoadScene(Scene.Exit, true, false);
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
        if (PlayerPrefs.HasKey("GameStarted") && PlayerPrefs.GetString("GameStarted") == "true") { // there is previously saved game
            saveExists = true;
            startButtonText.SetActive(false);
            continueButtonText.SetActive(true);
            startAgainButton.SetActive(true);
        } else { // there is no previously saved game
            startButtonText.SetActive(true);
            continueButtonText.SetActive(false);
            startAgainButton.SetActive(false);
        }
        // Initialize settings values
        UtilsMonoBehaviour.FindObject<SettingsUI>().LoadSettingsValues();
    }
}