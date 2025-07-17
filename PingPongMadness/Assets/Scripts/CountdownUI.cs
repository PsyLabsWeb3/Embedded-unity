using UnityEngine;
using TMPro;

public class CountdownUI : MonoBehaviour
{
    public static CountdownUI Instance;

    [SerializeField] private TextMeshProUGUI countdownText;

  private void Update()
    {
        if (GameStateManager.Instance == null)
            return;

        if (GameStateManager.Instance.GameStarted)
        {
            // Oculta el texto una vez que el juego ha comenzado
            countdownText.gameObject.SetActive(false);
            return;
        }

        int countdown = GameStateManager.Instance.CountdownValue;

        if (countdown > 0)
        {
            countdownText.text = $"Game starting in {countdown}...";
            countdownText.gameObject.SetActive(true);
        }
        else
        {
            countdownText.gameObject.SetActive(false);
        }
    }
}
