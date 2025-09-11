using Fusion;
using FusionExamples.UIHelpers;
using FusionHelpers;
using Tanknarok.UI;
using TMPro;
using UnityEngine;
using EmbeddedAPI;
using System;
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

		private FusionLauncher.ConnectionStatus _status = FusionLauncher.ConnectionStatus.Disconnected;
		private GameMode _gameMode;
		private int _nextPlayerIndex;

		private string _matchId = null;

private CancellationTokenSource _regionCts;

private async Task<string> PickBestRegionCodeAsync(string fallback = "us", int timeoutMs = 6000)
{
    _regionCts?.Cancel();
    _regionCts?.Dispose();
    _regionCts = new CancellationTokenSource();

    using var timeout = new CancellationTokenSource(timeoutMs);
    using var linked  = CancellationTokenSource.CreateLinkedTokenSource(_regionCts.Token, timeout.Token);

    try
    {
        var regions = await NetworkRunner.GetAvailableRegions(cancellationToken: linked.Token).ConfigureAwait(false);
        if (regions == null || regions.Count == 0)
            return fallback;

        var valid = regions.Where(r => r.RegionPing >= 0).ToList();
        if (valid.Count == 0)
            return fallback;

        var best = valid.OrderBy(r => r.RegionPing).First(); // <-- ya no puede ser null
        Debug.Log($"[RegionPicker] Mejor región: {best.RegionCode} ({best.RegionPing} ms)");
        return best.RegionCode;
    }
    catch (ArgumentNullException ex) when (ex.ParamName == "scheduler")
    {
        Debug.LogWarning("[RegionPicker] scheduler==null al pedir regiones; usando fallback.");
        return fallback;
    }
    catch (OperationCanceledException)
    {
        Debug.LogWarning("[RegionPicker] Timeout/cancel al obtener regiones; usando fallback.");
        return fallback;
    }
    catch (Exception ex)
    {
        Debug.LogError($"[RegionPicker] Error inesperado: {ex.Message}; usando fallback.");
        return fallback;
    }
    finally
    {
        _regionCts?.Dispose();
        _regionCts = null;
    }
}



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

			  if (string.IsNullOrEmpty(address))
            {
                Debug.LogError("❌ WalletAddress no disponible en WalletManager");
                throw new System.Exception("WalletAddress requerido pero no encontrado en WalletManager");
            }

			string txID = WalletManager.TransactionId;
			if (string.IsNullOrEmpty(txID))
            {
                Debug.LogError("❌ TransactionId no disponible en WalletManager");
                throw new System.Exception("TransactionId requerido pero no encontrado en WalletManager");
            }

				// Define los valores hardcodeados
			string region = "ussc";           // o "us", "eu", etc.
			
			string bestRegionCode = await PickBestRegionCodeAsync(fallback: "us", timeoutMs: 6000);
 			Debug.Log($"Best region 🌏: {bestRegionCode}");
			

            _matchId = await API.RegisterPlayerAsync(address, txID, gameName, region);
            Debug.Log($"Match ID received from backend: {_matchId}");

			PlayerSessionData.WalletAddress = address;
			PlayerSessionData.MatchId = _matchId;

			// ✅ Verificar guardado
			Debug.Log($"📝 PlayerSessionData poblado: Wallet = {PlayerSessionData.WalletAddress}, MatchId = {PlayerSessionData.MatchId}");

		

			// Inicia conexión directamente
			FusionLauncher.Launch(
				_gameMode,
				region,
				_matchId,
				_gameManagerPrefab,
				_levelManager,
				OnConnectionStatusUpdate
			);

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
			
			if(intro)
				MusicPlayer.instance.SetLowPassTranstionDirection( -1f);
		}
	}
}