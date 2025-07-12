using Fusion;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private CharacterController _controller;

    public float PlayerSpeed = 2f;

      public override void Spawned()
    {
        base.Spawned();

        // Forzar posición inicial solo en StateAuthority
        if (Object.HasStateAuthority)
        {
            _controller = GetComponent<CharacterController>();
            _controller.enabled = false;
            transform.position = Object.InputAuthority.PlayerId == 0 ? new Vector3(-3, 1, 0) : new Vector3(3, 1, 0);
            _controller.enabled = true;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority)
            return;

        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * Runner.DeltaTime * PlayerSpeed;

        _controller.Move(move);

        if (move != Vector3.zero)
        {
            transform.forward = move;
        }
    }
}