
using UnityEngine;
using TMPro;
using PlayFab.ClientModels;

public class Top3LeaderboardUI : MonoBehaviour
{
    [Header("Top 3 UI")]
    [SerializeField] private TMP_Text top1Text;
    [SerializeField] private TMP_Text top2Text;
    [SerializeField] private TMP_Text top3Text;

    private void OnEnable()
    {
        RefreshTop3();
    }

    public void RefreshTop3()
    {
        if (PlayFabSaveManager.Instance == null)
        {
            Debug.LogWarning("Top3LeaderboardUI: PlayFabSaveManager not found.");
            return;
        }

        SetText(top1Text, "Loading...");
        SetText(top2Text, "Loading...");
        SetText(top3Text, "Loading...");

        PlayFabSaveManager.Instance.GetTopLeaderboard(entries =>
        {
            if (this == null || !isActiveAndEnabled)
                return;

            if (entries == null)
            {
                SetText(top1Text, "Unable to load");
                SetText(top2Text, "Unable to load");
                SetText(top3Text, "Unable to load");
                return;
            }

            SetEntry(top1Text, entries.Count > 0 ? entries[0] : null, 1);
            SetEntry(top2Text, entries.Count > 1 ? entries[1] : null, 2);
            SetEntry(top3Text, entries.Count > 2 ? entries[2] : null, 3);
        });
    }

    private void SetEntry(TMP_Text target, PlayerLeaderboardEntry entry, int rank)
    {
        if (target == null)
            return;

        if (entry == null)
        {
            target.text = rank + ". No player yet";
            return;
        }

        string username = !string.IsNullOrWhiteSpace(entry.DisplayName)
            ? entry.DisplayName
            : "Player";

        target.text = username + "\n" + entry.StatValue + " points";
    }

    private void SetText(TMP_Text target, string value)
    {
        if (target != null)
            target.text = value;
    }
}
