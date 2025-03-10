using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaceResultsUI : MonoBehaviour {

    [Header("Results table")]
    [Tooltip("A parent object of all the results rows.")]
    [SerializeField] Transform resultRowsParent;
    [Tooltip("A prefab of a results table row which is instantiated several times.")]
    [SerializeField] RaceResultRowUI resultRowPrefab;

    int playerIndex;

    public void GoToPlayerOverview() {
        SceneLoader.Instance.LoadScene(Scene.PlayerOverview);
    }

    public void SetResultsTable(RaceResultData[] results, int playerPlace) {
        playerIndex = playerPlace - 1;
        // Remove all existing rows from the table
        UtilsMonoBehaviour.RemoveAllChildren(resultRowsParent);
        // Instantiate rows according to the results
        for (int i = 0; i < results.Length; i++) {
            RaceResultRowUI row = Instantiate<RaceResultRowUI>(resultRowPrefab, resultRowsParent);
            row.Initialize(i + 1, results[i]);
            // Highlight player
            if (i == playerIndex)
                row.HighlightPlayer();
        }
    }

    public void ShowResults() {
        StartCoroutine(nameof(ShowResultsOneByOne));
    }

    private IEnumerator ShowResultsOneByOne() {
        for (int i = 0; i < resultRowsParent.childCount; i++) {
            AudioManager.Instance.PlayOneShot(
                i == playerIndex
                    ? AudioManager.Instance.Events.GUI.RaceResultPlayer
                    : AudioManager.Instance.Events.GUI.RaceResult
            );
            resultRowsParent.GetChild(i).gameObject.TweenAwareEnable();
            yield return new WaitForSeconds(0.2f);
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
    public float timePenalization;
    public int coinsReward;
    public int coinsPenalization;
}