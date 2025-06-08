using UnityEngine;

public class Revive : MonoBehaviour
{
    public GameObject endGamePanelDefeat;
    public GameObject pixPanel;
    public HUDController hudController;

    private GameObject currentPlayer;

    public void ReviverJogador()
    {
        // Sempre tenta buscar o player no momento da ação
        currentPlayer = GameObject.FindGameObjectWithTag("Player");

        if (currentPlayer == null)
        {
            Debug.LogError("Não há player para reviver!");
            return;
        }

        if (endGamePanelDefeat != null) endGamePanelDefeat.SetActive(false);
        if (pixPanel != null) pixPanel.SetActive(false);

        currentPlayer.SetActive(true);

        PlayerHealth ph = currentPlayer.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.RevivePlayer();
        }

        if (hudController != null)
        {
            hudController.ResetDefeatFlag();
        }
        else
        {
            Debug.LogWarning("HUDController não está atribuído no Revive!");
        }
    }
}
