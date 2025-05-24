using UnityEngine;
using UnityEngine.Advertisements;

public class InterstitialAdExample : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] private string _androidAdUnitId = "Interstitial_Android";
    [SerializeField] private string _iOSAdUnitId = "Interstitial_iOS";
    private string _adUnitId;

    private bool isAdLoaded = false;

    public static event System.Action OnAdFinishedSuccessfully;

    void Awake()
    {
        _adUnitId = (Application.platform == RuntimePlatform.IPhonePlayer)
            ? _iOSAdUnitId
            : _androidAdUnitId;

        LoadAd(); // Carrega o anúncio ao iniciar
    }

    public void LoadAd()
    {
        if (!isAdLoaded)
        {
            Debug.Log("Carregando anúncio...");
            Advertisement.Load(_adUnitId, this);
        }
        else
        {
            Debug.Log("Anúncio já está carregado.");
        }
    }

    public void ShowAd()
    {
        Debug.Log($"ShowAd chamado. isAdLoaded = {isAdLoaded}");
        if (isAdLoaded)
        {
            Advertisement.Show(_adUnitId, this);
        }
        else
        {
            Debug.LogWarning("Anúncio ainda não carregado.");
        }
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        if (adUnitId == _adUnitId)
        {
            isAdLoaded = true;
            Debug.Log("Anúncio carregado.");
        }
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        if (adUnitId == _adUnitId)
        {
            Debug.LogError($"Erro ao carregar o anúncio {adUnitId}: {error} - {message}");
            isAdLoaded = false;
        }
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        if (adUnitId == _adUnitId)
        {
            Debug.LogError($"Erro ao exibir o anúncio {adUnitId}: {error} - {message}");
            isAdLoaded = false;
        }
    }

    public void OnUnityAdsShowStart(string adUnitId) { }

    public void OnUnityAdsShowClick(string adUnitId) { }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId == _adUnitId)
        {
            if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
            {
                Debug.Log("Anúncio finalizado com sucesso.");
                GameManager.Instance.RewardPlayerAfterAd();
                OnAdFinishedSuccessfully?.Invoke();
            }
            else if (showCompletionState == UnityAdsShowCompletionState.SKIPPED)
            {
                Debug.Log("Anúncio foi pulado.");
            }
            else if (showCompletionState == UnityAdsShowCompletionState.UNKNOWN)
            {
                Debug.LogWarning("Estado de anúncio desconhecido.");
            }

            // Após o anúncio terminar, carregue outro
            isAdLoaded = false;
            LoadAd();
        }
    }
}
