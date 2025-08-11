using UnityEngine;

public class WalletManager : MonoBehaviour
{
    public static string WalletAddress;

    public void SetWalletAddress(string address)
    {
        WalletAddress = address;
        Debug.Log("📥 Wallet address received in Unity: " + WalletAddress);
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
