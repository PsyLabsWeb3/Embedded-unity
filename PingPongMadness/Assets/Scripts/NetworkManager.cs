using UnityEngine;
using System.Collections.Generic;
using System.Collections;
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
        private List<PlayerMovement> registeredPlayers = new List<PlayerMovement>();
        private bool gameStarted = false;

        [Header("Photon Fusion")]
        public NetworkRunner runnerPrefab;
        public NetworkObject playerPrefab;

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
            string address = WalletManager.WalletAddress;
            string wallet = !string.IsNullOrEmpty(address) ? address : "player_wallet_" + System.Guid.NewGuid();

            Debug.Log(string.IsNullOrEmpty(address) ? $"❌ WalletAddress no disponible, generado aleatorio: {wallet}" : $"✅ Usando wallet del jugador: {wallet}");

            string tx = "player_tx_" + System.Guid.NewGuid();
            _matchId = await API.RegisterPlayerAsync(wallet, tx);
            Debug.Log($"Match ID received from backend: {_matchId}");

            PlayerSessionData.WalletAddress = wallet;
            PlayerSessionData.MatchId = _matchId;

            _runnerInstance = Instantiate(runnerPrefab);
            _runnerInstance.name = "Runner";
            DontDestroyOnLoad(_runnerInstance);
            _runnerInstance.ProvideInput = true;
            _runnerInstance.AddCallbacks(this);

            await _runnerInstance.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = _matchId,
                Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
                SceneManager = null,
                PlayerCount = 2
            });

            _ = API.JoinMatchAsync(_matchId, wallet);
        }

        // public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        // {
        //     _playersJoined++;
        //     Debug.Log($"Player {player.PlayerId} joined. Total players: {_playersJoined}");

        //     if (_playersJoined == 2)
        //     {
        //         Debug.Log("Game Ready");
        //         OnRoomFull?.Invoke();

        //         if (runner.LocalPlayer.PlayerId == 1)
        //         {
        //             Debug.Log("\ud83d\udc51 Este jugador actu\u00e1 como HOST forzado (PlayerId == 1)");
        //             StartCoroutine(RegisterAllPlayersAfterSceneLoad());
        //         }
        //         else
        //         {
        //             Debug.Log("\ud83d\uded1 Este jugador no actuar\u00e1 como host");
        //         }
        //     }
        // }

       public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
{
    _playersJoined++;
    Debug.Log($"Player {player.PlayerId} joined. Total players: {_playersJoined}");

    // Solo el host lógico (PlayerId == 1) debe hacer los spawns
    if (Runner.LocalPlayer.PlayerId == 1)
    {
        Vector3 spawnPos = player.PlayerId == 1 ? new Vector3(-5, 1, 0) : new Vector3(5, 1, 0);

        if (runner.GetPlayerObject(player) == null)
        {
            var obj = runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, player);
            Debug.Log($"🚀 Spawning player {player.PlayerId} at {spawnPos}");

            if (obj.TryGetComponent<NetworkWallet>(out var walletComp))
            {
                walletComp.WalletAddress = PlayerSessionData.WalletAddress;
                walletComp.MatchId = PlayerSessionData.MatchId;
            }

            runner.SetPlayerObject(player, obj);
        }
    }

    if (_playersJoined == 2)
    {
        Debug.Log("Game Ready");
         OnRoomFull?.Invoke();

        if (Runner.LocalPlayer.PlayerId == 1)
        {
            Debug.Log("👑 Este jugador actuá como HOST forzado (PlayerId == 1)");
            StartCoroutine(RegisterAllPlayersAfterSceneLoad());
        }
        else
        {
            Debug.Log("🛑 Este jugador no actuará como host");
        }
    }
}


        public void OnSceneLoadDone(NetworkRunner runner)
        {
            Debug.Log("\u2705 SceneLoadDone - evaluando si este jugador actuar\u00e1 como host l\u00f3gico");
            Debug.Log($"\ud83d\udd0d IsServer: {runner.IsServer}");

            if (runner.LocalPlayer.PlayerId == 1)
            {
                Debug.Log("\ud83d\udc51 Este jugador actuar\u00e1 como HOST l\u00f3gico (PlayerId == 1)");
                foreach (var player in runner.ActivePlayers)
                {
                    Debug.Log($"\ud83c\udfa9 Lanzando registro para PlayerRef {player.PlayerId}");
                    StartCoroutine(WaitAndRegisterAndStartIfReady(player));
                }
            }
            else
            {
                Debug.Log("\ud83d\uded1 Este jugador NO es el host. No se ejecutar\u00e1 el registro.");
            }
        }

        private IEnumerator WaitAndRegisterAndStartIfReady(PlayerRef player)
        {
            NetworkObject playerObj = null;
            PlayerMovement movement = null;

            yield return new WaitUntil(() => {
                playerObj = Runner.GetPlayerObject(player);
                return playerObj != null && playerObj.TryGetComponent(out movement);
            });

            yield return new WaitUntil(() => movement.HasBeenSpawned);

            if (!registeredPlayers.Contains(movement))
            {
                registeredPlayers.Add(movement);
                Debug.Log($"\u2705 Player registered. Total: {registeredPlayers.Count}");
            }

            if (registeredPlayers.Count == 2 && !gameStarted)
            {
                StartCoroutine(StartGameCountdown());
            }
        }

        private IEnumerator RegisterAllPlayersAfterSceneLoad()
        {
            Debug.Log("\u23f3 Esperando a que todos los jugadores est\u00e9n spawneados...");

            float timeout = 10f;
            float timer = 0f;

            while (registeredPlayers.Count < 2 && timer < timeout)
            {
                foreach (var playerRef in Runner.ActivePlayers)
                {
                    var playerObj = Runner.GetPlayerObject(playerRef);
                    if (playerObj != null && playerObj.TryGetComponent<PlayerMovement>(out var movement))
                    {
                        if (!registeredPlayers.Contains(movement) && movement.HasBeenSpawned)
                        {
                            registeredPlayers.Add(movement);
                            Debug.Log($"\u2705 Jugador registrado: {movement.name}. Total: {registeredPlayers.Count}");
                        }
                    }
                }

                timer += Time.deltaTime;
                yield return null;
            }

            if (registeredPlayers.Count == 2)
            {
                Debug.Log("\ud83c\udfaf Ambos jugadores registrados. Iniciando cuenta regresiva.");
                StartCoroutine(StartGameCountdown());
            }
            else
            {
                Debug.LogWarning("\u274c Timeout esperando a que se registren ambos jugadores.");
            }
        }

        private IEnumerator StartGameCountdown()
        {
            gameStarted = true;

            foreach (var player in registeredPlayers)
                player.Blocked = true;

            Debug.Log("\u23f3 Game starting in 6...");
            yield return new WaitForSeconds(1);
            Debug.Log("\u23f3 5...");
            yield return new WaitForSeconds(1);
            Debug.Log("\u23f3 4...");
            yield return new WaitForSeconds(1);
            Debug.Log("\u23f3 3...");
            yield return new WaitForSeconds(1);
            Debug.Log("\u23f3 2...");
            yield return new WaitForSeconds(1);
            Debug.Log("\u23f3 1...");
            yield return new WaitForSeconds(1);

            foreach (var player in registeredPlayers)
                player.Blocked = false;

            Debug.Log("\u2705 GAME STARTED!");
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

        // Required callbacks
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
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    }
}