using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreatorUI : MonoBehaviour
{
    [SerializeField] Button continueButton;

    private string currentName;

    public void ReturnBack() {
        SceneLoader.Instance.LoadScene(Scene.MainMenu);
    }

    public void StartGame() {
        // TODO: Save all the choices (including the name)
        SceneLoader.Instance.LoadScene(Scene.Tutorial);
    }

    public void NameChanged(string name) {
        currentName = name;
        if (string.IsNullOrEmpty(name)) {
            continueButton.interactable = false;
        } else {
            continueButton.interactable = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
