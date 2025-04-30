using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


/// <summary>
/// A component representing and controlling a loading screen which is displayed while a different scene is being loaded.
/// It should be attached to an object persistent between scenes (e.g. <c>SceneLoader</c>).
/// </summary>
public class LoadingSceneUI : MonoBehaviour {

    [Tooltip("How much time should pass before the number of dots in a label is updated.")]
    [SerializeField] private float durationOfOneDot = 0.3f;
    [Tooltip("Label displaying a localized text saying that it is loading.")]
    [SerializeField] private TextMeshProUGUI loadingText;

    private FMODUnity.StudioEventEmitter loadingSound;

    private float timeout;
    private int numberOfDots = 0;
    private string loadingString = "";


	void Update() {
        // Update dots in the loading text
        timeout -= Time.unscaledDeltaTime;
        if (timeout < 0) {
            numberOfDots = Utils.Wrap(numberOfDots + 1, 0, 3);
            if (numberOfDots == 0)
                loadingText.text = loadingString;
            else
                loadingText.text += ".";
            timeout += durationOfOneDot;
        }
    }

    private void OnEnable() {
        // Initialize everything
        timeout = durationOfOneDot;
        loadingString = LocalizationManager.Instance.GetLocalizedString("LoadingLabel");
        loadingText.text = loadingString;
        if (loadingSound == null) loadingSound = GetComponent<FMODUnity.StudioEventEmitter>();
        loadingSound.Play();
    }

    private void OnDisable() {
        // Finalize everything
        loadingSound.Stop();
	}

}
