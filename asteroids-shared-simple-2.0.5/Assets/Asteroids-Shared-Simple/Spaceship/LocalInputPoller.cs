using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace Asteroids.SharedSimple
{
    public enum InputMode
    {
        MOUSE_KEYBOARD,
        MOBILE
    }

    public class LocalInputPoller : MonoBehaviour, INetworkRunnerCallbacks
    {
        // --- Desktop axes/buttons (Input Manager clásico) ---
        private const string AXIS_HORIZONTAL = "Horizontal";
        private const string AXIS_VERTICAL = "Vertical";
        private const string BUTTON_FIRE1 = "Fire1";   // Mouse0 / Ctrl

        [Header("Mobile (opcional)")]
        [SerializeField] private MobileInput mobileInput;       // si lo dejas vacío, lo buscamos en Start
        [SerializeField, Range(0f, 1f)] private float rightStickFireThreshold = 0.2f;

        // Estado del modo actual (el botón de UI que hicimos puede cambiarlo)
        public static InputMode CurrentInputMode = InputMode.MOUSE_KEYBOARD;

        // Buffers locales (se muestrean en Update; se envían en OnInput)
        private float _h;         // rotación (izq/der)
        private float _v;         // thrust (adelante/atrás)
        private bool _fireHeld;   // mantener disparo

        private void Start()
        {
            // Auto-find si no lo asignaste por inspector
            if (!mobileInput)
                mobileInput = FindFirstObjectByType<MobileInput>(FindObjectsInactive.Include);

            // Debug.Log($"[Poller] MobileInput: {mobileInput?.name} " +
            // $"active={mobileInput?.gameObject.activeInHierarchy} " +
            // $"left={mobileInput?.GetLeftName()} right={mobileInput?.GetRightName()}");

        }

        public void SetMobileInput(MobileInput mi)
        {
            mobileInput = mi;
        }

        private void Update()
        {
            if (CurrentInputMode == InputMode.MOUSE_KEYBOARD)
            {
                // Desktop: leer Input Manager
                _h = Input.GetAxis(AXIS_HORIZONTAL); // rotación
                _v = Input.GetAxis(AXIS_VERTICAL);   // thrust
                _fireHeld = Input.GetButton(BUTTON_FIRE1);
            }
            else
            {
                // Mobile: leer joysticks virtuales
                Vector2 left = mobileInput ? mobileInput.MoveInput : Vector2.zero;
                Vector2 right = mobileInput ? mobileInput.AimInput : Vector2.zero;

                // 👇 compara lectura del poller VS lectura directa del handler
                var lj = mobileInput ? mobileInput.GetType().GetField("leftJoystick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(mobileInput) as JoystickHandler : null;
                Vector2 rawLeft = lj ? lj.Input : Vector2.zero;

                // Debug.Log($"[Poller] mobileInput={mobileInput?.name}#{mobileInput?.GetInstanceID()} left={left} (rawLeft={rawLeft}) right={right}");

                // Joystick izquierdo: X=rotar, Y=thrust
                _h = Mathf.Clamp(left.x, -1f, 1f);
                _v = Mathf.Clamp(left.y, -1f, 1f);

                // Debug.Log($"📲 MOBILE INPUT: move={left}, aim={right}");

                // Disparo si:
                //  a) hay botón/tap en MobileInput (FireHeld true por 0.05s)
                //  b) mueves el joystick derecho por encima del umbral
                bool rightMoved = right.sqrMagnitude > (rightStickFireThreshold * rightStickFireThreshold);
                _fireHeld = (mobileInput && mobileInput.FireHeld) || rightMoved;
            }
        }

        // Fusion → se llama por tick para recolectar input y enviarlo
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var localInput = new SpaceshipInput
            {
                HorizontalInput = _h,
                VerticalInput = _v
            };

            localInput.Buttons.Set(SpaceshipButtons.Fire, _fireHeld);

            input.Set(localInput);
        }

        // --- Callbacks requeridos por la interfaz, sin uso por ahora ---
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}
