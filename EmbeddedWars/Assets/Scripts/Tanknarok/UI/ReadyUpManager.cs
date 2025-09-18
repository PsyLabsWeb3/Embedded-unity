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


		private Dictionary<PlayerRef, ReadyupIndicator> _readyUIs = new Dictionary<PlayerRef, ReadyupIndicator>();
		private float _delay;

		public GameObject DisconnectPrompt => _disconnectPrompt;
        private void Start()
        {
			_disconnectPrompt.SetActive(false);
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
			if (_delay > 0)
			{
				_delay -= Time.deltaTime;
				return;
			}

			if (playState != GameManager.PlayState.LOBBY )
			{
				foreach (ReadyupIndicator ui in _readyUIs.Values)
					LocalObjectPool.Release(ui);
				_readyUIs.Clear();
				gameObject.SetActive(false);
				return;
			}

			gameObject.SetActive(true);

			int playerCount = 0, readyCount = 0;
			foreach (FusionPlayer fusionPlayer in allPlayers)
			{
				Player player = (Player) fusionPlayer;
				if (player.ready)
					readyCount++;
				playerCount++;
			}

			foreach (ReadyupIndicator ui in _readyUIs.Values)
			{
				ui.Dirty();
			}

			foreach (FusionPlayer fusionPlayer in allPlayers)
			{
				Player player = (Player) fusionPlayer;

				ReadyupIndicator indicator;
				if (!_readyUIs.TryGetValue(player.PlayerId, out indicator))
				{
					indicator = LocalObjectPool.Acquire(_readyPrefab, Vector3.zero, Quaternion.identity, _readyUIParent);
					_readyUIs.Add(player.PlayerId, indicator);
				}
				if(indicator.Refresh(player))
					_audioEmitter.PlayOneShot();
			}

			bool allPlayersReady = readyCount == playerCount;

			// _disconnectInfoText.SetActive(!allPlayersReady);
			_readyupInfoText.SetActive(!allPlayersReady && playerCount > 1);

			if (allPlayersReady)
			{
				_delay = 2.0f;
				onAllPlayersReady();
			}

		}
	}
}