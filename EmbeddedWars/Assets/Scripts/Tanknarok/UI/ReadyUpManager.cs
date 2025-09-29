using System;
using System.Collections.Generic;
using Fusion;
using FusionHelpers;
using UnityEngine;
using UnityEngine.UI;


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
				// ===== Helpers de Countdown =====

		private void StartCountdown()
		{
			_countdownActive = true;
			_autoStartTriggered = false;
			_countdownRemaining = _countdownSeconds;
			Debug.Log($"⏱️ Countdown iniciado: { _countdownSeconds }s");
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

	}
}