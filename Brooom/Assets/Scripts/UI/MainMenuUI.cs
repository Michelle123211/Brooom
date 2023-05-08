using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public LanguageButtonUI languageTogglePrefab;
    public Transform languagesParent;

    public void StartOrContinueGame() { 
        bool newGame = true;
        // TODO: If there is any previously saved state, load it here and set newGame to false
        if (newGame) {
            // Load the CharacterCreation scene
            SceneLoader.Instance.LoadScene(Scene.CharacterCreation);
        } else {
            // Load the PlayerOverview scene
            SceneLoader.Instance.LoadScene(Scene.PlayerOverview);
        }
    }

    public void RestartGame() {
        // TODO: Remove everything from the saved state
        // TODO: Reset the current state
        // Load the CharacterCreation scene
        SceneLoader.Instance.LoadScene(Scene.CharacterCreation);
    }

    public void ExitGame() {
        // TODO: Save the game state
        Application.Quit();
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
        // Don't forget to use localization correctly
	}
}