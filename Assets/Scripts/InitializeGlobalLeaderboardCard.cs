using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Leaderboards.Models;
using TMPro;
using Newtonsoft.Json;
using System;

public class InitializeGlobalLeaderboardCard : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI rankText; // global leaderboard rank
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private TextMeshProUGUI scoreText; // goals, assists, saves, etc
    [SerializeField] private Image rankImage;
    [SerializeField] private Image rankShadowImage;

    public void InitializeCard(LeaderboardEntry entry)
    {
        rankText.text = (entry.Rank + 1).ToString();
        usernameText.text = entry.PlayerName.Split('#')[0];
        scoreText.text = entry.Score.ToString();
        
        // if this is us
        // make the text yellow
        if (entry.PlayerId == HandlePlayerAuthentication.instance._playerId)
        {
            rankText.color = Color.yellow;
            usernameText.color = Color.yellow;
            scoreText.color = Color.yellow;
        }

        try
        {
            var meta = JsonConvert.DeserializeObject<HandleLeaderboards.RankMetadata>(entry.Metadata);

            Sprite rankSprite = HandlePlayerData.instance.GetRankSprite(meta.playerRankIndex);
            rankImage.sprite = rankSprite;
            rankShadowImage.sprite = rankSprite;
        }

        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}
