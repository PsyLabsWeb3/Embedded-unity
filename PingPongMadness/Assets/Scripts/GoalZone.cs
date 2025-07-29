using Fusion;
using UnityEngine;
using BEKStudio;


public class GoalZone : NetworkBehaviour
{
    public enum Side { Left, Right }
    public Side goalSide;

    private async void OnTriggerEnter(Collider other)
    {
        // if (!HasStateAuthority) return;
         Debug.Log($"🎯 Trigger activado por: {other.name}");

        if (other.CompareTag("Ball"))
        {
            Debug.Log($"🎯 Gol en lado {goalSide}");

            if (NetworkManager.Instance != null)
            {
                await NetworkManager.Instance.OnGoalScored(goalSide);

            }
        }
    }
}
