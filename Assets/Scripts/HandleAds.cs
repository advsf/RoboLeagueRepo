using UnityEngine;
using Unity.Services.LevelPlay;
using Unity.Services.Core;

public class HandleAds : MonoBehaviour
{
    public static HandleAds instance;

    [SerializeField] private string interstitialAdUnitId = "pmwfs9npvt27m3fc";

    private LevelPlayInterstitialAd interstitialAd;

    string appKey = "24fe938e5";

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

    public async void Start()
    {
        await UnityServices.InitializeAsync();

        LevelPlay.OnInitSuccess += LevelPlay_OnInitSuccess;
        LevelPlay.OnInitFailed += LevelPlay_OnInitFailed;

        // SDK init
        LevelPlay.Init(appKey);
    }

    private void LevelPlay_OnInitFailed(LevelPlayInitError obj)
    {
        throw new System.NotImplementedException();
    }

    private void EnableAds()
    {
        interstitialAd = new LevelPlayInterstitialAd(interstitialAdUnitId);

        // register to interstitial events
        interstitialAd.OnAdLoaded += InterstitialAd_OnAdLoaded;
        interstitialAd.OnAdLoadFailed += InterstitialAd_OnAdLoadFailed;
        interstitialAd.OnAdDisplayed += InterstitialAd_OnAdDisplayed;
        interstitialAd.OnAdDisplayFailed += InterstitialAd_OnAdDisplayFailed;
        interstitialAd.OnAdClicked += InterstitialAd_OnAdClicked;
        interstitialAd.OnAdClosed += InterstitialAd_OnAdClosed;
        interstitialAd.OnAdInfoChanged += InterstitialAd_OnAdInfoChanged;
    }

    private void LevelPlay_OnInitSuccess(LevelPlayConfiguration obj)
    {
        Debug.Log("AHHH");

        EnableAds();
        LoadInterstitialAd();
    }

    public void LoadInterstitialAd()
    {
        interstitialAd.LoadAd();

        Debug.Log("loaded");
    }

    public void ShowInterstitialAd()
    {
        if (interstitialAd.IsAdReady())
            interstitialAd.ShowAd();
        else 
            interstitialAd.LoadAd();
    }

    private void InterstitialAd_OnAdInfoChanged(LevelPlayAdInfo adInfo)
    {
        
    }

    private void InterstitialAd_OnAdClosed(LevelPlayAdInfo adInfo)
    {
        interstitialAd.LoadAd();
    }

    private void InterstitialAd_OnAdClicked(LevelPlayAdInfo adInfo)
    {
        
    }

    private void InterstitialAd_OnAdDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.LogError("interstital ad failed: " + error.ErrorMessage);

        if (HandleConnections.instance != null)
            HandleConnections.instance.ReturnToLobby();
    }

    private void InterstitialAd_OnAdDisplayed(LevelPlayAdInfo adInfo)
    {
        
    }

    private void InterstitialAd_OnAdLoadFailed(LevelPlayAdError adInfo)
    {
        
    }

    private void InterstitialAd_OnAdLoaded(LevelPlayAdInfo adInfo)
    {
        
    }
}
