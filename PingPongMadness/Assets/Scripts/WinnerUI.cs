using Fusion;
using UnityEngine;
using TMPro;

public class WinnerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text winnerText;

        [SerializeField] private GameObject gameEndPanel;



    private void Start()
        {
            if (gameEndPanel == null)
            {
                gameEndPanel = GameObject.Find("GameEndPanel"); // Usa el nombre exacto
                if (gameEndPanel == null)
                    Debug.LogWarning("⚠️ No se encontró el GameEndPanel en escena");
            }

            gameEndPanel.SetActive(false);
        }


   private void Update()
    {
        if (GameStateManager.Instance == null)
            return;

        string winner = GameStateManager.Instance.WinnerName;

              bool gameEnd = GameStateManager.Instance.GameEnded;
               Debug.Log($"GAME ENDED STATUS from CUI: {gameEnd}");

          if (gameEnd)
        {
            // Muestra el panel una vez que el juego ha comenzado
            Debug.Log("GAME ENDED in CountdownUI!");
            gameEndPanel.gameObject.SetActive(true);
              
        }

        if (!string.IsNullOrEmpty(winner))
        {
            winnerText.text = $"Winner: {winner}";
            winnerText.gameObject.SetActive(true);
        }
        else
        {
            winnerText.gameObject.SetActive(false);
        }
    }
}