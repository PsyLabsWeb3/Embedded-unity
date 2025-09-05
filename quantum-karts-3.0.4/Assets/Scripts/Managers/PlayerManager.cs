using System.Collections;
using System.Collections.Generic;
using Quantum;
using UnityEngine;

public class PlayerManager : MonoSingleton<PlayerManager>
{
    public Dictionary<PlayerRef, KartViewController> PlayerKarts { get; private set; } = new();
    public Dictionary<PlayerRef, KartViewController> BotKarts { get; private set; } = new();

    public override void OnAwake()
    {
        QuantumEvent.Subscribe(this, (EventOnRaceStateChanged e) => OnRaceStateChanged(e));
    }

    private void OnRaceStateChanged(EventOnRaceStateChanged e)
    {
        if (e.state >= RaceState.Countdown && QuantumRunner.Default?.NetworkClient?.CurrentRoom != null)
        {
            QuantumRunner.Default.NetworkClient!.CurrentRoom!.IsOpen = false;
        }
    }

    public void RegisterPlayer(QuantumGame game, KartViewController kartView)
    {
        if (kartView.isAI)
        {
            BotKarts[kartView._AIIndex] = kartView;
        }
        else
        {
            PlayerKarts[kartView.PlayerRef] = kartView;
        }

        if (game.GetLocalPlayers().Count > 0 && kartView.PlayerRef == game.GetLocalPlayers()[0])
        {
            LocalPlayerManager.Instance.RegisterLocalPlayer(kartView);
        }
    }

    public List<KartViewController> GetAllKarts()
    {
        List<KartViewController> allKarts = new List<KartViewController>();

        foreach (var pair in PlayerKarts)
        {
            allKarts.Add(pair.Value);
        }

        foreach (var pair in BotKarts)
        {
            allKarts.Add(pair.Value);
        }

        return allKarts;
    }

    public void UnregisterPlayer(QuantumGame game, KartViewController kartView)
    {
        PlayerKarts.Remove(kartView.PlayerRef);
    }
}
