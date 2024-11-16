using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaceResultsUI : MonoBehaviour {
    
    [Header("Main part")]
    [Tooltip("A label displaying player's final standing.")]
    [SerializeField] TextMeshProUGUI placeText;
    [Tooltip("A label displaying total number of racers.")]
    [SerializeField] TextMeshProUGUI totalRacersText;

    [Tooltip("A label displaying time.")]
    [SerializeField] TextMeshProUGUI timeText;

    [Header("Penalizations")]
    [Tooltip("An object containing all the elements related to penalizations.")]
    [SerializeField] GameObject penalizationsObject;
    [Tooltip("A label displaying penalization for missed hoops.")]
    [SerializeField] TextMeshProUGUI missedHoopsText;
    [Tooltip("A label displaying penalization for exposing magic.")]
    [SerializeField] TextMeshProUGUI exposingMagicText;
    [Tooltip("An object containing all the elements related to penalization for exposing magic.")]
    [SerializeField] GameObject exposingMagicObject;

    [Header("Results table")]
    [Tooltip("A parent object of all the results rows.")]
    [SerializeField] Transform resultRowsParent;
    [Tooltip("A prefab of a results table row which is instantiated several times.")]
    [SerializeField] RaceResultRowUI resultRowPrefab;

    int playerPlace = 0;
    Color[] placeColors = new Color[] { // TODO: Move to a separate color palette
        Utils.ColorFromRBG256(243, 217, 81), // gold
        Utils.ColorFromRBG256(164, 164, 164), // silver
        Utils.ColorFromRBG256(203, 128, 83), // bronze
        Utils.ColorFromRBG256(126, 92, 80) };

    public void GoToPlayerOverview() {
        SceneLoader.Instance.LoadScene(Scene.PlayerOverview);
    }

    public void SetPlace(int place, int totalRacers) {
        this.playerPlace = place;
        placeText.text = place.ToString();
        // Change text color according to the place
        int colorIndex = place - 1;
        if (colorIndex >= placeColors.Length) colorIndex = placeColors.Length - 1;
        placeText.color = placeColors[colorIndex];

        totalRacersText.text = "/" + totalRacers;
    }

    public void SetTime(float timeInSeconds) {
        timeText.text = string.Format(LocalizationManager.Instance.GetLocalizedString("ResultsLabelTime"), Utils.FormatTime(timeInSeconds));
    }

    public void SetPenalization(int secondsForHoops = 0, int coinsForExposure = 0) {
        if (secondsForHoops == 0 && coinsForExposure == 0) // there is no penalization
            penalizationsObject.SetActive(false);
        else { // there is some penalization
            if (secondsForHoops != 0) { // penalization for missing hoops
                missedHoopsText.text = "+" + string.Format(LocalizationManager.Instance.GetLocalizedString("ResultsLabelHoops"), secondsForHoops);
            }
            if (coinsForExposure != 0) { // penalization for exposing magic
                exposingMagicText.text = "-" + coinsForExposure.ToString();
            }
            // Set correct visibility
            missedHoopsText.gameObject.SetActive(secondsForHoops != 0);
            exposingMagicObject.SetActive(coinsForExposure != 0);
            penalizationsObject.SetActive(true);
        }
    }

    public void SetResultsTable(RaceResultData[] results) {
        // Remove all existing rows from the table
        UtilsMonoBehaviour.RemoveAllChildren(resultRowsParent);
        // Instantiate rows according to the results
        for (int i = 0; i < results.Length; i++) {
            RaceResultRowUI row = Instantiate<RaceResultRowUI>(resultRowPrefab, resultRowsParent);
            row.Initialize(i + 1, results[i]);
            // Highlight player
            if (i + 1 == playerPlace)
                row.HighlightPlayer();
        }
    }

	private void OnEnable() {
        // Enable cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

	private void Update() {
        // Enable cursor
        if (Cursor.lockState != CursorLockMode.None) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}

public struct RaceResultData {
    public Color color;
    public string name;
    public float time;
    public float penalization;
    public int coinsReward;
}