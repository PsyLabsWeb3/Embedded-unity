using Fusion;
using UnityEngine;

public class BallController : NetworkBehaviour
{
    [Networked] private Vector3 Velocity { get; set; }
    private Rigidbody _rb;

    public float speed = 8f;

    public override void Spawned()
    {
        _rb = GetComponent<Rigidbody>();

        if (HasStateAuthority)
            LaunchBall();
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            transform.position += Velocity * Runner.DeltaTime;
        }
    }

    public void LaunchBall()
    {
        Vector3 dir = Random.value < 0.5f ? Vector3.right : Vector3.left;
        dir += new Vector3(0, 0, Random.Range(-0.3f, 0.3f)); // añade algo de ángulo
        dir.Normalize();
        Velocity = dir * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!HasStateAuthority) return;

        Vector3 reflect = Vector3.Reflect(Velocity.normalized, collision.contacts[0].normal);
        Velocity = reflect * speed;
    }

    public void ResetPosition()
    {
        transform.position = Vector3.zero;
        LaunchBall();
    }
}
