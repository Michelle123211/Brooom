using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public LanguageButtonUI languageTogglePrefab;
    public Transform languagesParent;

    private bool saveExists = false;

    public void StartOrContinueGame() { 
        // If there is any previously saved state, load it here
        if (saveExists) {
            PlayerState.Instance.LoadSavedState();
            // Load the PlayerOverview scene
            SceneLoader.Instance.LoadScene(Scene.PlayerOverview);
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
        // TODO: Change the text on the first main button to either START or CONTINUE (according to the saved state)
        if (PlayerPrefs.HasKey("GameStarted") && PlayerPrefs.GetString("GameStarted") == "true") { // there is previously saved game
            saveExists = true;
        } else { // there is no previously saved game
        
        }
        // Don't forget to use localization correctly
	}
}