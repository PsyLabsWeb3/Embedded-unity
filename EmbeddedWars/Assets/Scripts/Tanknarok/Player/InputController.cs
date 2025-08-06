using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace FusionExamples.Tanknarok
{
	/// <summary>
	/// Handle player input by responding to Fusion input polling, filling an input struct and then working with
	/// that input struct in the Fusion Simulation loop.
	/// </summary>
		public enum InputMode
		{
			MOUSE_KEYBOARD,
			MOBILE
		}
	public class InputController : NetworkBehaviour, INetworkRunnerCallbacks
	{
		[SerializeField] private LayerMask _mouseRayMask;

		public static bool fetchInput = true;

	

		private Player _player;
		private NetworkInputData _inputData = new NetworkInputData();
		private Vector2 _moveDelta;
		private Vector2 _aimDelta;
		private Vector2 _leftPos;
		private Vector2 _leftDown;
		private Vector2 _rightPos;
		private Vector2 _rightDown;
		private bool _leftTouchWasDown;
		private bool _rightTouchWasDown;

		private MobileInput _mobileInput;

		private uint _buttonReset;
		private uint _buttonSample;

		 public static InputMode CurrentInputMode = InputMode.MOUSE_KEYBOARD;

		/// <summary>
		/// Hook up to the Fusion callbacks so we can handle the input polling
		/// </summary>
		public override void Spawned()
		{
			_mobileInput = FindObjectOfType<MobileInput>(true);
			_player = GetComponent<Player>();
			// Technically, it does not really matter which InputController fills the input structure, since the actual data will only be sent to the one that does have authority,
			// but in the name of clarity, let's make sure we give input control to the gameobject that also has Input authority.
			if (Object.HasInputAuthority)
			{
				Runner.AddCallbacks(this);

			}

			Debug.Log("Spawned [" + this + "] IsClient=" + Runner.IsClient + " IsServer=" + Runner.IsServer + " IsInputSrc=" + Object.HasInputAuthority + " IsStateSrc=" + Object.HasStateAuthority);

		}

		/// <summary>
		/// Get Unity input and store them in a struct for Fusion
		/// </summary>
		/// <param name="runner">The current NetworkRunner</param>
		/// <param name="input">The target input handler that we'll pass our data to</param>
		public void OnInput(NetworkRunner runner, NetworkInput input)
		{
				
			if (_player!=null && _player.Object!=null && _player.stage == Player.Stage.Active)
			{
				_inputData.aimDirection = _aimDelta.normalized;
				_inputData.moveDirection = _moveDelta.normalized;
				_inputData.Buttons = _buttonSample;
				_buttonReset |= _buttonSample; // This effectively delays the reset of the read button flags until next Update() in case we're ticking faster than we're rendering
				Debug.Log($"🔁 OnInput called, move={_moveDelta}, aim={_aimDelta}, buttons={_buttonSample}");
			}

			// Hand over the data to Fusion
			input.Set(_inputData);
			_inputData.Buttons = 0;
		}
		
		private void Update()
{
	_buttonSample &= ~_buttonReset;

	if (CurrentInputMode == InputMode.MOUSE_KEYBOARD)
	{
		if (Input.GetMouseButton(0))
			_buttonSample |= NetworkInputData.BUTTON_FIRE_PRIMARY;

		if (Input.GetMouseButton(1))
			_buttonSample |= NetworkInputData.BUTTON_FIRE_SECONDARY;

		_moveDelta = Vector2.zero;

		if (Input.GetKey(KeyCode.W))
			_moveDelta += Vector2.up;

		if (Input.GetKey(KeyCode.S))
			_moveDelta += Vector2.down;

		if (Input.GetKey(KeyCode.A))
			_moveDelta += Vector2.left;

		if (Input.GetKey(KeyCode.D))
			_moveDelta += Vector2.right;

		Vector3 mousePos = Input.mousePosition;
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(mousePos);
		Vector3 mouseCollisionPoint = Vector3.zero;

		if (Physics.Raycast(ray, out hit, Mathf.Infinity, _mouseRayMask))
		{
			if (hit.collider != null)
			{
				mouseCollisionPoint = hit.point;
			}
		}

		Vector3 aimDirection = mouseCollisionPoint - _player.turretPosition;
		_aimDelta = new Vector2(aimDirection.x, aimDirection.z);
	}
	else if (CurrentInputMode == InputMode.MOBILE)
	{
		if (_mobileInput != null && _mobileInput.gameObject.activeInHierarchy)
	{
		_moveDelta = _mobileInput.MoveInput;
		_aimDelta = _mobileInput.AimInput;

		Debug.Log($"📲 MOBILE INPUT: moveDelta={_moveDelta}, aimDelta={_aimDelta}");

		if (_mobileInput.RightReleasedThisFrame())
		{
			Debug.Log("🎯 Right joystick released → FIRE PRIMARY");
			_buttonSample |= NetworkInputData.BUTTON_FIRE_PRIMARY;
		}

		if (_mobileInput.LeftReleasedThisFrame() && _moveDelta.magnitude < 0.01f)
		{
			Debug.Log("💣 Left joystick tap (no move) → FIRE SECONDARY");
			_buttonSample |= NetworkInputData.BUTTON_FIRE_SECONDARY;
		}
	}
}
}
		public void ToggleReady()
		{
			_buttonSample |= NetworkInputData.BUTTON_TOGGLE_READY;
		}

		public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
		public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
		public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
		public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
		public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
		public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
		public void OnConnectedToServer(NetworkRunner runner) { }
		public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {}
		
		public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
		public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
		public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
		public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
		public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
		public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
		public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {}

		public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
		public void OnSceneLoadDone(NetworkRunner runner) { }
		public void OnSceneLoadStart(NetworkRunner runner) { }
	}

	public struct NetworkInputData : INetworkInput
	{
		public const uint BUTTON_FIRE_PRIMARY = 1 << 0;
		public const uint BUTTON_FIRE_SECONDARY = 1 << 1;
		public const uint BUTTON_TOGGLE_READY = 1 << 2;

		public uint Buttons;
		public Vector2 aimDirection;
		public Vector2 moveDirection;

		public bool IsUp(uint button)
		{
			return IsDown(button) == false;
		}

		public bool IsDown(uint button)
		{
			return (Buttons & button) == button;
		}

		public bool WasPressed(uint button, NetworkInputData oldInput)
		{
			return (oldInput.Buttons & button) == 0 && (Buttons&button)==button;
		}
	}
}