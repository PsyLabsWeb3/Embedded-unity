using UnityEngine;

public class WalletManager : MonoBehaviour
{
    public static string WalletAddress;
    public static string TransactionId { get; private set; }
    public static string GameMode { get; private set; }
    public static string BetAmount { get; private set; }


    public void SetWalletAddress(string address)
    {
        WalletAddress = address;
        Debug.Log("📥 Wallet address received in Unity: " + WalletAddress);
    }

    public void SetTransactionId(string txId)
    {
        TransactionId = txId;
        Debug.Log("📥 Transaction ID received in Unity: " + TransactionId);
    }

    public void SetGameMode (string mode) {
         GameMode = mode;
        Debug.Log("📥 Game Mode received in Unity: " + GameMode);

    }

     public void SetBetAmount(string amount)
    {
       BetAmount = amount;
        Debug.Log("📥 BetAmount received in Unity: " + BetAmount);
    }

    public static string GetWalletAddress()
    {
        return WalletAddress;
    }

      public static string GetTransactionId()
    {
        return TransactionId;
    }

     public static string GetGameMode()
    {
        return GameMode;
    }

      public static string GetBetAmount()
    {
        return BetAmount;
    }


     private void Awake()
    {
        // Que este objeto no se destruya entre escenas
        DontDestroyOnLoad(gameObject);
    }
}
