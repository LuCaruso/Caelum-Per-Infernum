using UnityEngine;
using UnityEngine.Advertisements;

public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
{
    [SerializeField] private string _androidGameId = "YOUR_ANDROID_GAME_ID";
    [SerializeField] private string _iOSGameId = "YOUR_IOS_GAME_ID";
    [SerializeField] private bool _testMode = false; // Deixe true para testar no Editor ou dispositivo
    private string _gameId;

    [SerializeField] private InterstitialAdExample interstitialAdExample;

    void Awake()
    {
        InitializeAds();
    }

    public void InitializeAds()
    {
#if UNITY_IOS
        _gameId = _iOSGameId;
#elif UNITY_ANDROID
        _gameId = _androidGameId;
#elif UNITY_EDITOR
        _gameId = _androidGameId; // Usa Android para teste no Editor
#else
        _gameId = _androidGameId; // Fallback
#endif

        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Debug.Log($"Inicializando Unity Ads com GameID: {_gameId}");
            Advertisement.Initialize(_gameId, _testMode, this);
        }
        else
        {
            Debug.Log("Unity Ads já inicializado ou não suportado.");
            if (interstitialAdExample != null)
            {
                interstitialAdExample.LoadAd();
            }
        }
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        if (interstitialAdExample != null)
        {
            interstitialAdExample.LoadAd();
        }
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"Unity Ads Initialization Failed: {error} - {message}");
    }
}
