using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using Unity.Services.Core;
using Unity.Services.Qos;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;
using UnityEngine.Localization;

public class HandleLobby : NetworkBehaviour
{
    public static HandleLobby instance;
    public ISession activeSession;

    [Header("LAN Settings")]
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private float lanTransitionDuration;
    private ushort k_Port = 7777;
    private string lanSceneGameName;

    [Header("Scene Names")]
    [SerializeField] private string gameSceneName;
    [SerializeField] private string practiceSceneName;
    [SerializeField] private string tutorialSceneName;

    [Header("Create Session References")]
    [SerializeField] private TMP_InputField sessionName;
    [SerializeField] private Toggle privateToggle;

    [Header("Session List References")]
    [SerializeField] private GameObject sessionItemPrefab;
    [SerializeField] private Transform contentParent;
    private QuerySessionsResults sessions;
    private IList<GameObject> listItems = new List<GameObject>();
    private ISessionInfo selectedSessionInfo;

    [Header("Join Session References")]
    [SerializeField] private TMP_InputField sessionJoinCode;
    [SerializeField] private Button sessionJoinIdButton;

    [Header("Other References")]
    [SerializeField] private SessionHolder sessionHolder;

    [Header("Scene Transition Setting")]
    [SerializeField] private float transitionDuration = 1f;

    private bool isHost = false;

    private bool cancelJoin = false;

    private void Awake()
    {
        // for singleton
        instance = this;
    }

    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            sessionJoinIdButton.interactable = false;

            RefreshSessionList();
        }

        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    private async Task<string> GetBestRegion()
    {
        try
        {
            var qosResults = await QosService.Instance.GetSortedQosResultsAsync("relay", new List<string>());

            if (qosResults != null && qosResults.Count > 0)
            {
                return qosResults[0].Region;
            }

            return "Unknown";
        }

        catch (Exception e)
        {
            Debug.Log(e);
        }

        return "Unknown";
    }

    #region Session Handling
    public async void CreateSessionAsHost()
    {
        try
        {
            cancelJoin = false;

            HandleLobbyUI.instance.CloseCreateSessionUI();
            HandleLobbyUI.instance.OpenJoiningServerUI();

            HandleLobbyUI.instance.SetJoiningServerExitButtonActiveness(true);

            // get our current region
            string myRegion = await GetBestRegion();

            // handle session settings
            SessionOptions options = new SessionOptions
            {
                MaxPlayers = 14,
                Name = sessionName.text.ToString(),
                IsPrivate = privateToggle.isOn,
            }.WithRelayNetwork();

            options.SessionProperties = new Dictionary<string, SessionProperty>
            {
                { "Region", new SessionProperty(myRegion, VisibilityPropertyOptions.Public)},
                { "HostRankIndex", new SessionProperty(FBPP.GetInt("RankIndex").ToString(), VisibilityPropertyOptions.Public)},
                { "HostUsername", new SessionProperty(FBPP.GetString("Username").ToString(), VisibilityPropertyOptions.Public)},

                // server customization
                { "IsServerModified", new SessionProperty(HandleServerCustomizations.instance.areServerSettingsChanged ? "T" : "F", VisibilityPropertyOptions.Public)},
                { "EachHalfDuration", new SessionProperty(HandleServerCustomizations.instance.serverEachHalfDuration.ToString(), VisibilityPropertyOptions.Public)},
                { "HalftimeDuration", new SessionProperty(HandleServerCustomizations.instance.serverHalftimeDuration.ToString(), VisibilityPropertyOptions.Public)},
                { "KickMultiplier", new SessionProperty(HandleServerCustomizations.instance.serverBallKickMultiplier.ToString(), VisibilityPropertyOptions.Public)},
                { "CurveMultiplier", new SessionProperty(HandleServerCustomizations.instance.serverBallCurveMultiplier.ToString(), VisibilityPropertyOptions.Public)},
                { "PlayerSpeedMultiplier", new SessionProperty(HandleServerCustomizations.instance.serverPlayerSpeedMultiplier.ToString(), VisibilityPropertyOptions.Public)},
                { "IsAbilityEnabled", new SessionProperty(HandleServerCustomizations.instance.serverAbilityEnabled, VisibilityPropertyOptions.Public)}, // True or False string
                { "DoAbilityCD", new SessionProperty(HandleServerCustomizations.instance.serverDoAbilityHaveCooldown, VisibilityPropertyOptions.Public)}
            };

            // create the new session
            activeSession = await MultiplayerService.Instance.CreateSessionAsync(options);

            if (await DidUserCancelJoin() || activeSession == null)
                return;

            // play transition
            HandleTransitions.instance.PlayFadeInTransition();

            // fade out music
            StartCoroutine(HandleLobbySound.instance.FadeOutMusic(0, 1.5f));

            sessionHolder.ActiveSession = activeSession;

            isHost = true;

            // we invoke loading scene
            // allowing the transition to play
            Invoke(nameof(JoinNetworkGame), transitionDuration);
        }

        catch (RequestFailedException e)
        {
            Debug.LogException(e);

            RefreshSessionList();
            CancelJoiningWithErrorMessageUI("JOIN REQUEST FAILED!");
        }

        catch (Exception e)
        {
            Debug.LogException(e);

            CancelJoiningWithErrorMessageUI("COULDN'T JOIN THE SERVER!");
        }
    }

    private async void UpdateSessions()
    {
        QuerySessionsOptions options = new QuerySessionsOptions
        {
            // shows the lobbies the with the most amount of players
            SortOptions = new List<SortOption>
            {
                new SortOption(SortOrder.Descending, SortField.AvailableSlots)
            }
        };

        sessions = await MultiplayerService.Instance.QuerySessionsAsync(options);
    }

    public void RefreshSessionList()
    {
        UpdateSessions();

        // destroy all the list items
        foreach (GameObject listItem in listItems)
            Destroy(listItem);

        if (sessions == null)
            return;

        // create the prefab instances
        foreach (var sessionInfo in sessions.Sessions)
        {
            if (sessionHolder.IsSessionBanned(sessionInfo))
                continue;

            GameObject itemPrefab = Instantiate(sessionItemPrefab, contentParent);

            if (itemPrefab.TryGetComponent<SessionItemData>(out var sessionItem))
            {
                sessionItem.SetSession(sessionInfo);

                // listens to the event where we select the item
                // useful for being able to join the session and enabling/disabling the join button
                sessionItem.OnSessionSelected.AddListener(HandleSessionSelected);
                sessionItem.OnSessionDeselected.AddListener(HandleSessionDeselected);
            }

            listItems.Add(itemPrefab);
        }
    }

    // joining session from the session list
    public async void JoinSessionById()
    {
        try
        {
            cancelJoin = false;

            HandleLobbyUI.instance.CloseSessionListUI();
            HandleLobbyUI.instance.OpenJoiningServerUI();

            HandleLobbyUI.instance.SetJoiningServerExitButtonActiveness(true);

            // join 
            activeSession = await MultiplayerService.Instance.JoinSessionByIdAsync(selectedSessionInfo.Id);

            if (await DidUserCancelJoin() || activeSession == null)
                return;

            // if we are banned from the session
            if (sessionHolder.IsSessionBanned(activeSession))
            {
                CancelJoining();
                return;
            }

            // play transition
            HandleTransitions.instance.PlayFadeInTransition();

            // fade out music
            StartCoroutine(HandleLobbySound.instance.FadeOutMusic(0, 0.5f));

            sessionHolder.ActiveSession = activeSession;

            isHost = false;

            // load the scene
            Invoke(nameof(JoinNetworkGame), transitionDuration);
        }

        catch (RequestFailedException e)
        {
            Debug.LogException(e);

            RefreshSessionList();
            CancelJoiningWithErrorMessageUI("JOIN REQUEST FAILED!");
        }

        catch (SessionException e)
        {
            Debug.LogException(e);

            RefreshSessionList();
            CancelJoiningWithErrorMessageUI("SERVER NOT FOUND!");
        }

        catch (Exception e)
        {
            Debug.LogException(e);

            CancelJoiningWithErrorMessageUI("COULDN'T JOIN THE SERVER!");
        }
    }

    public async void JoinSessionByCode()
    {
        try
        {
            cancelJoin = false;

            HandleLobbyUI.instance.CloseSessionListUI();
            HandleLobbyUI.instance.OpenJoiningServerUI();

            HandleLobbyUI.instance.SetJoiningServerExitButtonActiveness(true);

            string code = sessionJoinCode.text.ToString();

            // join 
            activeSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);

            if (await DidUserCancelJoin() || activeSession == null)
                return;

            // if we are banned from the session
            if (sessionHolder.IsSessionBanned(activeSession))
            {
                CancelJoiningWithErrorMessageUI("YOU ARE BANNED FROM THIS SERVER!");
                return;
            }

            // play transition
            HandleTransitions.instance.PlayFadeInTransition();

            // fade out music
            StartCoroutine(HandleLobbySound.instance.FadeOutMusic(0, 1.5f));

            sessionHolder.ActiveSession = activeSession;

            isHost = false;

            // load the scene
            Invoke(nameof(JoinNetworkGame), transitionDuration);
        }

        catch (RequestFailedException e)
        {
            Debug.LogException(e);

            RefreshSessionList();
            CancelJoiningWithErrorMessageUI("JOIN REQUEST FAILED!");
        }

        catch (Exception e)
        {
            Debug.LogException(e);

            CancelJoiningWithErrorMessageUI("COULDN'T JOIN THE SERVER!");
        }
    }

    private async Task<bool> DidUserCancelJoin()
    {
        if (cancelJoin)
        {
            // reset the flag
            cancelJoin = false;

            await CleanUpSessionAndNetwork();

            return true;
        }

        else
            return false;
    }

    public void CreateAndJoinPracticeServer()
    {
        try
        {
            cancelJoin = false;

            HandleLobbyUI.instance.CloseCreateSessionUI();
            HandleLobbyUI.instance.OpenJoiningServerUI();
            HandleLobbyUI.instance.CloseLanSessionUI();

            HandleLobbyUI.instance.SetJoiningServerExitButtonActiveness(true);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData("0.0.0.0", k_Port);

            HandleTransitions.instance.PlayFadeInTransition();
            StartCoroutine(HandleLobbySound.instance.FadeOutMusic(0, 0.1f));

            isHost = true;
            activeSession = null;

            lanSceneGameName = practiceSceneName;
            Invoke(nameof(CreateLan), lanTransitionDuration);
        }
        catch (Exception e)
        {
            CancelLanJoin();
        }
    }

    public void CreateAndJoinTutorialServer()
    {
        try
        {
            cancelJoin = false;

            HandleLobbyUI.instance.CloseCreateSessionUI();
            HandleLobbyUI.instance.OpenJoiningServerUI();
            HandleLobbyUI.instance.CloseLanSessionUI();

            HandleLobbyUI.instance.SetJoiningServerExitButtonActiveness(true);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData("0.0.0.0", k_Port);

            HandleTransitions.instance.PlayFadeInTransition();
            StartCoroutine(HandleLobbySound.instance.FadeOutMusic(0, 0.1f));

            isHost = true;
            activeSession = null;

            lanSceneGameName = tutorialSceneName;
            Invoke(nameof(CreateLan), lanTransitionDuration);
        }
        catch (Exception e)
        {
            CancelLanJoin();
        }
    }

    #endregion

    public void HostLanSession()
    {
        try
        {
            cancelJoin = false;

            HandleLobbyUI.instance.CloseCreateSessionUI();
            HandleLobbyUI.instance.OpenJoiningServerUI();
            HandleLobbyUI.instance.CloseLanSessionUI();

            HandleLobbyUI.instance.SetJoiningServerExitButtonActiveness(true);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData("0.0.0.0", k_Port);

            HandleTransitions.instance.PlayFadeInTransition();
            StartCoroutine(HandleLobbySound.instance.FadeOutMusic(0, 0.1f));

            isHost = true;
            activeSession = null;

            lanSceneGameName = gameSceneName;
            Invoke(nameof(CreateLan), lanTransitionDuration);
        }

        catch (Exception e)
        {
            CancelLanJoin();
        }
    }

    private void CreateLan()
    {
        if (NetworkManager.Singleton.StartHost())
            NetworkManager.Singleton.SceneManager.LoadScene(lanSceneGameName, LoadSceneMode.Single);

        else
            CancelJoining();
    }

    public void JoinLanSession()
    {
        try
        {
            cancelJoin = false;

            string ipAddress = "";

            if (ipInputField.text.Length > 0)
                ipAddress = ipInputField.text.ToString();
            else
                return;            

            HandleLobbyUI.instance.CloseSessionListUI();
            HandleLobbyUI.instance.CloseLanSessionUI();
            HandleLobbyUI.instance.OpenJoiningServerUI(); 
            HandleLobbyUI.instance.SetJoiningServerExitButtonActiveness(true);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(ipAddress, k_Port);

            HandleTransitions.instance.PlayFadeInTransition();
            StartCoroutine(HandleLobbySound.instance.FadeOutMusic(0, 0.1f));

            isHost = false;
            activeSession = null;

            Invoke(nameof(JoinLan), lanTransitionDuration);
        }

        catch (Exception e)
        {
            CancelLanJoin();
        }
    }

    private void JoinLan()
    {
        bool started = NetworkManager.Singleton.StartClient();

        // this pretty much ensures that if the user doesn't join a lan session
        // we just fallback
        if (!started)
            CancelLanJoin();
    }

    private void CancelLanJoin()
    {
        NetworkManager.Singleton.Shutdown();

        HandleLobbyUI.instance.CloseJoiningServerUI();
        HandleLobbyUI.instance.OpenLanSessionUI();

        HandleTransitions.instance.PlayFadeOutTransition(); 
        StartCoroutine(HandleLobbySound.instance.FadeInMusic(1, 1.5f)); 
    }

    private void JoinNetworkGame()
    {
        if (isHost)
        {
            NetworkManager.Singleton.StartHost();

            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }

        else
        {
            // if we successfully joined as a client, do not load the scene manually
            // the server will automatically sync to the current game scene
            NetworkManager.Singleton.StartClient();
        }

        isHost = false;
    }

    public void HandleSessionSelected(ISessionInfo sessionInfo)
    {
        selectedSessionInfo = sessionInfo;
        sessionJoinIdButton.interactable = true;
    }

    public void HandleSessionDeselected()
    {
        Invoke(nameof(DisableSessionButtonInteraction), 0.5f);
    }

    private void DisableSessionButtonInteraction() => sessionJoinIdButton.interactable = false;


    public async void CancelJoining()
    {
        cancelJoin = true;
        HandleLobbyUI.instance.CloseJoiningServerUI();

        await CleanUpSessionAndNetwork();
    }

    public async void CancelJoiningWithErrorMessageUI(string localizationKey)
    {
        cancelJoin = true;

        var localizedString = new LocalizedString("Table1", localizationKey);
        string errorMessage = localizedString.GetLocalizedString();

        HandleLobbyUI.instance.CloseJoiningServerUI(errorMessage);

        await CleanUpSessionAndNetwork();
    }

    private async Task CleanUpSessionAndNetwork()
    {
        if (activeSession != null)
        {
            try
            {
                await activeSession.LeaveAsync();
            }

            catch (Exception e)
            {
                Debug.LogError(e);
            }

            activeSession = null;
        }

        sessionHolder.ActiveSession = null;

        if (NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();
    }
}