using Fusion;
using UnityEngine;
using TMPro;

public class WinnerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text winnerText;

   private void Update()
    {
        if (GameStateManager.Instance == null)
            return;

        string winner = GameStateManager.Instance.WinnerName;

        if (!string.IsNullOrEmpty(winner))
        {
            winnerText.text = $"Ganador: {winner}";
            winnerText.gameObject.SetActive(true);
        }
        else
        {
            winnerText.gameObject.SetActive(false);
        }
    }
}