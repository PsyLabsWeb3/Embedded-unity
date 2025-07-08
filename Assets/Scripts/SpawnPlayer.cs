using BEKStudio;
using Fusion;
using UnityEngine;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;
    private bool hasSpawned = false;

    private void Start()
    {
        if (NetworkManager.Instance.RoomIsFull)
        {
            StartCoroutine(WaitAndSpawn());
        }
        else
        {
            NetworkManager.Instance.OnRoomFull += () => StartCoroutine(WaitAndSpawn());
        }
    }

    private IEnumerator WaitAndSpawn()
    {
        while (NetworkManager.Instance.Runner.LocalPlayer == null ||
               NetworkManager.Instance.Runner.LocalPlayer.PlayerId == -1)
        {
            Debug.Log("Waiting for LocalPlayer to be valid...");
            yield return null;
        }

        if (hasSpawned)
        {
            Debug.Log("Already spawned. Skipping...");
            yield break;
        }

        if (NetworkManager.Instance.Runner.GetPlayerObject(NetworkManager.Instance.Runner.LocalPlayer) != null)
        {
            Debug.Log("Player already spawned (via Fusion).");
            yield break;
        }

        Debug.Log($"Spawning player for: {NetworkManager.Instance.Runner.LocalPlayer.PlayerId}");

        var obj = NetworkManager.Instance.Runner.Spawn(
            playerPrefab,
            new Vector3(0, 1, 0),
            Quaternion.identity,
            NetworkManager.Instance.Runner.LocalPlayer
        );

        if (obj.TryGetComponent<NetworkWallet>(out var walletComp))
        {
            walletComp.WalletAddress = PlayerSessionData.WalletAddress;
            walletComp.MatchId = PlayerSessionData.MatchId;
        }

        NetworkManager.Instance.Runner.SetPlayerObject(NetworkManager.Instance.Runner.LocalPlayer, obj);

        hasSpawned = true;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnRoomFull -= () => StartCoroutine(WaitAndSpawn());
    }
}