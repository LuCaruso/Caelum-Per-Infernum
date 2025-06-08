using UnityEngine;

public class Revive : MonoBehaviour
{
    public GameObject endGamePanelDefeat;
    public GameObject pixPanel;
    public HUDController hudController;

    private GameObject currentPlayer;

    void Start()
    {
        currentPlayer = GameObject.FindGameObjectWithTag("Player");
        if (currentPlayer == null)
        {
            Debug.LogWarning("Player não encontrado na cena!");
        }
    }

    public void ReviverJogador()
    {
        if (currentPlayer == null)
        {
            Debug.LogError("Não há player para reviver!");
            return;
        }

        endGamePanelDefeat.SetActive(false);
        pixPanel.SetActive(false);

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
