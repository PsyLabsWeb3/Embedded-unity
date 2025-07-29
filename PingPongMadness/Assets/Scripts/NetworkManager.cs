using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using EmbeddedAPI;
using System;
using System.Threading.Tasks;


namespace BEKStudio
{
    public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        public static NetworkManager Instance;
        private List<PlayerMovement> registeredPlayers = new List<PlayerMovement>();
        private bool gameStarted = false;
        private bool gameEnded = false;
        private bool ballSpawned = false;


        [SerializeField] private NetworkObject ballPrefab;
        private NetworkObject ballInstance;


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

        if (player.PlayerId <= 0)
    {
        Debug.LogWarning($"⛔ Invalid PlayerId: {player.PlayerId}. Skipping spawn.");
        return;
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
                 PlayerMovement player1 = registeredPlayers[0];
                 PlayerMovement player2 = registeredPlayers[1];
                 string wallet1 = player1.WalletAddress;
                 string wallet2 = player2.WalletAddress;

                   Debug.Log($"📡 Player 1 Wallet: {wallet1}");
                   Debug.Log($"📡 Player 2 Wallet: {wallet2}");

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

            // foreach (var player in registeredPlayers)
            //     player.Blocked = true;
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.CountdownValue = 6;

            for (int i = 5; i >= 1; i--)
            {
                Debug.Log($"⏳ {i}...");
                yield return new WaitForSeconds(1);

                if (GameStateManager.Instance != null)
                    GameStateManager.Instance.CountdownValue = i;
            }

             // Este código solo lo ejecuta el host lógico (PlayerId == 1)
          if (Runner.LocalPlayer.PlayerId == 1 && GameStateManager.Instance != null)
            {
                if (!ballSpawned)
                {
                    GameStateManager.Instance.GameStarted = true; // ✅ se replica a todos
                    Debug.Log("✅ GameStarted replicado");

                    var hostPlayerRef = Runner.LocalPlayer;
                    Vector3 position = Vector3.zero;

                    Runner.Spawn(ballPrefab, position, Quaternion.identity, hostPlayerRef);
                    Debug.Log("🏐 Ball spawned by host.");

                    ballSpawned = true; // ✅ evita doble spawn
                }
                else
                {
                    Debug.Log("⚠️ Ball spawn skipped: already spawned.");
                }
            }

            Debug.Log("✅ GAME STARTED!");
        }

      public async Task OnGoalScored(GoalZone.Side side)

        {
            if (gameEnded) return;
            gameEnded = true;
            GameStateManager.Instance.GameEnded  = true;
            GameStateManager.Instance.SetGameEnded();
            

            // string winner = side == GoalZone.Side.Left ? "2" : "1";
            string winner = (side == GoalZone.Side.Left) ? "2" : "1";

            Debug.Log($"🏆 ¡{winner} gana!");

            GameStateManager.Instance.SetWinner(winner);

           // Determinar índice del ganador según el lado donde cayó el gol
            int winnerIndex = (side == GoalZone.Side.Left) ? 1 : 0; // Si el gol fue en el lado izquierdo, gana el de la derecha

            

            if (registeredPlayers.Count == 2)
            {
                // Asegura que estás accediendo al jugador, no a un string
                PlayerMovement winnerPlayer = registeredPlayers[winnerIndex];

                // Ahora intenta obtener el componente NetworkWallet
                if (winnerPlayer.TryGetComponent<NetworkWallet>(out var walletComp))
                {
                    string wallet = walletComp.WalletAddress;
                    string matchId = PlayerSessionData.MatchId;

                    Debug.Log($"🏆 Reportando ganador: Wallet={wallet}, MatchID={matchId}");

                    wallet = wallet?.Trim();
                    matchId = matchId?.Trim();

                    await API.ReportMatchResultAsync(matchId, wallet);
                  
                }
                else
                {
                    Debug.LogWarning("⚠️ No se encontró NetworkWallet en el jugador ganador");
                }
                }
                        // Opcional: despawn ball
                        if (ballInstance != null)
                        {
                            Runner.Despawn(ballInstance);
                        }
            }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            Vector2 direction = Vector2.zero;
            if (Input.GetKey(KeyCode.UpArrow)) direction = Vector2.up;
            if (Input.GetKey(KeyCode.DownArrow)) direction = Vector2.down;
            // if (Input.GetKey(KeyCode.LeftArrow)) direction = Vector2.left;
            // if (Input.GetKey(KeyCode.RightArrow)) direction = Vector2.right;

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