using System;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;

namespace EmbeddedAPI
{
    public static class API
    {
        private static readonly string sharedSecret = "5f725f60-f959-4d54-a081-7a935bdf3194";
        private static readonly string baseUrl = "https://backend.embedded.games/api";

        [Serializable]
        public class RegisterPayload
        {
            public string walletAddress;
            public string txSignature;
            public string game;
            // public string? mode; // Optional, can be "Casual" or "Betting"
            // public decimal? betAmount; // Optional, required if mode is "Betting"
            // public decimal? matchFee; // Optional, required if mode is "Betting"
        }

        [Serializable]
        public class RegisterResponse
        {
            public string matchId;
        }

        [Serializable]
        public class MatchCompletePayload
        {
            public string matchID;
            public string winnerWallet;
        }

        [Serializable]
        private class MatchJoinPayload
        {
            public string matchID;
            public string walletAddress;
        }

        private static string GenerateSignature(string body, string timestamp)
        {
            var key = Encoding.UTF8.GetBytes(sharedSecret);
            var message = Encoding.UTF8.GetBytes(body + timestamp);

            using var hmac = new HMACSHA256(key);
            var hashBytes = hmac.ComputeHash(message);
            var hex = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            return hex;
        }

        private static void AddSecureHeaders(UnityWebRequest request, string body)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var signature = GenerateSignature(body, timestamp);

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("x-signature", signature);
            request.SetRequestHeader("x-timestamp", timestamp);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
        }

        // ✅ Helper para hacer await de UnityWebRequest en Unity 2021.3
        private static Task<UnityWebRequest> SendWebRequestAsync(UnityWebRequest request)
        {
            var tcs = new TaskCompletionSource<UnityWebRequest>();
            var operation = request.SendWebRequest();
            operation.completed += _ => tcs.SetResult(request);
            return tcs.Task;
        }

        public static async Task<string> RegisterPlayerAsync(string walletAddress, string txSignature, string game)
        {
            var payload = new RegisterPayload
            {
                walletAddress = walletAddress,
                txSignature = txSignature,
                game = game,
                // mode = mode,
                // betAmount = betAmount,
                // matchFee = matchFee
            };

            var body = JsonUtility.ToJson(payload);
            var request = new UnityWebRequest(baseUrl + "/registerPlayer", "POST");

            AddSecureHeaders(request, body);

            await SendWebRequestAsync(request);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + request.error);
                throw new InvalidOperationException("Failed to register player and retrieve match ID.");
            }

            RegisterResponse responseData = JsonUtility.FromJson<RegisterResponse>(request.downloadHandler.text);

            if (string.IsNullOrEmpty(responseData.matchId))
            {
                throw new InvalidOperationException("Failed to register player and retrieve match ID.");
            }

            return responseData.matchId;
        }

        public static async Task<string> JoinMatchAsync(string matchId, string walletAddress)
        {
            var payload = new MatchJoinPayload
            {
                matchID = matchId,
                walletAddress = walletAddress
            };

            var jsonBody = JsonUtility.ToJson(payload);
            var request = new UnityWebRequest(baseUrl + "/matchJoin", "POST");

            AddSecureHeaders(request, jsonBody);

            await SendWebRequestAsync(request);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + request.error);
                throw new InvalidOperationException($"Failed to join match with ID {matchId}.");
            }

            return request.downloadHandler.text;
        }

        public static async Task ReportMatchResultAsync(string matchId, string winnerWallet)
        {
            var payload = new MatchCompletePayload
            {
                matchID = matchId,
                winnerWallet = winnerWallet
            };

            var body = JsonUtility.ToJson(payload);
            var request = new UnityWebRequest(baseUrl + "/matchComplete", "POST");

            AddSecureHeaders(request, body);

            await SendWebRequestAsync(request);

            if (request.result == UnityWebRequest.Result.Success)
                Debug.Log("Success: " + request.downloadHandler.text);
            else
                Debug.LogError("Error: " + request.error);
        }
    }
}