using UnityEngine;

using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using EmbeddedAPI;
using System;

namespace BEKStudio
{
    public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        public static NetworkManager Instance;

        [Header("Photon Fusion")]
        public NetworkRunner runnerPrefab;
        public NetworkObject playerPrefab;  // Asegúrate de asignar esto en el Inspector

        private NetworkRunner _runnerInstance;
        public NetworkRunner Runner => _runnerInstance;

        private int _playersJoined = 0;
        private string _matchId = null;

        public event Action OnRoomFull;
        public bool RoomIsFull => _playersJoined >= 2;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private async void Start()
        {
            // ToDo: Get wallet and tx for this player
            string wallet = "player_wallet_" + System.Guid.NewGuid().ToString();
            string tx = "player_tx_" + System.Guid.NewGuid().ToString();

            _matchId = await API.RegisterPlayerAsync(wallet, tx);
            Debug.Log($"Match ID received from backend: {_matchId}");

            PlayerSessionData.WalletAddress = wallet;
            PlayerSessionData.MatchId = _matchId;

            _runnerInstance = Instantiate(runnerPrefab);
            _runnerInstance.name = "Runner";
            DontDestroyOnLoad(_runnerInstance);
            _runnerInstance.ProvideInput = true;
            _runnerInstance.AddCallbacks(GetComponent<NetworkManager>());

            await _runnerInstance.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = _matchId,
                Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
                PlayerCount = 2
            });

            _ = API.JoinMatchAsync(_matchId, wallet);
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            _playersJoined++;
            Debug.Log($"Player {player.PlayerId} joined. Total players: {_playersJoined}");

            // Wait for 2 players
            if (_playersJoined == 2)
            {
                Debug.Log("Game Ready");
                OnRoomFull?.Invoke();
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            Vector2 direction = Vector2.zero;

            if (Input.GetKey(KeyCode.UpArrow)) direction = Vector2.up;
            if (Input.GetKey(KeyCode.DownArrow)) direction = Vector2.down;
            if (Input.GetKey(KeyCode.LeftArrow)) direction = Vector2.left;
            if (Input.GetKey(KeyCode.RightArrow)) direction = Vector2.right;

            input.Set(new SnakeInputData { direction = direction });
        }

        public async void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"Player {player.PlayerId} left.");

            string winnerWallet = PlayerSessionData.WalletAddress;
            string matchId = PlayerSessionData.MatchId;

            Debug.Log($"Reporting match result. Winner: {winnerWallet}");
            await API.ReportMatchResultAsync(matchId, winnerWallet);
        }

        // Requerido por Fusion (callbacks vacíos o de logging)
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) => Debug.LogWarning($"Disconnected: {reason}");
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    }
}

