using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Introduction : ExperimentPart {

	public static string sceneName = "0-Introduction";

	[SerializeField] CanvasGroup welcomeContent;
	[SerializeField] CanvasGroup contextContent;
	[SerializeField] CanvasGroup experimentContent;

	private IntroductionStep currentStep = IntroductionStep.Welcome;

	public override void OnNextButtonClicked() {
		nextButton.DOFade(0f, 0.3f);
		// Move to next step
		currentStep = (IntroductionStep)((int)currentStep + 1);
		// Do something based on current step
		InitializeStep();
	}

	private void InitializeStep() {
		switch (currentStep) {
			case IntroductionStep.Welcome:
				// Hide everything
				nextButton.alpha = 0;
				welcomeContent.alpha = 0;
				contextContent.alpha = 0;
				experimentContent.alpha = 0;
				// Show welcome info
				welcomeContent.DOFade(1f, 0.3f).OnComplete(() => nextButton.DOFade(1f, 0.3f));
				break;
			case IntroductionStep.Context:
				// Hide welcome info, show context info
				welcomeContent.DOFade(0f, 0.3f).OnComplete(() => {
					contextContent.DOFade(1f, 0.3f).OnComplete(() => nextButton.DOFade(1f, 0.3f));
				});
				break;
			case IntroductionStep.Experiment:
				// Hide context info, show experiment info
				contextContent.DOFade(0f, 0.3f).OnComplete(() => {
					experimentContent.DOFade(1f, 0.3f).OnComplete(() => nextButton.DOFade(1f, 0.3f));
				});
				break;
			case IntroductionStep.Finished:
				// Go to next scene
				experimentContent.DOFade(0f, 0.3f).OnComplete(() => SceneManager.LoadScene(IconsToDescription.sceneName));
				break;
		}
	}

	private void Start() {
		InitializeStep();
	}

}

internal enum IntroductionStep { 
	Welcome,
	Context,
	Experiment,
	Finished
}