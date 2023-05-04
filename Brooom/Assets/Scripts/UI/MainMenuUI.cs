using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{

    public void StartOrContinueGame() { 
        bool newGame = true;
        // TODO: If there is any previously saved state, load it here and set newGame to false
        if (newGame) {
            // TODO: Load the CharacterCreation scene
            Debug.Log("Start game.");
        } else {
            // TODO: Load the PlayerOverview scene
            Debug.Log("Continue game.");
        }
    }

    public void RestartGame() {
        // TODO: Remove everything from the saved state
        // TODO: Reset the current state
        // TODO: Load the CharacterCreation scene
        Debug.Log("Restart game.");
    }

    public void ExitGame() {
        // TODO: Save the game state
        Debug.Log("Exit game.");
        Application.Quit();
    }
}