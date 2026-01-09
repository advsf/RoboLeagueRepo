using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;

public class HandleConnections : NetworkBehaviour
{
    public static HandleConnections instance;

    [Header("UI References")]
    [SerializeField] private GameObject disconnectionUI;

    [Header("Session Management")]
    [SerializeField] private SessionHolder sessionHolder;

    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private float transitionDelay = 2f;

    private bool hasHandledQuit = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

        instance = this;

        if (disconnectionUI != null)
            disconnectionUI.SetActive(false);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;

        instance = null;
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        // handle when the host leaves
        if (clientId == NetworkManager.ServerClientId || clientId == NetworkManager.Singleton.LocalClientId)
        {
            disconnectionUI.SetActive(true);

            if (clientId == NetworkManager.LocalClientId)
                NetworkManager.Singleton.Shutdown();

            Invoke(nameof(ReturnToLobby), transitionDelay);
        }
    }

    // called via a button
    public void LeaveGame()
    {

        if ((Application.isMobilePlatform && PlayerInfo.instance.amountOfGamesPlayedInThisServer < 1) || Application.isEditor)
        //    HandleInterstitialAds.instance.ShowAd();

        ReturnToLobby();
    }

    private async void ReturnToLobby()
    {
        if (hasHandledQuit)
            return;

        hasHandledQuit = true;

        if (sessionHolder.ActiveSession != null)
        {
            try
            {
                await sessionHolder.ActiveSession.LeaveAsync();
            }

            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();

        HandleTransitions.instance.PlayFadeInTransition();
        HandleTransitions.instance.PlayFadeInMusic();

        // wait for the transition to finish
        await Task.Delay((int)(transitionDuration * 1000));

        // load the scene
        SceneManager.LoadScene("Lobby");
    }

    private async void OnApplicationQuit()
    {
        if (hasHandledQuit)
            return;

        hasHandledQuit = true;

        if (sessionHolder.ActiveSession != null)
        {
            try
            {
                await sessionHolder.ActiveSession.LeaveAsync();
            }

            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }
}