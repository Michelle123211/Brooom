using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class KeyBindingUI : MonoBehaviour {

    [Tooltip("Label displaying the readable name of the action.")]
    [SerializeField] private TextMeshProUGUI label;
    [Tooltip("An object with a button to start rebinding the key.")]
    [SerializeField] private Button rebindingButton;
    [Tooltip("Label on the button starting rebinding.")]
    [SerializeField] private TextMeshProUGUI keyButtonText;
    [Tooltip("An object displayed when a rebinding is in process.")]
    [SerializeField] private GameObject waitingForInputText;
    [Tooltip("An object with a button to reset the binding.")]
    [SerializeField] private GameObject resetButton;

    private PlayerInput playerInput;

    private InputAction action;
    private InputActionReference actionReference;

    public void Initialize(InputAction action, string name) {
        this.action = action;
        label.text = name;
        keyButtonText.text = action.bindings[0].ToDisplayString();
	}

    public void MakeReadOnly() {
        rebindingButton.interactable = false;
        resetButton.SetActive(false);
    }

    public void StartRebinding() {
        rebindingButton.gameObject.SetActive(false);
        waitingForInputText.SetActive(true);

        // Deactivate the map
        playerInput.currentActionMap.Disable();

        action.PerformInteractiveRebinding()
            .WithControlsExcluding("");
    }

	// Start is called before the first frame update
	void Start()
    {
        playerInput = InputManager.Instance.GetPlayerInput();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
