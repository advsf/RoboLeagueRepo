using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using System;
using System.Collections;

public class ServerManager : NetworkBehaviour
{
    public static ServerManager instance;

    [Header("Goalkeepers")]
    [SerializeField] private Transform blueGoalkeeper;
    [SerializeField] private Transform redGoalkeeper;
    [SerializeField] private HandleGoalkeeperAI blueGKAI;
    [SerializeField] private HandleGoalkeeperAI redGKAI;
    [SerializeField] private Transform blueGoalkeeperIdlePoint;
    [SerializeField] private Transform redGoalkeeperIdlePoint;

    [Header("Kick-off Barrier References")]
    [SerializeField] private GameObject blueStartingKickOffBarrier;
    [SerializeField] private GameObject redStartingKickOffBarrier;
    [SerializeField] private float timeBeforeAutoDisableOnKickOffBarriers = 10;

    [Header("Practice Server Settings")]
    [SerializeField] private float spawnBallInFrontDistance; // when the user presses 1, how far will the ball be spawned
    [SerializeField] private float spawnBallInFrontForce;
    [SerializeField] private float spawnBallInFrontForceLower;
    [SerializeField] private float spawnBallToSideUpwardsForce;

    [Header("References")]
    [SerializeField] private NetworkObject networkPlayerObj;
    [SerializeField] private Transform originalBallSpawnPos;
    [SerializeField] private GameObject canvaObj;
    [SerializeField] private GameObject startUIObj;
    [SerializeField] private BallSync mainBallSync;
    private HandleBarrierBallDetection outOfBoundsScript;

    [Header("Setting")]
    [SerializeField] private float outOfBoundMaxTimer = 15f;
    [SerializeField] private int startingGameTimer = 5;
    public bool isPracticeServer = false;
    public bool isTutorialServer = false;

    [Header("Server Settings")]
    public float eachHalfDuration = 300;
    public NetworkVariable<float> ballKickMultiplier = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> ballCurveMultiplier = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> playerSpeedMultiplier = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isAbilityEnabled = new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> doAbilityCD = new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private List<SpawnButtonInfo> _spawnButtons = new List<SpawnButtonInfo>();

    // server-side dictionaries to track game state
    private Dictionary<ulong, string> clientUsernames = new Dictionary<ulong, string>();
    private Dictionary<ulong, PlayerInfo> connectedPlayers = new Dictionary<ulong, PlayerInfo>();
    private Dictionary<(PlayerDataTypes.Team, PlayerDataTypes.Position), ulong> takenSpawns = new Dictionary<(PlayerDataTypes.Team, PlayerDataTypes.Position), ulong>();

    public NetworkVariable<int> spawnedPlayerCount = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // goal counts and match timer
    public NetworkVariable<int> redTeamGoalCount = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> blueTeamGoalCount = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> matchTime = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // for out of bounds play
    public NetworkVariable<FixedString32Bytes> possessionTeam = new(string.Empty, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> outOfPlayTime = new(15, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public bool isBallOutOfBounds = false; // transitioning period between the ball being oob and actually being in play again (prevents the users from scoring goals while the ball has yet to teleport)

    public List<ulong> blueTeamPlayerIds;
    public List<ulong> redTeamPlayerIds;

    public NetworkVariable<bool> didATeamScore = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> startGameTimer = new(5, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isStartingGame = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> didStartGame = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isGameOver = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isInHalftime = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private bool isPossessionChanging = false;

    // get winning team
    public NetworkVariable<FixedString32Bytes> wonTeam = new(string.Empty, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // for late joiners
    // handling the kick off barrier
    private NetworkVariable<FixedString32Bytes> kickOffStartingTeam = new("Blue", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> didKickOffEnd = new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // only for the editor
    // CHANGE ONLY IN THE EDITOR
    public bool isTesting = false;

    private Rigidbody mainBallRb;

    public override void OnNetworkSpawn()
    {
        // simple singleton pattern
        if (instance != null && instance != this)
            Destroy(gameObject);

        else
            instance = this;

        _spawnButtons = FindObjectsByType<SpawnButtonInfo>(FindObjectsSortMode.None).ToList();

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnectedServer;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnectedServer;

            didKickOffEnd.Value = true;
        }

        // register the local client's username
        if (IsServer)
            RegisterClient(NetworkManager.LocalClientId, HandlePlayerData.instance.GetUsername());

        else
            RegisterUsernameServerRpc(HandlePlayerData.instance.GetUsername());
    }


    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnectedServer;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnectedServer;
        }
    }

    private void Start()
    {
        // disable the goalkeepers
        if (!didStartGame.Value)
        {
            blueGoalkeeper.gameObject.SetActive(false);
            redGoalkeeper.gameObject.SetActive(false);
        }

        if (isPracticeServer || isTutorialServer)
        {
            blueStartingKickOffBarrier.SetActive(false);
            redStartingKickOffBarrier.SetActive(false);
        }

        // just spawn the player immediately if tutorial
        if (isTutorialServer)
            startUIObj.SetActive(false);

        if (isPracticeServer)
            mainBallRb = BallManager.instance.mainBallSync.GetRigidbody();

        // initialize custom server settings
        InitializeCustomServerSettings();
    }

    private void Update()
    {
        // handle practice server related tools
        if (!isTutorialServer && !didStartGame.Value && !HandleCursorSettings.instance.IsUIOn())
        {
            if (isPracticeServer)
            {
                // enable or disable goalkeepers
                if (Input.GetKeyDown(KeyCode.M) && !BallManager.instance.mainBallSync.GetRigidbody().isKinematic && CanGoalkeepersBeDisabled())
                    EnableDisableAIGK();
            }

            // if the local ball isn't spawned, just spawn it asap
            if ((Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKey(KeyCode.Alpha2)) && !HandleKicking.instance.IsLocalBallSpawned() && !isPracticeServer)
                HandleKicking.instance.HandleSpawningLocalBall();

            // spawn low ball
            if (Input.GetKeyDown(KeyCode.Alpha1))
                SpawnLowBallServerRpc(PlayerMovement.instance.transform.position, PlayerMovement.instance.transform.forward);

            // spawn high ball
            if (Input.GetKeyDown(KeyCode.Alpha2))
                SpawnHighBallServerRpc(PlayerMovement.instance.transform.position, PlayerMovement.instance.transform.forward);
        }

        // if this is a practice or tutorial server, no need to do anything else
        if (isPracticeServer || isTutorialServer)
            return;

        HandleUpdatingScoreboardUI();

        HandleKickOffBarriers();

        if (!IsServer)
            return;

        // check if we can start the game
        if ((Input.GetKeyDown(KeyCode.T) && !isPracticeServer && !isTutorialServer && !HandleCursorSettings.instance.IsUIOn() && spawnedPlayerCount.Value >= 2 && !didStartGame.Value && !isGameOver.Value)
            || isTesting)
            StartCoroutine(HandleStartingGame());

        if (mainBallSync.isOutOfPlay.Value)
            outOfPlayTime.Value -= Time.deltaTime;

        // if not enough people, reset the game
        if (spawnedPlayerCount.Value < 2 && didStartGame.Value)
            StartCoroutine(HandleEndingGame());

        HandleSwappingPossessionTeamAfterTimerRunsOut();
        UpdateGameTimer();
    }

    private void InitializeCustomServerSettings()
    {
        if (!IsServer || !HandleServerCustomizations.instance.areServerSettingsChanged)
            return;

        eachHalfDuration = HandleServerCustomizations.instance.serverEachHalfDuration;
        HandleHalftime.instance.halftimeDuration.Value = HandleServerCustomizations.instance.serverHalftimeDuration;
        ballKickMultiplier.Value = HandleServerCustomizations.instance.serverBallKickMultiplier;
        ballCurveMultiplier.Value = HandleServerCustomizations.instance.serverBallCurveMultiplier;
        playerSpeedMultiplier.Value = HandleServerCustomizations.instance.serverPlayerSpeedMultiplier;
        isAbilityEnabled.Value = HandleServerCustomizations.instance.serverAbilityEnabled.Equals("True");
        doAbilityCD.Value = HandleServerCustomizations.instance.serverDoAbilityHaveCooldown.Equals("True");

        Debug.Log($"is ability enabled: {HandleServerCustomizations.instance.serverAbilityEnabled.Equals("True")} ||| do ability CD: {HandleServerCustomizations.instance.serverDoAbilityHaveCooldown.Equals("True")}");

        Debug.Log(eachHalfDuration);
    }

    public bool CanGoalkeepersBeDisabled()
    {
        return blueGoalkeeper.GetComponent<HandleGoalkeeperAI>().canGoalkeeperBeDisabled && redGoalkeeper.GetComponent<HandleGoalkeeperAI>().canGoalkeeperBeDisabled;
    }

    private void HandleUpdatingScoreboardUI()
    {
        // if we are just starting
        if (!didStartGame.Value && !isStartingGame.Value)
        {
            HandleScoreboardUI.instance.EnableScoreboardInformationUI(true);
            HandleScoreboardUI.instance.ChangeScoreboardInformationText($"Waiting for players... {spawnedPlayerCount.Value}/10");

            if (IsServer)
            {
                HandleScoreboardUI.instance.EnableStartGameHelperTextUI(true);

                if (Application.isMobilePlatform)
                    HandleScoreboardUI.instance.EnableMobileStartGameBututon(spawnedPlayerCount.Value > 1);
            }
        }

        // handle staring game timer
        if (isStartingGame.Value)
        {
            int roundedTime = (int)Mathf.Clamp(startGameTimer.Value, 0f, startingGameTimer);

            HandleScoreboardUI.instance.EnableScoreboardInformationUI(true);
            HandleScoreboardUI.instance.ChangeScoreboardInformationText($"Game starting in {roundedTime}...");

            HandleScoreboardUI.instance.EnableStartGameHelperTextUI(false);
        }

        if (isGameOver.Value)
        {
            HandleScoreboardUI.instance.EnableScoreboardInformationUI(true);
            HandleScoreboardUI.instance.ChangeScoreboardInformationText("Match over");

            if (IsServer)
                HandleScoreboardUI.instance.EnableStartGameHelperTextUI(false);
        }

        if (mainBallSync.isOffside.Value)
        {
            HandleScoreboardUI.instance.EnableScoreboardInformationUI(true);
            HandleScoreboardUI.instance.ChangeScoreboardInformationText("Offside");
        }

        // handle halftime information
        if (isInHalftime.Value)
        {
            int roundedTime = (int)Mathf.Clamp(HandleHalftime.instance.halftimeDuration.Value, 0f, 5);

            HandleScoreboardUI.instance.ChangeScoreboardInformationText($"Halftime ({roundedTime})s");
            HandleScoreboardUI.instance.EnableScoreboardInformationUI(true);
        }

        // handle scoreboard information text when the ball is out of bounds or out of play
        if (mainBallSync.isOutOfPlay.Value)
        {
            int roundedTime = (int) Mathf.Clamp(outOfPlayTime.Value, 0f, outOfBoundMaxTimer);

            if (mainBallSync.isOffside.Value)
            {
                if (possessionTeam.Value.Equals("Blue"))
                    HandleScoreboardUI.instance.ChangeScoreboardInformationText($"<color=#26B5E3>Indirect Freekick ({roundedTime}s)");
                else
                    HandleScoreboardUI.instance.ChangeScoreboardInformationText($"<color=red>Indirect Freekick ({roundedTime}s)");
            }

            else if (mainBallSync.isGoalKick.Value)
            {
                if (possessionTeam.Value.Equals("Blue"))
                    HandleScoreboardUI.instance.ChangeScoreboardInformationText($"<color=#26B5E3>Goal kick ({roundedTime}s)");
                else
                    HandleScoreboardUI.instance.ChangeScoreboardInformationText($"<color=red>Goal kick ({roundedTime}s)");
            }

            else if (mainBallSync.isThrowIn.Value)
            {
                // #26B5E3 - light blue
                if (possessionTeam.Value.Equals("Blue"))
                    HandleScoreboardUI.instance.ChangeScoreboardInformationText($"<color=#26B5E3>Throw in ({roundedTime}s)");
                else
                    HandleScoreboardUI.instance.ChangeScoreboardInformationText($"<color=red>Throw in ({roundedTime}s)");
            }

            else if (mainBallSync.isCornerKick.Value)
            {
                if (possessionTeam.Value.Equals("Blue"))
                    HandleScoreboardUI.instance.ChangeScoreboardInformationText($"<color=#26B5E3>Corner kick ({roundedTime}s)");
                else
                    HandleScoreboardUI.instance.ChangeScoreboardInformationText($"<color=red>Corner kick ({roundedTime}s)");
            }

            HandleScoreboardUI.instance.EnableScoreboardInformationUI(true);
        }

        // disable
        if (didStartGame.Value && !mainBallSync.isOutOfPlay.Value && !mainBallSync.isOffside.Value && !isInHalftime.Value)
            HandleScoreboardUI.instance.EnableScoreboardInformationUI(false);
    }

    private void HandleSwappingPossessionTeamAfterTimerRunsOut()
    {
        // change the possession team
        if (outOfPlayTime.Value <= 0 && !isPossessionChanging)
        {
            isPossessionChanging = true;

            // just swap possession
            if (mainBallSync.isOffside.Value)
            {
                // throw in goes to the other team (we also reset the timer here)
                HandleOffsides.instance.StartIndirectionKickServerRpc(mainBallSync.GetRigidbody().position, possessionTeam.Value.Equals("Blue") ? "Red" : "Blue", true);
            }

            // goalkick -> corner kick
            else if (mainBallSync.isGoalKick.Value)
            {
                mainBallSync.isGoalKick.Value = false;
                mainBallSync.isCornerKick.Value = true;

                StartCoroutine(outOfBoundsScript.HandleGoalLineOutOfBoundsPlay(false, outOfBoundsScript.rightCorner.position, possessionTeam.Value.Equals("Blue") ? "Red" : "Blue"));
                DisableGoalkickBoundaries();
            }

            // corner kick -> goalkick
            else if (mainBallSync.isCornerKick.Value)
                StartCoroutine(outOfBoundsScript.HandleGoalLineOutOfBoundsPlay(true, possessionTeam.Value.Equals("Blue") ? outOfBoundsScript.redGoalKickSpawnPoint.position : outOfBoundsScript.blueGoalKickSpawnPoint.position));

            // throw in
            else
            {
                // throw in goes to the other team (we also reset the timer here)
                StartCoroutine(outOfBoundsScript.HandleSideOutOfBoundsPlay(outOfBoundsScript.ballHitPos, possessionTeam.Value.Equals("Blue") ? "Red" : "Blue"));

                // drop the ball (since there might be users who were currently holding)
                Invoke(nameof(DropBallIfThrowInPickedUpClientRpc), outOfBoundsScript.restartPlayDelay);
            }

            Invoke(nameof(EndChangePossessionState), (mainBallSync.isOffside.Value ? HandleOffsides.instance.restartPlayDelay : outOfBoundsScript.restartPlayDelay) + 0.15f);
        }
    }

    public void DisableGoalkickBoundaries()
    {
        // disable blue net boundary
        outOfBoundsScript.EnableGoalkickBoundary(true, false);

        // disable red net boundary
        outOfBoundsScript.EnableGoalkickBoundary(false, false);
    }

    private void UpdateGameTimer()
    {
        if (didStartGame.Value)
        {
            // if it's not in halftime
            // up the clock
            if (matchTime.Value <= eachHalfDuration && !isInHalftime.Value)
                matchTime.Value += Time.deltaTime;

            // if we need to start halftime
            if (matchTime.Value > eachHalfDuration && !HandleHalftime.instance.isAfterHalftime)
                StartCoroutine(HandleHalftime.instance.TurnOnHalftime());

            // end game AFTER the halftime is called as well
            if (matchTime.Value >= eachHalfDuration && !isGameOver.Value && HandleHalftime.instance.isAfterHalftime)
                StartCoroutine(HandleEndingGame());
        }
    }

    #region Practice Ball Physics and AI GK

    [ServerRpc(RequireOwnership = false)]
    public void SpawnLowBallServerRpc(Vector3 playerPos, Vector3 playerForwardDir, ServerRpcParams serverRpcParams = default)
    {
        if (!HandleKicking.instance.IsLocalBallSpawned() && !isPracticeServer)
            HandleKicking.instance.HandleSpawningLocalBall();

        Rigidbody rb = isPracticeServer ? mainBallRb : BallManager.instance.GetLocalSpawnedBall(serverRpcParams.Receive.SenderClientId).GetRigidbody();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = playerPos + playerForwardDir * spawnBallInFrontDistance;

        // add force
        rb.AddForce(-playerForwardDir * spawnBallInFrontForce, ForceMode.Impulse);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnHighBallServerRpc(Vector3 playerPos, Vector3 playerForwardDir, ServerRpcParams serverRpcParams = default)
    {
        if (!HandleKicking.instance.IsLocalBallSpawned() && !isPracticeServer)
            HandleKicking.instance.HandleSpawningLocalBall();

        Rigidbody rb = isPracticeServer ? mainBallRb : BallManager.instance.GetLocalSpawnedBall(serverRpcParams.Receive.SenderClientId).GetRigidbody();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = playerPos + playerForwardDir * spawnBallInFrontDistance;

        // add force
        rb.AddForce(-playerForwardDir * spawnBallInFrontForceLower + Vector3.up * spawnBallToSideUpwardsForce, ForceMode.Impulse);
    }

    public void EnableDisableAIGK()
    {
        blueGoalkeeper.gameObject.SetActive(!blueGoalkeeper.gameObject.activeInHierarchy);
        redGoalkeeper.gameObject.SetActive(!redGoalkeeper.gameObject.activeInHierarchy);
    }

    #endregion

    #region End Game Logic
    private void DetermineWinningTeam()
    {
        // blue team winning
        if (blueTeamGoalCount.Value > redTeamGoalCount.Value)
            wonTeam.Value = "Blue";

        // red team winning
        else if (redTeamGoalCount.Value > blueTeamGoalCount.Value)
            wonTeam.Value = "Red";

        // tie
        else
            wonTeam.Value = "Tie";
    }

    private IEnumerator HandleEndingGame()
    {
        DetermineWinningTeam();

        // mark the match as over
        isGameOver.Value = true;
        didStartGame.Value = false;

        HandleEndingGameClientRpc();

        if (!HandleServerCustomizations.instance.areServerSettingsChanged)
            ShowEndOfGameChatMessageClientRpc(wonTeam.Value.ToString(), wonTeam.Value.Equals("Tie"));

        mainBallSync.EndBallOutOfPlayServerRpc();

        // wait 8 seconds before resetting
        yield return new WaitForSeconds(8);

        // reset the stadium
        HandleHalftime.instance.isAfterHalftime = false;
        HandleHalftime.instance.ChangeIntoStadiumHalf1ClientRpc();

        // clear player game stats values
        ResetPlayersDataClientRpc();

        // allow players to spawn their own local balls
        BallManager.instance.ClearAllClientBalls();

        // reset all match state values
        isGameOver.Value = false;
        isStartingGame.Value = false;
        matchTime.Value = 0;
        redTeamGoalCount.Value = 0;
        blueTeamGoalCount.Value = 0;
        didATeamScore.Value = false;
        didKickOffEnd.Value = true;
        possessionTeam.Value = string.Empty;
        outOfPlayTime.Value = outOfBoundMaxTimer;

        // reset the ball to the center
        ResetBallToTheCenter();

        // re-enable idle goalkeepers
        blueGoalkeeper.position = blueGoalkeeperIdlePoint.position;
        redGoalkeeper.position = redGoalkeeperIdlePoint.position;
        DisableGoalkeepersClientRpc();

        // disable all kick-off barriers
        blueStartingKickOffBarrier.SetActive(false);
        redStartingKickOffBarrier.SetActive(false);

        // reset all players to their initial spawn points and reset UI
        ResetGameClientRpc("Blue", false);
    }

    [ClientRpc]
    private void DisableGoalkeepersClientRpc()
    {
        blueGoalkeeper.gameObject.SetActive(false);
        redGoalkeeper.gameObject.SetActive(false);
    }

    [ClientRpc]
    private void HandleEndingGameClientRpc()
    {
        SoundManager.instance.PlayWhistleSound();

        // be able to spawn the local ball again
        HandleKicking.instance.DeleteLocalSpawnedBall();

        PlayerInfo.instance.amountOfGamesPlayedInThisServer++;

        // play ads for that dolla
        /*
        if (Application.isMobilePlatform)
            HandleInterstitialAds.instance.ShowAd();
        */
    }

    [ClientRpc]
    private void ShowEndOfGameChatMessageClientRpc(string wonTeam, bool isTie)
    {
        int goals = PlayerInfo.instance.goals.Value;
        int assists = PlayerInfo.instance.assists.Value;
        int saves = PlayerInfo.instance.saves.Value;

        int xp = 0;

        bool didWin = PlayerInfo.instance.currentTeam.Value.ToString().Equals(wonTeam);
        xp = DetermineEXP(didWin, isTie, goals, assists, saves);

        HandlePlayerData.instance.IncreaseRankEXP(xp);

        // check if player played enough (7 minutes)
        if (PlayerInfo.instance.gamePlayDuration > 480)
        {
            string message;

            // if we won
            if (didWin)
                message = $"You earned {xp} xp for {goals} goals, {assists} assists, and {saves} saves!";

            // if a tie
            else if (isTie)
                message = $"You gained zero xp for a tie game!";

            // if we lost
            else
                message = $"You lost {-xp} xp for losing a match!";


            // didnt know you can give hints as to what the parameter values are (very useful)
            HandleChatbox.instance.HandleFormattingTexts(
                    isChattingAll: true,
                    rankIndex: -1,
                    teamColor: "",
                    username: "",
                    position: "",
                    text: message,
                    isServer: true);
        }

        // player didn't play enough
        else
        {
            HandleChatbox.instance.HandleFormattingTexts(
                    isChattingAll: true,
                    rankIndex: -1,
                    teamColor: "",
                    username: "",
                    position: "",
                    text: "You did not earn any xp for not playing enough!",
                    isServer: true);
        }
    }

    private int DetermineEXP(bool isWon, bool isTied, int goals, int assists, int saves)
    {
        int xp = 0;

        if (isWon)
        {
            // default to 10 for winning
            xp += 10;

            // 5 xp for every goal
            xp += goals * 5;

            // 3 xp for every assists
            xp += assists * 3;

            // 1 xp for every saves
            xp += saves * 1;
        }

        else if (isTied)
            xp = 0;

        else
            xp = -15;

        return xp;
    }

    [ClientRpc]
    private void ResetPlayersDataClientRpc()
    {
        // reset all player stats
        PlayerInfo.instance.saves.Value = 0;

        // reset the game play duration
        PlayerInfo.instance.gamePlayDuration = 0;

        ResetPlayersDataServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetPlayersDataServerRpc(ServerRpcParams serverRpcParams = default)
    {
        PlayerInfo playerInfo = NetworkManager.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject.GetComponent<PlayerInfo>();

        // for some reason i made these two variables only changable by server and im not looking to change it to owner owned
        playerInfo.goals.Value = 0;
        playerInfo.assists.Value = 0;
    }

    #endregion
    public void RegisterClient(ulong clientId, string username)
    {
        if (!IsHost) 
            return;

        clientUsernames[clientId] = username;
    }

    public void RestartRound(string scoredTeam, bool countGoal = true)
    {
        if (!IsServer)
            return;

        mainBallSync.lastKickedClientId.Value = -1;
        mainBallSync.secondLastKickedClientId.Value = -1;

        mainBallSync.scorerUsername.Value = string.Empty;
        mainBallSync.assisterUsername.Value = string.Empty;

        // reset the ball's position
        ResetBallToTheCenter();

        // now set the didATeamScore flag to false to prevent multiple goals from being scored due to interpolation and weird detection collisions by unity
        didATeamScore.Value = false;

        // reset the goalkeeper's position
        blueGoalkeeper.position = blueGoalkeeperIdlePoint.position;
        redGoalkeeper.position = redGoalkeeperIdlePoint.position;

        // handle the kick off barriers (handled in the update function)
        kickOffStartingTeam.Value = scoredTeam.Equals("Blue") ? "Red" : "Blue";
        didKickOffEnd.Value = false;
        Invoke(nameof(AutomaticallyDisableKickOffBarrier), timeBeforeAutoDisableOnKickOffBarriers);

        // for now, just make the player's move back to their spawn points
        ResetGameClientRpc(scoredTeam, countGoal);
    }

    public void ResetBallToTheCenter() => mainBallSync.ResetBallServerRpc(originalBallSpawnPos.position);

    [ServerRpc(RequireOwnership = false)]
    private void RegisterUsernameServerRpc(string newUsername, ServerRpcParams rpcParams = default) => RegisterClient(rpcParams.Receive.SenderClientId, newUsername);

    #region Kick-off Barriers

    public void DisableKickOffBarriers()
    {
        // LOCALLY disable the barriers first
        blueStartingKickOffBarrier.SetActive(false);
        redStartingKickOffBarrier.SetActive(false);

        if (IsServer)
            didKickOffEnd.Value = true;
        else
            DisableKickOffBarriersServerRpc();
    }


    private void HandleKickOffBarriers()
    {
        if (!didKickOffEnd.Value)
        {
            // enable the kick off barriers
            if (kickOffStartingTeam.Value.Equals("Blue"))
            {
                blueStartingKickOffBarrier.SetActive(true);
                redStartingKickOffBarrier.SetActive(false);
            }

            else
            {
                redStartingKickOffBarrier.SetActive(true);
                blueStartingKickOffBarrier.SetActive(false);
            }
        }

        // disable them when it's not kick off
        else
        {
            redStartingKickOffBarrier.SetActive(false);
            blueStartingKickOffBarrier.SetActive(false);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void DisableKickOffBarriersServerRpc() => didKickOffEnd.Value = true;

    private void AutomaticallyDisableKickOffBarrier()
    {
        didKickOffEnd.Value = true;
    }

    #endregion

    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnServerRpc(PlayerDataTypes.Team team, PlayerDataTypes.Position position, PlayerDataTypes.Team concededTeam = PlayerDataTypes.Team.Blue, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        // Prevent a player who has already spawned from taking another spot.
        if (takenSpawns.ContainsValue(clientId))
            return;

        HandlePlayerSpawning(clientId, team, position, concededTeam);
    }

    private void HandlePlayerSpawning(ulong clientId, PlayerDataTypes.Team team, PlayerDataTypes.Position position, PlayerDataTypes.Team concededTeam = PlayerDataTypes.Team.Blue)
    {
        var spawnKey = (team, position);

        if (takenSpawns.ContainsKey(spawnKey))
            return;

        spawnedPlayerCount.Value++;

        string username = clientUsernames.ContainsKey(clientId) ? clientUsernames[clientId] : "Player " + clientId;
        takenSpawns[spawnKey] = clientId;

        if (team == PlayerDataTypes.Team.Blue)
            blueTeamPlayerIds.Add(clientId);
        else
            redTeamPlayerIds.Add(clientId);

        // set the player network obj's info
        NetworkManager.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerInfo>().SetTeamAndPositionAndUsername(team, position, clientUsernames[clientId]);

        // disable the goalkeeper if we are becoming a goalkeeper
        if (position == PlayerDataTypes.Position.GK)
            EnableAIGoalkeeperClientRpc(team, false);

        // spawn player
        SpawnPlayerClientRpc(team == concededTeam, team, position, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        });

        UpdateSpawnButtonUIClientRpc(team, position, username);

        HandleChatbox.instance.SendTextClientRpc(true, -1, "", "", "", $"{username} joined {team} team as a {position}", true);
    }

    [ClientRpc]
    private void UpdateSpawnButtonUIClientRpc(PlayerDataTypes.Team team, PlayerDataTypes.Position position, string username, ClientRpcParams clientRpcParams = default)
    {
        foreach (SpawnButtonInfo button in _spawnButtons)
        {
            if (button.buttonTeam == team && button.buttonPosition == position)
            {
                button.SetAsTaken(username);
                break;
            }
        }
    }

    private void HandleClientConnectedServer(ulong clientId)
    {
        // define RPC parameters to target only the newly connected client
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        };

        // sync the new client with the current state of all taken spawn positions
        foreach (var entry in takenSpawns)
        {
            var spawnInfo = entry.Key;
            var ownerClientId = entry.Value;
            string username = clientUsernames.ContainsKey(ownerClientId) ? clientUsernames[ownerClientId] : $"Player {ownerClientId}";
            UpdateSpawnButtonUIClientRpc(spawnInfo.Item1, spawnInfo.Item2, username, clientRpcParams);
        }

        // update the goalkeeper status to late joiners
        if (didStartGame.Value)
        {
            bool isBlueAIActive = !takenSpawns.ContainsKey((PlayerDataTypes.Team.Blue, PlayerDataTypes.Position.GK));
            EnableAIGoalkeeperClientRpc(PlayerDataTypes.Team.Blue, isBlueAIActive, clientRpcParams);

            bool isRedAIActive = !takenSpawns.ContainsKey((PlayerDataTypes.Team.Red, PlayerDataTypes.Position.GK));
            EnableAIGoalkeeperClientRpc(PlayerDataTypes.Team.Red, isRedAIActive, clientRpcParams);
        }

        // store the playerinfo now
        // when the clienet leaves, we will need to access this dictionary later
        var playerInfo = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerInfo>();
        connectedPlayers[clientId] = playerInfo;

        // send message to all the clients
        HandleChatbox.instance.SendTextClientRpc(true, -1, "", "", "", $"{clientUsernames[clientId]} connected", true);
    }

    private void HandleClientDisconnectedServer(ulong clientId)
    {
        // send message that the client disconnected
        if (HandleChatbox.instance != null)
            HandleChatbox.instance.SendTextClientRpc(true, -1, "", "", "", $"{clientUsernames[clientId]} disconnected", true);

        if (connectedPlayers.TryGetValue(clientId, out PlayerInfo playerInfo))
        {
            PlayerDataTypes.Team previousTeam = Enum.Parse<PlayerDataTypes.Team>(playerInfo.currentTeam.Value.ToString()); // this isn't safe but idgaf
            PlayerDataTypes.Position previousPosition = Enum.Parse<PlayerDataTypes.Position>(playerInfo.currentPosition.Value.ToString());

            // handle reenabling the goalkeeper
            if (previousPosition == PlayerDataTypes.Position.GK && didStartGame.Value)
                EnableAIGoalkeeperClientRpc(previousTeam, true);

            // remove the id
            if (previousTeam == PlayerDataTypes.Team.Blue)
                blueTeamPlayerIds.Remove(clientId);
            else
                redTeamPlayerIds.Remove(clientId);

            connectedPlayers.Remove(clientId);
        }

        // find if the disconnected client had a spawn spot and free it up
        if (takenSpawns.ContainsValue(clientId))
        {
            var spotToRemove = takenSpawns.First(kvp => kvp.Value == clientId).Key;
            takenSpawns.Remove(spotToRemove);

            // tell all clients to free up this button
            FreeUpSpawnButtonClientRpc(spotToRemove.Item1, spotToRemove.Item2);

            spawnedPlayerCount.Value--;
        }

        // remove the disconnected client's username from our list
        if (clientUsernames.ContainsKey(clientId))
            clientUsernames.Remove(clientId);
    }

    public void StartGameFromUI()
    {
        if (isStartingGame.Value)
            return;

        StartCoroutine(HandleStartingGame());
    }

    private IEnumerator HandleStartingGame()
    {
        if (didStartGame.Value || !IsServer)
            yield break;

        startGameTimer.Value = startingGameTimer;
        isStartingGame.Value = true;

        while (startGameTimer.Value > 0)
        {
            startGameTimer.Value -= Time.deltaTime;
            yield return null;
        }

        // delete all the non main balls
        BallManager.instance.DeleteNonMainBalls();

        isStartingGame.Value = false;
        didStartGame.Value = true;

        // re enable goalkeeprs if their position isn't already taken
        if (!takenSpawns.ContainsKey((PlayerDataTypes.Team.Blue, PlayerDataTypes.Position.GK)))
            EnableAIGoalkeeperClientRpc(PlayerDataTypes.Team.Blue, true);

        if (!takenSpawns.ContainsKey((PlayerDataTypes.Team.Red, PlayerDataTypes.Position.GK)))
            EnableAIGoalkeeperClientRpc(PlayerDataTypes.Team.Red, true);

        // let the blue team start first
        RestartRound("Red", false);
    }

    [ClientRpc]
    public void EnableAIGoalkeeperClientRpc(PlayerDataTypes.Team team, bool condition, ClientRpcParams clientRpcParams = default)
    {
        if (team == PlayerDataTypes.Team.Blue)
            blueGoalkeeper.gameObject.SetActive(condition);
        else
            redGoalkeeper.gameObject.SetActive(condition);
    }

    public (PlayerDataTypes.Team, PlayerDataTypes.Position)? FindTakenSpawn(ulong clientId)
    {
        if (takenSpawns.ContainsValue(clientId))
        {
            return takenSpawns.First(kvp => kvp.Value == clientId).Key;
        }

        return null;
    }

    public void RemovePlayerFromSpot(PlayerDataTypes.Team team, PlayerDataTypes.Position position, ulong clientId)
    {
        spawnedPlayerCount.Value--;

        var spawnKey = (team, position);

        // remove the ids
        if (team == PlayerDataTypes.Team.Blue)
            blueTeamPlayerIds.Remove(clientId);
        else
            redTeamPlayerIds.Remove(clientId);

        if (takenSpawns.ContainsKey(spawnKey))
        {
            takenSpawns.Remove(spawnKey);
        }
    }

    [ClientRpc]
    public void FreeUpSpawnButtonClientRpc(PlayerDataTypes.Team team, PlayerDataTypes.Position position)
    {
        foreach (SpawnButtonInfo button in _spawnButtons)
        {
            if (button.buttonTeam == team && button.buttonPosition == position)
            {
                button.ResetButton();
                break;
            }
        }
    }

    [ClientRpc]
    private void ResetGameClientRpc(string scoredTeam, bool countGoal)
    {
        PlayerInfo.instance.ResetPlayerPosition(!PlayerInfo.instance.currentTeam.Value.Equals(scoredTeam));
    }

    [ClientRpc]
    private void SpawnPlayerClientRpc(bool didConcede, PlayerDataTypes.Team team, PlayerDataTypes.Position position, ClientRpcParams clientRpcParams = default) => PlayerInfo.instance.StartAsPlayingPlayer(didConcede, team, position);

    public void EnableSpawnSelectionCanvaObj(bool condition)
    {
        // currently, there is the StartUI which has the player selection UI
        // so we need to enable both
        canvaObj.transform.GetChild(0).gameObject.SetActive(condition);
    }

    public bool IsSpawnSelectionCanvaObjActive() => canvaObj.transform.GetChild(0).gameObject.activeInHierarchy;

    [ServerRpc(RequireOwnership = false)]
    public void RequestChangeOfPositionServerRpc(PlayerDataTypes.Team team, PlayerDataTypes.Position position, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        PlayerInfo playerInfo = NetworkManager.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerInfo>();

        var previousTeam = Enum.Parse<PlayerDataTypes.Team>(playerInfo.currentTeam.Value.ToString());
        var previousPosition = Enum.Parse<PlayerDataTypes.Position>(playerInfo.currentPosition.Value.ToString());

        var spotToRemove = FindTakenSpawn(clientId);
        if (spotToRemove.HasValue)
        {
            // reenable the goalkeeper
            if (previousPosition == PlayerDataTypes.Position.GK && didStartGame.Value)
                EnableAIGoalkeeperClientRpc(previousTeam, true);

            // updates the UI for everyone else
            RemovePlayerFromSpot(spotToRemove.Value.Item1, spotToRemove.Value.Item2, clientId);
            FreeUpSpawnButtonClientRpc(spotToRemove.Value.Item1, spotToRemove.Value.Item2);

            HandlePlayerSpawning(clientId, team, position, PlayerDataTypes.Team.Blue);
        }

        HandlePlayerSpawning(rpcParams.Receive.SenderClientId, team, position);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSpectateServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        var spotToRemove = FindTakenSpawn(clientId);
        if (spotToRemove.HasValue)
        {
            var previousTeam = spotToRemove.Value.Item1;
            var previousPosition = spotToRemove.Value.Item2;

            if (previousPosition == PlayerDataTypes.Position.GK && didStartGame.Value)
                EnableAIGoalkeeperClientRpc(previousTeam, true);

            RemovePlayerFromSpot(previousTeam, previousPosition, clientId);
            FreeUpSpawnButtonClientRpc(previousTeam, previousPosition);
        }
    }

    #region Out Of Play Validation

    [ServerRpc(RequireOwnership = false)]
    public void HandleWhichTeamHasPossessionInIndirectFreeKickServerRpc(string possessionTeam)
    {
        mainBallSync.isOutOfPlay.Value = true;
        mainBallSync.isIndirectFreekick.Value = true;

        this.possessionTeam.Value = new FixedString32Bytes(possessionTeam);
    }

    [ServerRpc(RequireOwnership = false)]
    public void HandleWhichTeamHasPossessionInOutOfBoundsInServerRpc(bool isThrowIn, string possessionTeam)
    {
        mainBallSync.isOutOfPlay.Value = true;
        mainBallSync.isThrowIn.Value = isThrowIn;

        this.possessionTeam.Value = new FixedString32Bytes(possessionTeam);
    }

    [ServerRpc(RequireOwnership = false)]
    public void HandleGoalkickPossessionsServerRpc(string possessionTeam)
    {
        mainBallSync.isGoalKick.Value = true;

        this.possessionTeam.Value = new FixedString32Bytes(possessionTeam);
    }

    [ClientRpc]
    private void DropBallIfThrowInPickedUpClientRpc()
    {
        NetworkManager.LocalClient.PlayerObject.GetComponentInChildren<HandleThrowIn>().DropBall();
    }

    public void DetermineWhichDetectorWasUsed(HandleBarrierBallDetection script)
    {
        outOfBoundsScript = script;
    }

    public void SetAllDetectorsToInactive()
    {
        outOfBoundsScript = null;
    }

    public void ResetOutOfBoundsTimer()
    {
        outOfPlayTime.Value = outOfBoundMaxTimer;
    }

    private void EndChangePossessionState() => isPossessionChanging = false;

    #endregion  
}