using Fusion;
using UnityEngine;

public class GameStateManager : NetworkBehaviour
{
    public static GameStateManager Instance;

    [Networked] public bool GameStarted { get; set; }

    [Networked] public int CountdownValue { get; set; }
    
    [Networked] public string WinnerName { get; set; }

    [Networked] public bool GameEnded { get; set; }



    public override void Spawned()
    {
        if (Instance == null)
            Instance = this;

        Debug.Log("📡 GameStateManager Spawned");
    }

    public void SetWinner(string name)
    {
        if (!HasStateAuthority) return;

        WinnerName = name;
        Debug.Log($"📢 Ganador sincronizado: {WinnerName}");
        
    }

    public void SetGameEnded(){
         if (!HasStateAuthority) return;
        GameEnded = true;
          // Muestra u oculta la UI si el estado cambio
            // gameEndPanel.SetActive(true);
              Debug.Log($"GAME ENDED: {GameEnded}");

        
    }

}


