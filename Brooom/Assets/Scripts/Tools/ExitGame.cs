using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A component which simply exits the game from its <c>Start()</c> method.
/// It can be used in the final scene which is empty and is there only to make sure everything gets destroyed in a correct order.
/// </summary>
public class ExitGame : MonoBehaviour {

    void Start() {
        // Exit the game
        Application.Quit();
    }

}
