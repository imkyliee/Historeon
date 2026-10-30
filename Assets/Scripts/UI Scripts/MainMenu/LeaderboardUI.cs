
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;

public class LeaderboardUI : MonoBehaviour
{
    [Header("Leaderboard")]
    [SerializeField] private Transform scoresContainer;
    [SerializeField] private GameObject scoreRowPrefab;
    [SerializeField] private TMP_Text yourScoreText;

    private readonly List<GameObject> createdRows = new List<GameObject>();

    private Transform GetRowParent()
    {
        if (scoreRowPrefab == null)
            return scoresContainer;

        // If Scores is assigned as both fields, use its parent.
        if (scoresContainer == scoreRowPrefab.transform)
            return scoreRowPrefab.transform.parent;

        return scoresContainer;
    }

    private void OnEnable()
    {
        RefreshLeaderboard();
    }

    public void RefreshLeaderboard()
    {
        ClearRows();

        Transform rowParent = GetRowParent();

        if (rowParent == null || scoreRowPrefab == null)
        {
            Debug.LogError(
                "LeaderboardUI: Assign Scores as the row template and its parent as the container.");
            return;
        }

        string playFabId = PlayerPrefs.GetString("PlayFabId", "");

        if (string.IsNullOrEmpty(playFabId))
        {
            SetYourScore("Your rank is unavailable.\nPlease log in.");
            return;
        }

        if (PlayFabSaveManager.Instance == null)
        {
            SetYourScore("Leaderboard unavailable.");
            return;
        }

        SetYourScore("Loading your rank and score...");

        // Hide only the template, never the container.
        scoreRowPrefab.SetActive(false);

        PlayFabSaveManager.Instance.GetTopLeaderboard(entries =>
        {
            if (this == null || !isActiveAndEnabled)
                return;

            if (entries == null)
            {
                Debug.LogError("LeaderboardUI: Failed to retrieve leaderboard entries.");
                SetYourScore("Could not load leaderboard.");
                return;
            }

            Debug.Log("LeaderboardUI: Received " + entries.Count + " leaderboard entries.");

            foreach (PlayerLeaderboardEntry entry in entries)
            {
                string playerName = !string.IsNullOrWhiteSpace(entry.DisplayName)
                    ? entry.DisplayName
                    : "Player " + entry.PlayFabId.Substring(
                        0, Mathf.Min(6, entry.PlayFabId.Length));

                CreateScoreRow(
                    rowParent,
                    (entry.Position + 1) + ". " + playerName
                    + " - " + entry.StatValue + " points");
            }

            LoadYourScore();
        });
    }

    private void LoadYourScore()
    {
        string playFabId = PlayerPrefs.GetString("PlayFabId", "");

        PlayFabClientAPI.GetLeaderboardAroundPlayer(
            new GetLeaderboardAroundPlayerRequest
            {
                StatisticName = PlayFabSaveManager.TotalPointsStatisticName,
                MaxResultsCount = 1,
                PlayFabId = playFabId
            },
            result =>
            {
                if (this == null || !isActiveAndEnabled)
                    return;

                if (result.Leaderboard == null || result.Leaderboard.Count == 0)
                {
                    SetYourScore("YOUR RANK\nNo leaderboard score yet.");
                    return;
                }

                PlayerLeaderboardEntry player = result.Leaderboard[0];

                string username = !string.IsNullOrWhiteSpace(player.DisplayName)
                    ? player.DisplayName
                    : PlayerPrefs.GetString("Username", "Player");

                SetYourScore(
                        "#" + (player.Position + 1) + " - " + username
                        + " - " + player.StatValue + " points");
            },
            error =>
            {
                Debug.LogError(
                    "Failed to load your leaderboard rank: "
                    + error.GenerateErrorReport());

                SetYourScore("Could not load your rank.");
            });
    }

    private void CreateScoreRow(Transform rowParent, string text)
    {
        GameObject row = Instantiate(scoreRowPrefab, rowParent);
        row.name = "LeaderboardScoreRow";
        row.SetActive(true);

        TMP_Text rowText = row.GetComponentInChildren<TMP_Text>(true);

        if (rowText != null)
        {
            rowText.text = text;
        }
        else
        {
            Debug.LogWarning("LeaderboardUI: Row template has no TMP text.");
        }

        createdRows.Add(row);
    }

    private void ClearRows()
    {
        foreach (GameObject row in createdRows)
        {
            if (row != null)
                Destroy(row);
        }

        createdRows.Clear();
    }

    private void SetYourScore(string text)
    {
        if (yourScoreText != null)
            yourScoreText.text = text;
    }
}
