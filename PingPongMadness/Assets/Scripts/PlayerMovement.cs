using Fusion;
using UnityEngine;
using BEKStudio;

public class PlayerMovement : NetworkBehaviour
{
    private CharacterController _controller;
    public float PlayerSpeed = 100f;

   [Networked] public bool Blocked { get; set; } 


    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }


    public override void FixedUpdateNetwork()
    {
         if (!HasInputAuthority || GameStateManager.Instance == null || !GameStateManager.Instance.GameStarted)
        return;
         

        // if (Blocked) return;

                    if (MobileInputUI.Instance == null)
            {
                Debug.LogWarning("❌ MobileInputUI.Instance is null");
            }

                 

                float moveZ = Input.GetAxis("Vertical"); // PC
        if (MobileInputUI.Instance != null)
        {
            if (MobileInputUI.Instance.IsMovingUp)
                moveZ = 1f;
            else if (MobileInputUI.Instance.IsMovingDown)
                moveZ = -1f;
        }

                Vector3 move = new Vector3(0, 0, moveZ) * Runner.DeltaTime * PlayerSpeed;
                _controller.Move(move);
    
     
        // Vector3 move = new Vector3(0, 0, Input.GetAxis("Vertical")) * Runner.DeltaTime * PlayerSpeed;


        // _controller.Move(move);

        // if (move != Vector3.zero)
        // {
        //     transform.forward = move;
        // }
    }
    public bool HasBeenSpawned { get; private set; } = false;

    public override void Spawned()
    {
        HasBeenSpawned = true;
        Blocked = true;
        Debug.Log($"📌 Spawned() confirmado para Player {Object.InputAuthority.PlayerId}");
    }

       private void OnTriggerEnter(Collider other)
    {
        // if (!HasStateAuthority) return;
         Debug.Log($"🎯 PADTrigger activado por: {other.name} en Player {Object.InputAuthority.PlayerId}");

       
    }
}
