using Fusion;
using UnityEngine;

public class GameStateManager : NetworkBehaviour
{
    public static GameStateManager Instance;

    [Networked] public bool GameStarted { get; set; }

    public override void Spawned()
    {
        if (Instance == null)
            Instance = this;

        Debug.Log("📡 GameStateManager Spawned");
    }
}


