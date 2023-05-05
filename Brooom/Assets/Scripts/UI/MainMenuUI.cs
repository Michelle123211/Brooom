using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{

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
}