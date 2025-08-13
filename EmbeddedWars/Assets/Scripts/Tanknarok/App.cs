using Fusion;
using FusionExamples.UIHelpers;
using FusionHelpers;
using Tanknarok.UI;
using TMPro;
using UnityEngine;
using EmbeddedAPI;

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

		private FusionLauncher.ConnectionStatus _status = FusionLauncher.ConnectionStatus.Disconnected;
		private GameMode _gameMode;
		private int _nextPlayerIndex;

		private string _matchId = null;

		private void Awake()
		{
			Application.targetFrameRate = 60;
			DontDestroyOnLoad(this);
			_levelManager.onStatusUpdate = OnConnectionStatusUpdate;
		}

		private async void Start()
		{
			// OnConnectionStatusUpdate( null, FusionLauncher.ConnectionStatus.Disconnected, "");

			_status = FusionLauncher.ConnectionStatus.Disconnected;

			// Ocultar UIs innecesarios
			_uiStart.SetVisible(false);
			_uiRoom.SetVisible(false);     // 👈 Oculta también el de Room
			_uiProgress.SetVisible(true);  // Muestra que está conectando

			// Establece el modo de juego (Host, Client o Shared)
			_gameMode = GameMode.Shared;

			string gameName = "EmbeddedWars";

			string address = WalletManager.WalletAddress;

			string wallet = !string.IsNullOrEmpty(address) ? address : "player_wallet_" + System.Guid.NewGuid();

            Debug.Log(string.IsNullOrEmpty(address) ? $"❌ WalletAddress no disponible, generado aleatorio: {wallet}" : $"✅ Usando wallet del jugador: {wallet}");

            string tx = "player_tx_" + System.Guid.NewGuid();
            _matchId = await API.RegisterPlayerAsync(wallet, tx, gameName);
            Debug.Log($"Match ID received from backend: {_matchId}");

			PlayerSessionData.WalletAddress = wallet;
			PlayerSessionData.MatchId = _matchId;

			// ✅ Verificar guardado
			Debug.Log($"📝 PlayerSessionData poblado: Wallet = {PlayerSessionData.WalletAddress}, MatchId = {PlayerSessionData.MatchId}");

			// Define los valores hardcodeados
			string region = "";           // o "us", "eu", etc.

			// Inicia conexión directamente
			FusionLauncher.Launch(
				_gameMode,
				region,
				_matchId,
				_gameManagerPrefab,
				_levelManager,
				OnConnectionStatusUpdate
			);
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
			
			if(intro)
				MusicPlayer.instance.SetLowPassTranstionDirection( -1f);
		}
	}
}