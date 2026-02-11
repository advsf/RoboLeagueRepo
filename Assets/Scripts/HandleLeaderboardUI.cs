using UnityEngine;
using Unity.Netcode;

public class HandleLeaderboardUI : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform leaderboardParent;
    [SerializeField] private GameObject sessionCodeObj;
    [SerializeField] private GameObject clickToRevealObj;
    [SerializeField] private GameObject playerLeaderboardStatPrefab; // single instance of a player stat

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
            return;

        leaderboardParent.gameObject.SetActive(false);

        if (IsHost)
            sessionCodeObj.SetActive(false);

        PlayerInputReference.instance.controls.Gameplay.Leaderboard.Enable();
    }

    private void OnEnable()
    {
        if (!IsOwner)
            return;

        PlayerInputReference.instance.controls.Gameplay.Leaderboard.Enable();
    }

    private void OnDisable()
    {
        if (!IsOwner)
            return;

        PlayerInputReference.instance.controls.Gameplay.Leaderboard.Disable();
    }

    private void Update()
    {
        if (!IsOwner || ServerManager.instance.isPracticeServer || ServerManager.instance.isTutorialServer)
            return;

        // sybau twin 
        if (PlayerInputReference.instance.controls.Gameplay.Leaderboard.IsPressed() && !leaderboardParent.gameObject.activeInHierarchy && !HandleCursorSettings.instance.IsUIOn())
        {
            Debug.Log("should be called");
            SetUpLeaderboard();
        }

        if (PlayerInputReference.instance.controls.Gameplay.Leaderboard.WasReleasedThisFrame() && leaderboardParent.gameObject.activeInHierarchy)
            DisableLeaderboard();
    }

    private void SetUpLeaderboard()
    {
        // enable the cursor and disable cam movement
        HandleCursorSettings.instance.EnableCursor(true, false);

        Debug.Log("calling");

        // handle setting up the arrangment of the leaderboard dynamically
        // meaning that the owner's team will appear above the other team
        leaderboardParent.gameObject.SetActive(true);

        if (IsHost)
            sessionCodeObj.gameObject.SetActive(true);

        // handle blue first, then red
        if (PlayerInfo.instance.currentTeam.Value.Equals("Blue"))
        {
            PlayerInfo[] players = FindObjectsByType<PlayerInfo>(FindObjectsSortMode.None);

            // blue team
            foreach (PlayerInfo player in players)
                if (player.currentTeam.Value.Equals("Blue") && player.playingObj.activeInHierarchy)
                    CreatePlayerStatsInstance(player);

            // red team
            foreach (PlayerInfo player in players)
                if (player.currentTeam.Value.Equals("Red") && player.playingObj.activeInHierarchy)
                    CreatePlayerStatsInstance(player);
        }

        // handle red first, then blue
        else
        {
            PlayerInfo[] players = FindObjectsByType<PlayerInfo>(FindObjectsSortMode.None);

            // red team
            foreach (PlayerInfo player in players)
                if (player.currentTeam.Value.Equals("Red"))
                    CreatePlayerStatsInstance(player);

            // blue team
            foreach (PlayerInfo player in players)
                if (player.currentTeam.Value.Equals("Blue"))
                    CreatePlayerStatsInstance(player);
        }
    }

    private void CreatePlayerStatsInstance(PlayerInfo player)
    {
        GameObject playerStatLeaderboard = Instantiate(playerLeaderboardStatPrefab, leaderboardParent);
        playerStatLeaderboard.GetComponent<HandlePlayerLeaderboardStats>().InitializePlayerStats(player);
    }

    public void DisableLeaderboard()
    {
        // destroy all playerStats
        // maybe in the future when a client disconnects
        // we manually remove that client's playerStat
        // instead of deleting everytime when a user disables the leaderboard
        // type shiii
        foreach (Transform playerStat in leaderboardParent)
            if (playerStat.GetComponent<HandlePlayerLeaderboardStats>())
                Destroy(playerStat.gameObject);

        leaderboardParent.gameObject.SetActive(false);

        if (IsHost)
        {
            sessionCodeObj.SetActive(false);
            clickToRevealObj.SetActive(true);
        }

        // disable the cursor and reenable camera
        HandleCursorSettings.instance.EnableCursor(false, true);
    }

    public void RevealLobbyCode() => clickToRevealObj.SetActive(false);

    #region UI Functions

    public void HandleMobileLeaderboardUI()
    {
        if (!leaderboardParent.gameObject.activeInHierarchy && !HandleCursorSettings.instance.IsUIOn())
        {
            SetUpLeaderboard();

            if (PlayerInfo.instance.playingObj.activeInHierarchy)
                HandleMobileUI.instance.EnableTouchPadObj(false);
            else
                HandleSpectatingMobileUI.instance.EnableTouchPadObj(false);
        }

        else
        {
            DisableLeaderboard();

            if (PlayerInfo.instance.playingObj.activeInHierarchy)
                HandleMobileUI.instance.EnableTouchPadObj(true);
            else
                HandleSpectatingMobileUI.instance.EnableTouchPadObj(true);
        }
    }

    #endregion
}
