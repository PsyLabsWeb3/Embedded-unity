using Fusion;

public class NetworkWallet : NetworkBehaviour
{
    [Networked, Capacity(64)] public string WalletAddress { get; set; }
    [Networked] public string MatchId { get; set; }
}