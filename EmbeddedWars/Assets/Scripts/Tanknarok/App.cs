using Fusion;
using FusionExamples.UIHelpers;
using FusionHelpers;
using Tanknarok.UI;
using TMPro;
using UnityEngine;
using EmbeddedAPI;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace FusionExamples.Tanknarok
{
	/// <summary>
	/// App entry point and main UI flow management.
	/// </summary>
	public class App : MonoBehaviour
	{
		[SerializeField] private LevelManager _levelManager;
		[SerializeField] private GameManager _gameManagerPrefab;
		[SerializeField] private TMP_InputField _room;
		[SerializeField] private TextMeshProUGUI _progress;
		[SerializeField] private Panel _uiCurtain;
		[SerializeField] private Panel _uiStart;
		[SerializeField] private Panel _uiProgress;
		[SerializeField] private Panel _uiRoom;
		[SerializeField] private GameObject _uiGame;
		[SerializeField] private TMP_Dropdown _regionDropdown;
		[SerializeField] private TextMeshProUGUI _audioText;

		[SerializeField] private NetworkRunner _runnerPrefab;


		

		private FusionLauncher.ConnectionStatus _status = FusionLauncher.ConnectionStatus.Disconnected;
		private GameMode _gameMode;
		private int _nextPlayerIndex;

		private string _matchId = null;


		private void Awake()
		{
			Application.targetFrameRate = 60;
			DontDestroyOnLoad(this);
			_levelManager.onStatusUpdate = OnConnectionStatusUpdate;

			 // 🔒 Bloquea ReadyUp durante la fase de conexión
			if (_levelManager && _levelManager.readyUpManager)
				_levelManager.readyUpManager.gameObject.SetActive(false);
		}

		private async void Start()
		{
			_status = FusionLauncher.ConnectionStatus.Disconnected;

			_uiStart.SetVisible(false);
			_uiRoom.SetVisible(false);
			_uiProgress.SetVisible(true);

			 if (_progress) _progress.text = "Connecting…";

			_gameMode = GameMode.Shared;

			string gameName = "EmbeddedWars";
			string address = WalletManager.WalletAddress;
			string txID = WalletManager.TransactionId;

			if (string.IsNullOrEmpty(address))
			{
				Debug.LogError("❌ WalletAddress no disponible en WalletManager");
				throw new System.Exception("WalletAddress requerido pero no encontrado en WalletManager");
			}

			if (string.IsNullOrEmpty(txID))
			{
				Debug.LogError("❌ TransactionId no disponible en WalletManager");
				throw new System.Exception("TransactionId requerido pero no encontrado en WalletManager");
			}

			string bettingMode = WalletManager.GameMode;                 // "casual" | "betting"
			string betForApi   = WalletManager.BetAmount; // <-betAmount en string

			Debug.Log($"MODE 🎮 : {bettingMode}");
			Debug.Log($"BET AMOUNT💰: {betForApi}");



			// ✅ Obtener mejor región con NetworkRunner temporal (evita error scheduler==null en WebGL)
			string regionCode = await RegionPicker.GetBestRegionViaRunnerAsync(_runnerPrefab, fallback: "us", timeoutMs: 6000);
			Debug.Log($"🌍 Región seleccionada: {regionCode}");

			// 🔐 Registrar jugador con backend
			_matchId = await API.RegisterPlayerAsync(address, txID, gameName, regionCode, bettingMode, betForApi);
			Debug.Log($"Match ID recibido desde backend: {_matchId}");

			// 🧠 Guardar datos de sesión
			PlayerSessionData.WalletAddress = address;
			PlayerSessionData.MatchId = _matchId;

			Debug.Log($"📝 PlayerSessionData: Wallet = {address}, MatchId = {_matchId}");

			// 🚀 Lanzar juego
			FusionLauncher.Launch(
				_gameMode,
				regionCode,
				_matchId,
				_gameManagerPrefab,
				_levelManager,
				OnConnectionStatusUpdate
			);

		// 👉 Notificar que el jugador se ha unido
		_ = API.JoinMatchAsync(_matchId, address);
	}


		private void Update()
		{
			if (_uiProgress.isShowing)
			{
				if (Input.GetKeyUp(KeyCode.Escape))
				{
					NetworkRunner runner = FindObjectOfType<NetworkRunner>();
					if (runner != null && !runner.IsShutdown)
					{
						// Calling with destroyGameObject false because we do this in the OnShutdown callback on FusionLauncher
						runner.Shutdown(false);
					}
				}
				UpdateUI();
			}
		}

		// What mode to play - Called from the start menu
		public void OnHostOptions()
		{
			SetGameMode(GameMode.Host);
		}

		public void OnJoinOptions()
		{
			SetGameMode(GameMode.Client);
		}

		public void OnSharedOptions()
		{
			SetGameMode(GameMode.Shared);
		}

		public void OnCancel()
        {
			if (GateUI(_uiRoom))
				_uiStart.SetVisible(true);
        }

		private void SetGameMode(GameMode gamemode)
		{
			_gameMode = gamemode;
			if (GateUI(_uiStart))
				_uiRoom.SetVisible(true);
		}

		// public void OnEnterRoom()
		// {
		// 	if (GateUI(_uiRoom))
		// 	{
		// 		// Get region from dropdown
		// 		string region = string.Empty;
		// 		if (_regionDropdown.value > 0)
        //         {
		// 			region = _regionDropdown.options[_regionDropdown.value].text;
		// 			region = region.Split(" (")[0];
        //         }

		// 		FusionLauncher.Launch(_gameMode, region, _room.text, _gameManagerPrefab, _levelManager, OnConnectionStatusUpdate);
		// 	}
		// }
		public void OnEnterRoom()
			{
				string hardcodedRoom = "default-room-001";
				string hardcodedRegion = "";

				if (GateUI(_uiRoom)) 
				{

							FusionLauncher.Launch(
								_gameMode,
								hardcodedRegion,
								hardcodedRoom,
								_gameManagerPrefab,
								_levelManager,
								OnConnectionStatusUpdate
							);
				}
			}


        // public async void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        // {
        //     // Debug.Log($"Player {player.PlayerId} left.✅ ");
		// 	Debug.Log($"ON PLAYER LEFT✅ ");
        //     string winnerWallet = PlayerSessionData.WalletAddress;
        //     string matchId = PlayerSessionData.MatchId;
        //     Debug.Log($"Reporting match result. Winner: {winnerWallet}");
        //     await API.ReportMatchResultAsync(matchId, winnerWallet);
        // }


		/// <summary>
		/// Call this method from button events to close the current UI panel and check the return value to decide
		/// if it's ok to proceed with handling the button events. Prevents double-actions and makes sure UI panels are closed. 
		/// </summary>
		/// <param name="ui">Currently visible UI that should be closed</param>
		/// <returns>True if UI is in fact visible and action should proceed</returns>
		private bool GateUI(Panel ui)
		{
			if (!ui.isShowing)
				return false;
			ui.SetVisible(false);
			return true;
		}

		private void OnConnectionStatusUpdate(NetworkRunner runner, FusionLauncher.ConnectionStatus status, string reason)
		{
			if (!this)
				return;

			Debug.Log(status);

			if (status != _status)
			{
				switch (status)
				{
					case FusionLauncher.ConnectionStatus.Disconnected:
						ErrorBox.Show("Disconnected!", reason, () => { });
						break;
					case FusionLauncher.ConnectionStatus.Failed:
						ErrorBox.Show("Error!", reason, () => { });
						break;
				}
			}

			_status = status;
			UpdateUI();
		}

		public void ToggleAudio()
        {
			AudioListener.volume = 1f - AudioListener.volume;
			_audioText.text = AudioListener.volume > 0.5f ? "ON" : "OFF";
        }

		private void UpdateUI()
		{
			bool intro = false;
			bool progress = false;
			bool running = false;

			switch (_status)
			{
				case FusionLauncher.ConnectionStatus.Disconnected:
					_progress.text = "Disconnected!";
					intro = true;
					break;
				case FusionLauncher.ConnectionStatus.Failed:
					_progress.text = "Failed!";
					intro = true;
					break;
				case FusionLauncher.ConnectionStatus.Connecting:
					_progress.text = "Connecting";
					progress = true;
					break;
				case FusionLauncher.ConnectionStatus.Connected:
					_progress.text = "Connected";
					progress = true;
					break;
				case FusionLauncher.ConnectionStatus.Loading:
					_progress.text = "Loading";
					progress = true;
					break;
				case FusionLauncher.ConnectionStatus.Loaded:
					running = true;
					break;
			}

				_uiCurtain.SetVisible(!running);
				_uiStart.SetVisible(intro);
				_uiProgress.SetVisible(progress);
				_uiGame.SetActive(running);
			 

				  // 🟢 Al estar Loaded, habilita ReadyUp (Lobby real)
			if (_levelManager && _levelManager.readyUpManager){
				_levelManager.readyUpManager.gameObject.SetActive(running);
			}
			
			if(intro)
				MusicPlayer.instance.SetLowPassTranstionDirection( -1f);
		}
	}
}