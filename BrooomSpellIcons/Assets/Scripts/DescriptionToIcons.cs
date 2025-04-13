using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class DescriptionToIcons : ExperimentPart {

    public static string sceneName = "2-DescriptionToIcons";

    [Header("Introduction")]

    [SerializeField] CanvasGroup introductoryContent;

    [SerializeField] CanvasGroup directionsContent;
    [SerializeField] CanvasGroup exampleContent;
    [SerializeField] CanvasGroup whatsNextContent;

    [Header("Experiment")]
    [SerializeField] CanvasGroup experimentContent;
    [SerializeField] CanvasGroup spellContent;
    [SerializeField] TextMeshProUGUI spellDescriptionEnglish;
    [SerializeField] TextMeshProUGUI spellDescriptionCzech;
    [SerializeField] TMP_InputField iconDescriptionInputField;


    private DescriptionToIconsStep currentStep = DescriptionToIconsStep.Directions;
    private List<SpellDescription> spells;
    private int spellIndex = -1;

    private bool hideIntroductoryInfo = true;

    public override void OnNextButtonClicked() {
        nextButton.DOFade(0f, 0.3f);
        // Move to next step
        if (currentStep == DescriptionToIconsStep.Description) {
            // Save current data and remove the last spell
            DataLogger.Instance.Log($"Part 2 | {spells[spellIndex].spellName} | {iconDescriptionInputField.text}");
            spells.RemoveAt(spellIndex);
            // If there are no more icons, finish
            if (spells.Count <= 0) currentStep = DescriptionToIconsStep.Finished;
        } else {
            currentStep = (DescriptionToIconsStep)((int)currentStep + 1);
        }
        // Do something based on current step
        InitializeStep();
    }

    public void OnInputFieldValueChanged() {
        nextButton.interactable = !string.IsNullOrEmpty(iconDescriptionInputField.text);
    }

    private void InitializeStep() {
        switch (currentStep) {
            case DescriptionToIconsStep.Directions:
                // Hide everything
                ResetContent();
                // Show directions
                directionsContent.DOFade(1f, 0.3f).OnComplete(() => nextButton.DOFade(1f, 0.3f));
                break;
            case DescriptionToIconsStep.Example:
                // Show example
                exampleContent.DOFade(1f, 0.3f).OnComplete(() => nextButton.DOFade(1f, 0.3f));
                break;
            case DescriptionToIconsStep.WhatsNext:
                // Show what's next
                whatsNextContent.DOFade(1f, 0.3f).OnComplete(() => nextButton.DOFade(1f, 0.3f));
                break;
            case DescriptionToIconsStep.Description:
                // Hide introductory info (if not hidden already)
                if (hideIntroductoryInfo) {
                    hideIntroductoryInfo = false;
                    introductoryContent.DOFade(0f, 0.3f).OnComplete(() => experimentContent.DOFade(1f, 0.3f));
                }
                // Show next spell icon and clear input fields
                nextButton.interactable = false;
                InitializeNewSpell();
                break;
            case DescriptionToIconsStep.Finished:
                // Go to next scene
                DataLogger.Instance.Log("----------------------------------------", false);
                experimentContent.DOFade(0f, 0.3f).OnComplete(() => SceneManager.LoadScene(ConnectingPairs.sceneName));
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
        // Initialize content (set descriptions, clear input field)
        spellDescriptionEnglish.text = spells[spellIndex].spellDescriptionEnglish;
        spellDescriptionCzech.text = spells[spellIndex].spellDescriptionCzech;
        iconDescriptionInputField.text = string.Empty;
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

internal enum DescriptionToIconsStep {
    Directions,
    Example,
    WhatsNext,
    Description,
    Finished
}