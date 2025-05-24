using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [HideInInspector]
    public HUDController hudController;

    public int playerHealth = 5;
    public int maxPlayerHealth = 5;
    public bool hasSlashAbility = true;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (hudController == null)
        {
            hudController = FindObjectOfType<HUDController>();
            if (hudController == null)
            {
                Debug.LogWarning("HUDController não encontrado na cena.");
            }
        }
    }

    public void RewardPlayerAfterAd()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.RevivePlayer();
                Debug.Log("Player revivido após o anúncio.");

                // Atualiza HUD para esconder game over e restaurar tempo
                if (hudController != null)
                {
                    hudController.HideDefeatPanelAfterRevive();
                }
            }
            else
            {
                Debug.LogWarning("PlayerHealth não encontrado.");
            }
        }
        else
        {
            Debug.LogWarning("Player não encontrado.");
        }
    }

    public void ResetPlayerHealth()
    {
        playerHealth = maxPlayerHealth;
    }
}
