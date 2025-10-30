using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using EmbeddedAPI;

public class PvEBootstrap : MonoBehaviour
{
    [Header("Game identifier used by backend")]
    [SerializeField] private string gameName = "EmbeddedSpaceRace";

    private string _walletAddress;
    private string _txSignature;

    private bool _started;

    [Serializable]
    private class CredsDto
    {
        public string wallet;
        public string tx;
    }

    /// <summary>
    /// SendMessage("PvEBootstrap", "SetCredentialsJson", "{\"wallet\":\"...\",\"tx\":\"...\"}")
    /// </summary>
    public void SetCredentialsJson(string json)
    {
        try
        {
            var dto = JsonUtility.FromJson<CredsDto>(json);
            if (!string.IsNullOrEmpty(dto.wallet)) _walletAddress = dto.wallet;
            if (!string.IsNullOrEmpty(dto.tx)) _txSignature = dto.tx;
            TryStartFlow();
        }
        catch (Exception e)
        {
            Debug.LogError($"SetCredentialsJson parse error: {e}");
            FailAndExit();
        }
    }

    public void SetWalletAddress(string wallet)
    {
        _walletAddress = wallet;
        TryStartFlow();
    }

    public void SetTxSignature(string txSig)
    {
        _txSignature = txSig;
        TryStartFlow();
    }

    private async void TryStartFlow()
    {
        if (_started) return;
        if (string.IsNullOrEmpty(_walletAddress) || string.IsNullOrEmpty(_txSignature)) return;

        _started = true;

        try
        {
            // public static Task<RegisterPvEResponse> RegisterPlayerPvEAsync(string walletAddress, string txSignature, string game)
            var res = await API.RegisterPlayerPvEAsync(_walletAddress, _txSignature, gameName);

            if (res == null || string.IsNullOrEmpty(res.matchId))
            {
                Debug.LogWarning("RegisterPlayerPvEAsync returned no matchId.");
                FailAndExit();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"PvE registration failed: {e}");
            FailAndExit();
        }
    }

    private void FailAndExit()
    {
        SceneManager.LoadScene(3); // GameEnded
    }

    // For local testing in Editor
    #if UNITY_EDITOR
    [Header("Editor testing")]
    [SerializeField] private bool autoTestInEditor = false;
    [SerializeField] private string editorWallet = "ABCDEF1234567890";
    [SerializeField] private string editorTx = "TX1234567890";

    private void Start()
    {
        if (Application.isEditor && autoTestInEditor)
        {
            _walletAddress = editorWallet;
            _txSignature = editorTx;
            TryStartFlow();
        }
    }
    #endif
}