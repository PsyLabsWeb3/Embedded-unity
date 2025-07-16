using Fusion;
using UnityEngine;

public class BallController : NetworkBehaviour
{
    private Rigidbody _rb;

    public float initialSpeed = 0.5f;

    public override void Spawned()
    {
        _rb = GetComponent<Rigidbody>();

        if (HasStateAuthority)
        {
            // Movimiento inicial en dirección aleatoria
            Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-0.5f, 0.5f)).normalized;
            _rb.linearVelocity = direction * initialSpeed;
        }
    }

 public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || _rb == null) return;

        // Reaplica velocidad si se detiene (opcional)
        if (_rb.linearVelocity.magnitude < 0.1f)
        {
            Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-0.5f, 0.5f)).normalized;
            _rb.linearVelocity = _rb.linearVelocity.normalized * initialSpeed;
        }
    }
     private void OnCollisionEnter(Collision collision)
    {
        if (!HasStateAuthority) return;

        // Rebote en dirección reflejada
        Vector3 normal = collision.contacts[0].normal;
        Vector3 reflectedVelocity = Vector3.Reflect(_rb.linearVelocity, normal);
        _rb.linearVelocity = reflectedVelocity.normalized * initialSpeed;
    }
}
