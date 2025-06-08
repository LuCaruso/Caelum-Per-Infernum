using UnityEngine;

public class Revive : MonoBehaviour
{
    public GameObject endGamePanelDefeat;
    public GameObject pixPanel;
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

        currentPlayer.SetActive(true);

        PlayerHealth ph = currentPlayer.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.RevivePlayer();
        }
    }
}

