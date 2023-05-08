using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingSceneUI : MonoBehaviour
{
    [Tooltip("How much time should pass before the number of dots is updated.")]
    [SerializeField] private float durationOfOneDot = 0.3f;

    [SerializeField] private TextMeshProUGUI loadingText;

    private float timeout;
    private int numberOfDots = 0;

    private void Start() {
        timeout = durationOfOneDot;
        loadingText.text = LocalizationManager.Instance.GetLocalizedString("LoadingLabel");
    }


	// Update is called once per frame
	void Update()
    {
        timeout -= Time.deltaTime;
        if (timeout < 0) {
            numberOfDots = Utils.Wrap(numberOfDots + 1, 0, 3);
            if (numberOfDots == 0)
                loadingText.text = LocalizationManager.Instance.GetLocalizedString("LoadingLabel");
            else
                loadingText.text += ".";
            timeout += durationOfOneDot;
        }
    }
}
