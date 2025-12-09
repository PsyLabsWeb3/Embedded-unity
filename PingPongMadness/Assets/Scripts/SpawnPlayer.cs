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
            Debug.Log("⏳ Waiting for LocalPlayer to be valid...");
            yield return null;
        }

        if (hasSpawned)
        {
            Debug.Log("⚠️ Already spawned. Skipping...");
            yield break;
        }

        if (NetworkManager.Instance.Runner.GetPlayerObject(NetworkManager.Instance.Runner.LocalPlayer) != null)
        {
            Debug.LogWarning("⚠️ Player already spawned. Aborting manual spawn.");
            yield break;
        }

        int playerId = NetworkManager.Instance.Runner.LocalPlayer.PlayerId;

        Vector3 spawnPos = playerId == 1 ? new Vector3(-7, 1, 0) : new Vector3(7, 1, 0);
        Debug.Log($"🚀 Spawning player {playerId} at {spawnPos}");

        // Manual spawn
        var obj = NetworkManager.Instance.Runner.Spawn(
            playerPrefab,
            spawnPos,
            Quaternion.identity,
            NetworkManager.Instance.Runner.LocalPlayer
        );

        // 🔧 Reposicionamiento forzado (por si no se respeta spawnPos)
        var cc = obj.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        obj.transform.position = spawnPos;
        if (cc != null) cc.enabled = true;

        // Asignar datos de jugador
        if (obj.TryGetComponent<NetworkWallet>(out var walletComp))
        {
            walletComp.WalletAddress = PlayerSessionData.WalletAddress;
            walletComp.MatchId = PlayerSessionData.MatchId;
        }

        NetworkManager.Instance.Runner.SetPlayerObject(NetworkManager.Instance.Runner.LocalPlayer, obj);

        hasSpawned = true;
        Debug.Log($"✅ Player {playerId} spawned and moved to {spawnPos}");
    }

    private void OnDestroy()
    {
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnRoomFull -= () => StartCoroutine(WaitAndSpawn());
    }
}
