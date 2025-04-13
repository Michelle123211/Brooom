using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class End : MonoBehaviour {

    public static string sceneName = "4-End";

    [SerializeField] CanvasGroup mainContent;
    [SerializeField] CanvasGroup exitButton;

	public void OnExitButtonClicked() {
		Application.Quit();
	}

	private void Start() {
		mainContent.alpha = 0;
		exitButton.alpha = 0;
		mainContent.DOFade(1f, 0.3f).OnComplete(() => exitButton.DOFade(1f, 0.3f));
	}

}
