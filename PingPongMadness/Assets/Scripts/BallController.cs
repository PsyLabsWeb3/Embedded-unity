using Fusion;
using UnityEngine;

public class BallController : NetworkBehaviour
{
    private Rigidbody _rb;

    public float initialSpeed = 0.5f;

    private Vector3 lastVelocity;

        
    public override void Spawned()
    {
        _rb = GetComponent<Rigidbody>();

        if (HasStateAuthority)
        {
            // Forzar movimiento inicial con suficiente componente en X
            float x = Random.Range(0.5f, 1f) * (Random.value > 0.5f ? 1 : -1); // asegúrate que avanza en X
            float z = Random.Range(-0.5f, 0.5f); // opcional variación vertical

            Vector3 direction = new Vector3(x, 0, z).normalized;
            _rb.linearVelocity = direction * initialSpeed;
        }
    }


 public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || _rb == null) return;

        lastVelocity = _rb.linearVelocity;

        // Reaplica velocidad si se detiene (opcional)
        if (_rb.linearVelocity.magnitude < 0.05f)
        {
            Debug.Log("⚠️ Velocidad baja, relanzando pelota");
            Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            _rb.linearVelocity = direction * initialSpeed;
        }
    }
     private void OnCollisionEnter(Collision collision)
    {
        if (!HasStateAuthority) return;

        // Rebote en dirección reflejada
        // Vector3 normal = collision.contacts[0].normal;
        // Vector3 reflectedVelocity = Vector3.Reflect(_rb.linearVelocity, normal);
        // _rb.linearVelocity = reflectedVelocity.normalized * initialSpeed;
        ContactPoint contact = collision.contacts[0];
        Vector3 reflected = Vector3.Reflect(lastVelocity.normalized, contact.normal);

        // En caso de que el ángulo sea demasiado plano, fuerza inclinación mínima
        if (Mathf.Abs(reflected.z) < 0.2f)
        {
            reflected.z = Mathf.Sign(reflected.z) * 0.2f;
        }

        _rb.linearVelocity = reflected.normalized * initialSpeed;
    


    }
}
