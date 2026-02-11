using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
using Steamworks;
#endif

public class HandlePlayerAuthentication : MonoBehaviour
{
    public static HandlePlayerAuthentication instance;

    private TaskCompletionSource<bool> _authCompletionSource = new TaskCompletionSource<bool>();
    public Task EnsureAuthentication() => _authCompletionSource.Task;

    public string _playerId;

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX

    private Callback<GetTicketForWebApiResponse_t> m_AuthTicketForWebApiResponseCallback;
    private string m_SessionTicket;
    private const string identity = "unityauthenticationservice";

#endif

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();

#if UNITY_ANDROID
        SignInWithGooglePlayGames();
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        if (SteamManager.Initialized)
            SignInWithSteam();
#endif
        }

        catch (Exception e)
        {
            Debug.LogError(e);

            _authCompletionSource.TrySetResult(true);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        if (AuthenticationService.Instance.IsSignedIn)
        {
            _authCompletionSource.TrySetResult(true);
            return;
        }
    }


    #region Google Play Games (Android)

#if UNITY_ANDROID
    private void SignInWithGooglePlayGames()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        PlayGamesPlatform.Instance.Authenticate((success) =>
        {
            if (success == SignInStatus.Success)
            {
                Debug.Log("Google Play Games Login Successful. Requesting Server Auth Code...");
                
                PlayGamesPlatform.Instance.RequestServerSideAccess(true, (code) =>
                {
                    _ = ExchangeGoogleCodeForUnityAuth(code);
                });
            }

            else
            {
                Debug.LogError($"Google Play Games Login Failed: {success}");
            }
        });
    }

    private async Task ExchangeGoogleCodeForUnityAuth(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(authCode);

            _authCompletionSource.TrySetResult(true);

            _playerId = AuthenticationService.Instance.PlayerId;
            HandleSettings.instance.SetPlayerIdText("Player Id: " + AuthenticationService.Instance.PlayerId);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
#endif

    #endregion

    #region Steam Login (Desktop)

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX 
    private void SignInWithSteam()
    {
        m_AuthTicketForWebApiResponseCallback = Callback<GetTicketForWebApiResponse_t>.Create(OnAuthCallback);

        SteamUser.GetAuthTicketForWebApi(identity);
    }

    private void OnAuthCallback(GetTicketForWebApiResponse_t callback)
    {
        m_SessionTicket = BitConverter.ToString(callback.m_rgubTicket).Replace("-", string.Empty);
        SignInWithSteamAsync(m_SessionTicket, "unityauthenticationservice");
    }

    async void SignInWithSteamAsync(string ticket, string identity)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithSteamAsync(ticket, identity);
            _authCompletionSource.TrySetResult(true);

            _playerId = AuthenticationService.Instance.PlayerId;
            HandleSettings.instance.SetPlayerIdText("Player Id: " + AuthenticationService.Instance.PlayerId);
        }

        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }

#endif

    #endregion
} 