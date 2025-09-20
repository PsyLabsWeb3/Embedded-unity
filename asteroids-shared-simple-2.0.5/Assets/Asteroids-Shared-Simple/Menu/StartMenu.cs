using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.SceneManagement;
using EmbeddedAPI;

namespace Asteroids.SharedSimple
{
    // A utility class which defines the behaviour of the various buttons and input fields found in the Menu scene
    public class StartMenu : MonoBehaviour
    {
        [SerializeField] private NetworkRunner _networkRunnerPrefab = null;

        [SerializeField] private TMP_InputField _nickName = null;

        // The Placeholder Text is not accessible through the TMP_InputField component so need a direct reference
        [SerializeField] private TextMeshProUGUI _nickNamePlaceholder = null;

        [SerializeField] private TMP_InputField _roomName = null;
        [SerializeField] private string _gameScenePath = null;

        private NetworkRunner _runnerInstance = null;

        private string _matchId = "";

        //Start
        public void Start()
        {
            Debug.Log("Start Application");
            StartShared();
        }


        // Attempts to start a new game session 
        public async void StartShared()
        {
            string address = WalletManager.WalletAddress;

            if (string.IsNullOrEmpty(address))
            {
                Debug.LogWarning("❌ Wallet address no disponible, usando valor por defecto.");
                return;
            }
            else
            {
                Debug.Log($"✅ Usando wallet address del jugador: {address}");
            }
           

            string tx = WalletManager.TransactionId;

            if (string.IsNullOrEmpty(tx))
            {
                Debug.LogWarning("❌ TransactionId no disponible, usando valor por defecto.");
                return;
            }
            else
            {
                Debug.Log($"✅ Usando TransactionId del match: {tx}");
            }

            string gameName = "Asteroids";
            string bestRegionCode = "ussc";


            string gameMode = WalletManager.GameMode;

            if (string.IsNullOrEmpty(gameMode))
            {
                gameMode = "Casual"; // Default to casual
                Debug.LogWarning("❌ GameMode no disponible, usando valor por defecto.");
            }
            else
            {
                Debug.Log($"✅ Usando GameMode: {gameMode}");
            }

        

            string betAmount = WalletManager.BetAmount;

            if (string.IsNullOrEmpty(betAmount))
            {
                betAmount = "0.5"; // Casual value
            }
            else
            {
                Debug.Log($"✅ Usando BetAmount: {betAmount}");
            }

            //Log data before registering
            Debug.Log($"Registering player with wallet: {address}, tx: {tx}, game: {gameName}, region: {bestRegionCode}, mode: {gameMode}, betAmount: {betAmount}");

            // Register the player and get a match ID from the backend
            _matchId = await API.RegisterPlayerAsync(address, tx, gameName, bestRegionCode, gameMode, betAmount);
            Debug.Log($"Match ID received from backend: {_matchId}");

            // string matchId = "AsteroidsRoom";
            SetPlayerData();
            StartGame(GameMode.Shared, _matchId, _gameScenePath);
        }

        //Start game in lobyby mode to wait for an opponent
       

        private void SetPlayerData()
        {
            //Slice wallet address to show only first 4 and last 4 characters
            string address = WalletManager.WalletAddress;
            string slicedAddress = address.Length > 8 ? address.Substring(0, 4) + "..." + address.Substring(address.Length - 4) : address;
            LocalPlayerData.NickName = slicedAddress;
            // if (string.IsNullOrWhiteSpace(_nickName.text))
            // {
            //     LocalPlayerData.NickName = _nickNamePlaceholder.text;
            // }
            // else
            // {
            //     LocalPlayerData.NickName = _nickName.text;
            // }
        }

        private async void StartGame(GameMode mode, string roomName, string sceneName)
        {
            _runnerInstance = FindObjectOfType<NetworkRunner>();
            if (_runnerInstance == null)
            {
                _runnerInstance = Instantiate(_networkRunnerPrefab);
            }

            // Let the Fusion Runner know that we will be providing user input
            _runnerInstance.ProvideInput = true;

            var startGameArgs = new StartGameArgs()
            {
                GameMode = mode,
                SessionName = roomName,
                Scene = SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath(_gameScenePath)),
                ObjectProvider = _runnerInstance.GetComponent<NetworkObjectPoolDefault>(),
            };

            // GameMode.Host = Start a session with a specific name
            // GameMode.Client = Join a session with a specific name
            await _runnerInstance.StartGame(startGameArgs);

            if (_runnerInstance.IsServer)
            {
                _runnerInstance.LoadScene(sceneName);
            }
        }
    }
}