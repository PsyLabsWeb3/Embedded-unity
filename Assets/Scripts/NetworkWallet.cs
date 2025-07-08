using Fusion;

public class NetworkWallet : NetworkBehaviour
{
    [Networked] public string WalletAddress { get; set; }
    [Networked] public string MatchId { get; set; }
}