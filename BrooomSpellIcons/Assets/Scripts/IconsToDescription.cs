using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class IconsToDescription : ExperimentPart {

    public static string sceneName = "1-IconsToDescription";

    [Header("Introduction")]

    [SerializeField] CanvasGroup introductoryContent;

    [SerializeField] CanvasGroup directionsContent;
    [SerializeField] CanvasGroup exampleContent;
    [SerializeField] CanvasGroup whatsNextContent;

    [Header("Experiment")]
    [SerializeField] CanvasGroup experimentContent;
    [SerializeField] CanvasGroup spellContent;
    [SerializeField] Image iconBackground;
    [SerializeField] Image iconImage;
    [SerializeField] TMP_InputField formInputField;
    [SerializeField] TMP_InputField functionInputField;



    private IconsToDescriptionStep currentStep = IconsToDescriptionStep.Directions;
    private List<SpellDescription> spells;
    private int spellIndex = -1;

    private bool hideIntroductoryInfo = true;

    public override void OnNextButtonClicked() {
        nextButton.DOFade(0f, 0.3f);
        // Move to next step
        if (currentStep == IconsToDescriptionStep.Icon) {
            // Save current data and remove the last spell
            DataLogger.Instance.Log($"Part 1 | {spells[spellIndex].name} | {formInputField.text} | {functionInputField.text}");
            spells.RemoveAt(spellIndex);
            // If there are no more icons, finish
            if (spells.Count <= 0) currentStep = IconsToDescriptionStep.Finished;
        } else {
            currentStep = (IconsToDescriptionStep)((int)currentStep + 1);
        }
        // Do something based on current step
        InitializeStep();
    }

    private void InitializeStep() {
        switch (currentStep) {
            case IconsToDescriptionStep.Directions:
                // Hide everything
                ResetContent();
                // Show directions
                directionsContent.DOFade(1f, 0.3f).OnComplete(() => nextButton.DOFade(1f, 0.3f));
                break;
            case IconsToDescriptionStep.Example:
                // Show example
                exampleContent.DOFade(1f, 0.3f).OnComplete(() => nextButton.DOFade(1f, 0.3f));
                break;
            case IconsToDescriptionStep.WhatsNext:
                // Show what's next
                whatsNextContent.DOFade(1f, 0.3f).OnComplete(() => nextButton.DOFade(1f, 0.3f));
                break;
            case IconsToDescriptionStep.Icon:
                // Hide introductory info (if not hidden already)
                if (hideIntroductoryInfo) {
                    hideIntroductoryInfo = false;
                    introductoryContent.DOFade(0f, 0.3f).OnComplete(() => experimentContent.DOFade(1f, 0.3f));
                }
                // Show next spell icon and clear input fields
                InitializeNewSpell();
                break;
            case IconsToDescriptionStep.Finished:
                // Go to next scene
                experimentContent.DOFade(0f, 0.3f).OnComplete(() => SceneManager.LoadScene(DescriptionToIcons.sceneName));
                break;
        }
    }

    private void InitializeNewSpell() {
        // Choose a random spell
        spellIndex = Random.Range(0, spells.Count);
        // Hide all content, initialize it and show it again
        spellContent.DOFade(0f, 0.3f).OnComplete(() => {
            SetSpellContent();
            spellContent.DOFade(1f, 0.3f).OnComplete(() => nextButton.DOFade(1f, 0.3f));
        });
    }

    private void SetSpellContent() {
        // Initialize content (set icon, clear input fields)
        iconBackground.color = spells[spellIndex].spellIconBackgroundColor;
        iconImage.sprite = spells[spellIndex].spellIcon;
        formInputField.text = string.Empty;
        functionInputField.text = string.Empty;
    }

    private void ResetContent() {
        nextButton.alpha = 0;
        directionsContent.alpha = 0;
        exampleContent.alpha = 0;
        whatsNextContent.alpha = 0;
        introductoryContent.alpha = 1;
        experimentContent.alpha = 0;
        spellContent.alpha = 0;
    }

	private void Start() {
        SpellDescription[] spellsArray = Resources.LoadAll<SpellDescription>("Spells/");
        spells = new List<SpellDescription>(spellsArray); // create a list so we can remove already used items
        InitializeStep();
    }

}

internal enum IconsToDescriptionStep { 
    Directions,
    Example,
    WhatsNext,
    Icon,
    Finished
}
