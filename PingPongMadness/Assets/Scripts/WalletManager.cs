using UnityEngine;

public class WalletManager : MonoBehaviour
{
    public static string WalletAddress;
    public static string TransactionId;
    
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
     public static string GetTransactionId()
    {
        return TransactionId;
    }


    public static string GetWalletAddress()
    {
        return WalletAddress;
    }
        private void Awake()
    {
        // Que este objeto no se destruya entre escenas
        DontDestroyOnLoad(gameObject);
    }
}