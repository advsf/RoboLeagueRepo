using UnityEngine;
using UnityEngine.Advertisements;

// copy pasted from teh official Unity APK helper guide or something like that
public class InitializeAds : MonoBehaviour, IUnityAdsInitializationListener
{
    [SerializeField] string _androidGameId;
    [SerializeField] string _iOSGameId;
    [SerializeField] bool _testMode = true;
    private string _gameId;

    void Awake()
    {
        HandleInitializingAds();
    }

    public void HandleInitializingAds()
    {
#if !UNITY_ANDROID && !UNITY_IOS
    if (!testMode)
    {
    gameObject.SetActive(false);
    return;
    }
#elif UNITY_IOS
    _gameId = _iOSGameId;
#elif UNITY_ANDROID
        _gameId = _androidGameId;
#elif UNITY_EDITOR
    _gameId = _androidGameId; 
#endif

        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(_gameId, _testMode, this);
        }
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }
}
