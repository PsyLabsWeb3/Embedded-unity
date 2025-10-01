using System;
using System.Collections.Generic;
using Fusion;
using FusionHelpers;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EmbeddedAPI;
using System.Threading.Tasks;


namespace FusionExamples.Tanknarok
{
	public class ReadyUpManager : MonoBehaviour
	{
		// [SerializeField] private GameObject _disconnectInfoText;
		[SerializeField] private GameObject _readyupInfoText;
		[SerializeField] private Transform _readyUIParent;
		[SerializeField] private ReadyupIndicator _readyPrefab;
		[SerializeField] private AudioEmitter _audioEmitter;
		[SerializeField] private GameObject _disconnectPrompt;
		[SerializeField] private Button _readyUpButton;

		// ⏱️ UI opcional para mostrar el tiempo restante
		[SerializeField] private Text _timerText; // Asigna un Text (UGUI). Opcional.

		[SerializeField] private Button _abortButton;       
		private bool _abortInProgress = false;


		private Dictionary<PlayerRef, ReadyupIndicator> _readyUIs = new Dictionary<PlayerRef, ReadyupIndicator>();
		private float _delay;

		private int _countdownSeconds = 30;
		private float _countdownRemaining = -1f;     // -1 = inactivo
		private bool _countdownActive = false;
		private bool _autoStartTriggered = false;

		public GameObject DisconnectPrompt => _disconnectPrompt;

		private void Start()
		{
			_disconnectPrompt.SetActive(false);
			UpdateTimerText(false);

			if (_abortButton)
			{
				_abortButton.onClick.RemoveAllListeners();
				_abortButton.onClick.AddListener(OnAbortClicked);
				_abortButton.gameObject.SetActive(false);
			}

		}

		public void OnReadyUpClicked()
		{
			Debug.Log("OnReadyUpClicked🚨 ");
			var runner = FindFirstObjectByType<NetworkRunner>();
			if (runner == null || runner.LocalPlayer == null)
			{
				Debug.LogWarning("❌ No NetworkRunner or LocalPlayer found");
				return;
			}

			// Buscar al jugador local y su InputController
			foreach (var player in FindObjectsByType<Player>(FindObjectsSortMode.None))
			{
				if (player.Object != null && player.Object.InputAuthority == runner.LocalPlayer)
				{
					var input = player.GetComponent<InputController>();
					if (input != null)
					{
						Debug.Log("✅ Calling ToggleReady on local InputController");
						input.ToggleReady();
						return;
					}
				}
			}

			Debug.LogWarning("❌ No InputController with InputAuthority found!");
		}

		public void AttemptDisconnect()
		{
			GameManager gm = FindFirstObjectByType<GameManager>();
			if (gm == null)
				return;

			gm.DisconnectByPrompt = true;
		}

		public void UpdateUI(GameManager.PlayState playState, IEnumerable<FusionPlayer> allPlayers, Action onAllPlayersReady)
		{
			if (_delay > 0f)
			{
				_delay -= Time.deltaTime;
				return;
			}

			if (playState != GameManager.PlayState.LOBBY)
			{
				StopAndResetCountdown();
				foreach (ReadyupIndicator ui in _readyUIs.Values)
					LocalObjectPool.Release(ui);
				_readyUIs.Clear();
				gameObject.SetActive(false);
				return;
			}

			gameObject.SetActive(true);

			// ---- Recuento de jugadores y ready ----
			int playerCount = 0, readyCount = 0;
			foreach (FusionPlayer fusionPlayer in allPlayers)
			{
				Player player = (Player)fusionPlayer;
				if (player.ready) readyCount++;
				playerCount++;
			}

			foreach (ReadyupIndicator ui in _readyUIs.Values)
				ui.Dirty();

			foreach (FusionPlayer fusionPlayer in allPlayers)
			{
				Player player = (Player)fusionPlayer;

				if (!_readyUIs.TryGetValue(player.PlayerId, out var indicator))
				{
					indicator = LocalObjectPool.Acquire(_readyPrefab, Vector3.zero, Quaternion.identity, _readyUIParent);
					_readyUIs.Add(player.PlayerId, indicator);
				}
				if (indicator.Refresh(player))
					_audioEmitter.PlayOneShot();
			}

			RefreshAbortButtonVisibility(playState, playerCount);

			bool allPlayersReady = (playerCount > 0) && (readyCount == playerCount);

			// Info de ready
			_readyupInfoText.SetActive(!allPlayersReady && playerCount > 1);

			// ---- Flujo original: si TODOS están ready, inicia con delay y sal ----
			if (allPlayersReady)
			{
				StopAndResetCountdown();
				_delay = 2.0f;
				onAllPlayersReady?.Invoke();
				return;
			}

			// ==========================
			//   LÓGICA DE COUNTDOWN
			// ==========================
			// Arranca solo si hay 2+ jugadores y NO todos están ready
			if (playerCount >= 2)
			{
				if (!_countdownActive)
					StartCountdown();

				// Si está activo, descontar
				if (_countdownActive)
				{
					_countdownRemaining -= Time.deltaTime;
					if (_countdownRemaining < 0f) _countdownRemaining = 0f;
					UpdateTimerText(true);

					// Autoinicio una sola vez por cliente
					if (_countdownRemaining <= 0f && !_autoStartTriggered)
					{
						Debug.Log("⏱️ Countdown llegó a 0. Iniciando partida automáticamente…");
						_autoStartTriggered = true;
						_delay = 2.0f;
						onAllPlayersReady?.Invoke();
					}
				}
			}
			else
			{
				// Menos de 2 jugadores -> reset
				if (_countdownActive)
					StopAndResetCountdown();
			}
		}

		// ===== Botón de ABORTAR partida (solo en LOBBY con 1 jugador) =====
		private void RefreshAbortButtonVisibility(GameManager.PlayState playState, int playerCount)
		{
			if (_abortButton == null || _abortInProgress)
			{
				if (_abortButton) _abortButton.gameObject.SetActive(false);
				return;
			}

			// Mostrar solo en LOBBY y cuando hay exactamente 1 jugador
			bool shouldShow = (playState == GameManager.PlayState.LOBBY) && (playerCount == 1);

			_abortButton.gameObject.SetActive(shouldShow);
			_abortButton.interactable = shouldShow && !_abortInProgress;
		}



		// ===== Helpers de Countdown =====

		private void StartCountdown()
		{
			_countdownActive = true;
			_autoStartTriggered = false;
			_countdownRemaining = _countdownSeconds;
			Debug.Log($"⏱️ Countdown iniciado: {_countdownSeconds}s");
			UpdateTimerText(true);
		}

		private void StopAndResetCountdown()
		{
			if (_countdownActive)
				Debug.Log("⏹️ Countdown detenido / reseteado.");

			_countdownActive = false;
			_autoStartTriggered = false;
			_countdownRemaining = -1f;
			UpdateTimerText(false);
		}

		private void UpdateTimerText(bool visible)
		{
			if (_timerText == null) return;

			if (!visible || _countdownRemaining < 0f)
			{
				_timerText.gameObject.SetActive(false);
				return;
			}

			_timerText.gameObject.SetActive(true);
			_timerText.text = Mathf.CeilToInt(_countdownRemaining).ToString();
		}

		// ===== Botón de ABORTAR partida (solo en LOBBY con 1 jugador) =====
		private async void OnAbortClicked()
		{
			if (_abortInProgress) return;

			string matchId = PlayerSessionData.MatchId;
			string wallet = PlayerSessionData.WalletAddress;

			if (string.IsNullOrEmpty(matchId) || string.IsNullOrEmpty(wallet))
			{
				Debug.LogError("[ReadyUpManager] Abort: faltan MatchId/WalletAddress.");
				return;
			}

			var runner = FindFirstObjectByType<NetworkRunner>();
			if (runner == null || runner.IsShutdown)
			{
				Debug.LogWarning("[ReadyUpManager] Abort: runner no disponible.");
				return;
			}

			// (Opcional) permitir abortar solo al que tiene autoridad de estado:
			var gm = FindFirstObjectByType<GameManager>();
			if (gm && !(gm.Object && gm.Object.HasStateAuthority))
			{
				Debug.LogWarning("[ReadyUpManager] Abort: sin StateAuthority.");
				return;
			}

			try
			{
				_abortInProgress = true;
				if (_abortButton) _abortButton.interactable = false;

				//Debug.Log("[ReadyUpManager] Abortando matchID y Player");
				Debug.Log($"[ReadyUpManager] Abortando matchID: {matchId}, Player: {wallet}");

				await API.AbortMatchAsync(matchId, wallet);
				Debug.Log("[ReadyUpManager] AbortMatchAsync OK — cerrando sesión.");

				await runner.Shutdown(false);
		
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[ReadyUpManager] Abort error: {ex.Message}");
			
				_abortInProgress = false;
				if (_abortButton) _abortButton.interactable = true;
			}
		}


	}
}