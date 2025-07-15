using UnityEngine;
using TMPro;

public class CountdownUI : MonoBehaviour
{
    public static CountdownUI Instance;

    [SerializeField] private TextMeshProUGUI countdownText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowCountdown(int number)
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = number.ToString();
        }
    }

    public void HideCountdown()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
    }
}
