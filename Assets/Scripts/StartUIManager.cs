using UnityEngine;
using Unity.Netcode;

public class StartUIManager : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject startUIObj;

    [Header("Kick Players UI References")]
    [SerializeField] private GameObject kickPlayersButtonUIObj;
    [SerializeField] private GameObject kickPlayersUIObj;
    [SerializeField] private GameObject kickPlayersButtonPrefab;
    [SerializeField] private Transform kickPlayersParent;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        kickPlayersUIObj.SetActive(false);

        kickPlayersButtonUIObj.SetActive(IsHost);

        if (!IsHost)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsHost)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
    }

    private void Singleton_OnClientConnectedCallback(ulong obj)
    {
        if (kickPlayersButtonUIObj.activeInHierarchy)
            InitializePlayerListForKickUI();
    }

    private void Singleton_OnClientDisconnectCallback(ulong obj)
    {
        if (kickPlayersButtonUIObj.activeInHierarchy)
            InitializePlayerListForKickUI();
    }

    private void OnEnable()
    {
        // only the host should be able to kick
        if (!IsHost)
            kickPlayersButtonUIObj.SetActive(false);
    }

    private void Update()
    {
        if (ServerManager.instance.isPracticeServer || ServerManager.instance.isTutorialServer)
            return;

        if (Input.GetKeyDown(KeyCode.Escape) && kickPlayersUIObj.activeInHierarchy)
        {
            // for mobile users using KMB, when they release the RMB, it triggers the back button
            // this prevents it
            if (Application.isMobilePlatform && Input.GetMouseButton(1))
                return;

            kickPlayersUIObj.SetActive(false);
        }
    }

    public void OpenSettingsMenu()
    {
        HandleSettings.instance.OpenSettingsUI();
    }

    public void CloseSettingsAndOrStartUIMenu()
    {
        HandleSettings.instance.CloseSettingsUI();
        startUIObj.SetActive(false);
    }
    
    public void ChangeSpectatorMode()
    {
        startUIObj.SetActive(false);

        PlayerInfo.instance.ChangeIntoSpectatorMode();
    }

    #region Kicking Players UI

    public void OpenKickPlayersMenu()
    {
        kickPlayersUIObj.SetActive(true);

        InitializePlayerListForKickUI();
    }

    public void CloseKickPlayersMenu()
    {
        kickPlayersUIObj.SetActive(false);
    }

    public void InitializePlayerListForKickUI()
    {
        foreach (Transform obj in kickPlayersParent)
            Destroy(obj.gameObject);

        foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
        {
            // we can't kick the host
            if (clientId == 0)
                continue;

            PlayerInfo playerInfo = NetworkManager.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerInfo>();

            GameObject button = Instantiate(kickPlayersButtonPrefab, kickPlayersParent);

            button.GetComponent<HandleKickPlayerButton>().InitializeButtonInformation(playerInfo);
        }
    }

    #endregion
}
