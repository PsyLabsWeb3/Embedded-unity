using UnityEngine;

public class WaitingForPlayersUI : MonoBehaviour
{
    private void Start()
    {
        if (BEKStudio.NetworkManager.Instance.RoomIsFull)
        {
            HideWaitingPanel();
        }
        else
        {
            BEKStudio.NetworkManager.Instance.OnRoomFull += HideWaitingPanel;
        }
    }

    private void OnDestroy()
    {
        if (BEKStudio.NetworkManager.Instance != null)
            BEKStudio.NetworkManager.Instance.OnRoomFull -= HideWaitingPanel;
    }

    private void HideWaitingPanel()
    {
        gameObject.SetActive(false);
    }
}