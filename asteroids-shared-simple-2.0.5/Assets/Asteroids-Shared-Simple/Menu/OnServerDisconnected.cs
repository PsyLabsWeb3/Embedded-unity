using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Asteroids.SharedSimple
{
    public class OnServerDisconnected : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private string _menuSceneName = String.Empty;

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
             Debug.Log($"Runner shutdown. Reason={shutdownReason}");

                // 👉 Aquí cargas tu nueva escena de desconexión
                SceneManager.LoadScene("Disconnection");
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log("Player Joined");
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log("Player Left");

            //If Match not reported report it 
            if (PlayerSessionData.MatchReported == false)
            {
                string matchId = PlayerSessionData.MatchId;
				string wallet = PlayerSessionData.WalletAddress;

                if (string.IsNullOrEmpty(wallet) || string.IsNullOrEmpty(matchId))
                {
                    Debug.LogWarning("❌ Wallet address or MatchId no disponible, no se puede reportar el abandono del match.");
                    return;
                }
                else
                {
                    Debug.Log($"🏆 Local player is the WINNER. Reporting match. MatchId: {matchId}, Wallet: {wallet}");
					_ = EmbeddedAPI.API.ReportMatchResultAsync(matchId, wallet);
                    	JsBridge.NotifyGameOver("Dissconected", matchId );
                    PlayerSessionData.MatchReported = true;
                }

            }

        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token)
        {
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }
    }
}
