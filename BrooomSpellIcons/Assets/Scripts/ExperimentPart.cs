using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ExperimentPart : MonoBehaviour {

	[SerializeField] protected CanvasGroup nextButton;

	public abstract void OnNextButtonClicked();

}
