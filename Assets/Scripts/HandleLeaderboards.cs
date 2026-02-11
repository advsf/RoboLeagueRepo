using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class HandleLeaderboards : MonoBehaviour
{
    private const string goalLeaderboardId = "Goals";
    private const string assistLeaderboardId = "Assists";
    private const string savesLeaderboardId = "Saves";

    [Header("UI References")]
    [SerializeField] private GameObject leaderboardCardPrefab;
    [SerializeField] private GameObject[] scrollViews; // 0 - goals | 2 - assists | 3 - saves
    [SerializeField] private Transform goalsContainer;
    [SerializeField] private Transform assistsContainer;
    [SerializeField] private Transform savesContainer;
    [SerializeField] private TextMeshProUGUI headerText;

    [Header("Localization References")]
    [SerializeField] private LocalizedString topGoalsLoc = new("Table1", "TOP GOAL SCORERS");
    [SerializeField] private LocalizedString topAssistsLoc = new("Table1", "TOP ASSISTERS");
    [SerializeField] private LocalizedString topSavesLoc = new("Table1", "TOP SAVERS");

    private List<LeaderboardEntry> goalsEntries;
    private List<LeaderboardEntry> assistsEntries;
    private List<LeaderboardEntry> savesEntries;

    private int currentIndex;

    [Serializable]
    public class RankMetadata
    {
        public int playerRankIndex;
    }

    private void Start()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        DisplayCurrentLeaderboard();
    }

    private async void OnEnable()
    {
        await UnityServices.InitializeAsync();
        await HandlePlayerAuthentication.instance.EnsureAuthentication();

        SubmitScore(HandlePlayerData.instance.GetGoalsCount(), HandlePlayerData.instance.GetAssistsCount(), HandlePlayerData.instance.GetSavesCount());

        // retrieve entries
        goalsEntries = await RetrieveLeaderboardScores(goalLeaderboardId);
        assistsEntries = await RetrieveLeaderboardScores(assistLeaderboardId);
        savesEntries = await RetrieveLeaderboardScores(savesLeaderboardId);

        PopulateLeaderboards();

        currentIndex = 0;
        DisplayCurrentLeaderboard();
    }

    public async void SubmitScore(int goals, int assists, int saves)
    {
        try
        {
            var rankMetadata = new RankMetadata { playerRankIndex = HandlePlayerData.instance.GetRankIndex() };

            // top goals
            var goalsPlayerEntry = await LeaderboardsService.Instance.AddPlayerScoreAsync(goalLeaderboardId, goals, new AddPlayerScoreOptions { Metadata = rankMetadata });

            // top assists
            var assistsPlayerEntry = await LeaderboardsService.Instance.AddPlayerScoreAsync(assistLeaderboardId, assists, new AddPlayerScoreOptions { Metadata = rankMetadata });

            // top saves
            var savesPlayerEntry = await LeaderboardsService.Instance.AddPlayerScoreAsync(savesLeaderboardId, saves, new AddPlayerScoreOptions { Metadata = rankMetadata });
        }

        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async Task<List<LeaderboardEntry>> RetrieveLeaderboardScores(string leaderboardId)
    {
        try
        {
            var scoreResponse = await LeaderboardsService.Instance.GetScoresAsync(
                leaderboardId,
                new GetScoresOptions { IncludeMetadata = true, Limit = 10 }
            );

            return scoreResponse.Results;
        }

        catch (Exception e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    private void PopulateLeaderboards()
    {
        DisableAllScrollViews();

        // all time goal scoring leaderboard
        foreach (var entry in goalsEntries)
        {
            GameObject card = Instantiate(leaderboardCardPrefab, goalsContainer);
            card.GetComponent<InitializeGlobalLeaderboardCard>().InitializeCard(entry);
        }

        foreach (var entry in assistsEntries)
        {
            GameObject card = Instantiate(leaderboardCardPrefab, assistsContainer);
            card.GetComponent<InitializeGlobalLeaderboardCard>().InitializeCard(entry);
        }

        foreach (var entry in savesEntries)
        {
            GameObject card = Instantiate(leaderboardCardPrefab, savesContainer);
            card.GetComponent<InitializeGlobalLeaderboardCard>().InitializeCard(entry);
        }
    }

    #region UI Functions

    public void GoLeftLeaderboardIndex()
    {
        scrollViews[currentIndex].SetActive(false);

        currentIndex--;

        if (currentIndex < 0)
            currentIndex = scrollViews.Length - 1;

        DisplayCurrentLeaderboard();
    }

    public void GoRightLeaderboardIndex()
    {
        scrollViews[currentIndex].SetActive(false);

        currentIndex++;

        if (currentIndex > scrollViews.Length - 1)
            currentIndex = 0;

        DisplayCurrentLeaderboard();
    }

    public void DisplayCurrentLeaderboard()
    {
        // localize text
        switch (currentIndex)
        {
            case 0:
                headerText.text = topGoalsLoc.GetLocalizedString();
                break;
            case 1:
                headerText.text = topAssistsLoc.GetLocalizedString();
                break;
            case 2:
                headerText.text = topSavesLoc.GetLocalizedString();
                break;
        }

        scrollViews[currentIndex].SetActive(true);
    }

    public void DisableAllScrollViews()
    {
        foreach (GameObject scrollView in scrollViews)
            scrollView.SetActive(false);
    }

    #endregion
}
