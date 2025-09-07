using Fusion;
using UnityEngine;

public class BallController : NetworkBehaviour
{
    private Rigidbody _rb;

    [Header("Plano de juego (XZ)")]
    [SerializeField] private bool lockToPlaneY = true;
    [SerializeField] private float planeY = 0.5f;     // 🔧 Ajusta a la altura de tus pads
    [SerializeField] private float launchYOffset = 0.02f;

    [Header("Speed")]
    [SerializeField] private float initialSpeed = 8f;
    [SerializeField] private float accelPerSecond = 0.6f;
    [SerializeField] private float maxSpeed = 20f;
    [Networked] private float CurrentSpeed { get; set; }

    [Header("Ángulo anti-paralelo")]
    [SerializeField, Range(0.0f, 0.99f)] private float minXFracAtServe = 0.85f;
    [SerializeField, Range(0.0f, 0.99f)] private float minXFracOnBounce = 0.60f;

    private Vector3 lastVelocity;

    public override void Spawned()
    {
        _rb = GetComponent<Rigidbody>();

        // CCD y restricciones para mantenerse en el plano
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.interpolation = RigidbodyInterpolation.None;
        _rb.sleepThreshold = 0f;

        // Congelar Y y rotaciones fuera del plano
        _rb.constraints = RigidbodyConstraints.FreezePositionY | 
                          RigidbodyConstraints.FreezeRotationX  | 
                          RigidbodyConstraints.FreezeRotationZ;

        // Colocar la bola exactamente en el plano al spawnear
        if (lockToPlaneY)
        {
            var p = _rb.position;
            p.y = planeY + launchYOffset;  // levántala un poquito
            _rb.position = p;
        }

        if (HasStateAuthority)
        {
            CurrentSpeed = initialSpeed;
            Vector3 dir = BiasedServeDir();
            SetLinearVelocity(dir * CurrentSpeed);

            // Asegurar que Y quede clavado y sin componente vertical
            if (lockToPlaneY)
            {
                var v = GetLinearVelocity(); v.y = 0f; SetLinearVelocity(v);
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || _rb == null) return;

        // Acelerar por tiempo
        float dt = Runner.DeltaTime;
        CurrentSpeed = Mathf.Min(maxSpeed, CurrentSpeed + accelPerSecond * dt);

        // Mantener dirección + magnitud
        Vector3 v = GetLinearVelocity();
        if (v.sqrMagnitude > 1e-6f)
        {
            Vector3 dir = v.normalized;

            // Si por acumulación numérica se vuelve paralelo, reforzar componente X
            if (Mathf.Abs(dir.x) < (minXFracOnBounce * 0.9f))
                dir = EnforceMinX(dir, minXFracOnBounce);

            // Forzar a plano XZ
            if (lockToPlaneY)
            {
                dir.y = 0f;
                var pos = _rb.position; pos.y = planeY; _rb.position = pos;
            }

            SetLinearVelocity(dir * CurrentSpeed);
        }

        // Relanzar si se “muere”
        if (GetLinearVelocity().magnitude < 0.05f)
        {
            Vector3 dir = BiasedServeDir();
            SetLinearVelocity(dir * Mathf.Max(CurrentSpeed, initialSpeed));

            if (lockToPlaneY)
            {
                var pos = _rb.position; pos.y = planeY + launchYOffset; _rb.position = pos;
                var vel = GetLinearVelocity(); vel.y = 0f; SetLinearVelocity(vel);
            }
        }

        lastVelocity = GetLinearVelocity();

        // Clamp duro al plano (última línea de defensa)
        if (lockToPlaneY)
        {
            if (Mathf.Abs(_rb.position.y - planeY) > 0.0005f)
            {
                var pos = _rb.position; pos.y = planeY; _rb.position = pos;
            }
            if (Mathf.Abs(_rb.linearVelocity.y) > 1e-5f)
            {
                var vel = _rb.linearVelocity; vel.y = 0f; _rb.linearVelocity = vel;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!HasStateAuthority || _rb == null) return;

        ContactPoint contact = collision.contacts[0];
        Vector3 reflected = Vector3.Reflect(lastVelocity.normalized, contact.normal);

        // Garantiza componente X significativa tras el rebote
        Vector3 dir = EnforceMinX(reflected.normalized, minXFracOnBounce);

        // Plano XZ
        if (lockToPlaneY) dir.y = 0f;

        SetLinearVelocity(dir * CurrentSpeed);

        // Re-coloca en el plano por si la normal tenía componente Y
        if (lockToPlaneY)
        {
            var pos = _rb.position; pos.y = planeY; _rb.position = pos;
            var vel = _rb.linearVelocity; vel.y = 0f; _rb.linearVelocity = vel;
        }
    }

    // ===== Helpers =====
    private Vector3 BiasedServeDir()
    {
        float thetaMax = Mathf.Acos(Mathf.Clamp(minXFracAtServe, 0.0001f, 0.9999f));
        float theta = Random.Range(-thetaMax, thetaMax);
        float sideX = Random.value < 0.5f ? -1f : 1f; // izquierda o derecha
        float x = Mathf.Cos(theta) * sideX;
        float z = Mathf.Sin(theta);
        return new Vector3(x, 0f, z).normalized;
    }

    private Vector3 EnforceMinX(Vector3 dir, float minXFrac)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 1e-6f) dir = new Vector3(1f, 0f, 0f);
        dir.Normalize();

        if (Mathf.Abs(dir.x) < minXFrac)
        {
            float sx = Mathf.Sign(dir.x == 0 ? 1f : dir.x);
            float sz = Mathf.Sign(dir.z == 0 ? 1f : dir.z);
            float x = minXFrac;
            float z = Mathf.Sqrt(Mathf.Max(0f, 1f - x * x));
            dir = new Vector3(sx * x, 0f, sz * z);
        }
        return dir;
    }

    private Vector3 GetLinearVelocity() => _rb.linearVelocity;
    private void SetLinearVelocity(Vector3 v) => _rb.linearVelocity = v;
}


// using Fusion;
// using UnityEngine;

// public class BallController : NetworkBehaviour
// {
//     private Rigidbody _rb;

//     public float initialSpeed = 0.5f;

//     private Vector3 lastVelocity;

        
//     public override void Spawned()
//     {
//         _rb = GetComponent<Rigidbody>();

//         if (HasStateAuthority)
//         {
//             // Forzar movimiento inicial con suficiente componente en X
//             float x = Random.Range(0.5f, 1f) * (Random.value > 0.5f ? 1 : -1); // asegúrate que avanza en X
//             float z = Random.Range(-0.5f, 0.5f); // opcional variación vertical

//             Vector3 direction = new Vector3(x, 0, z).normalized;
//             _rb.linearVelocity = direction * initialSpeed;
//         }
//     }


//  public override void FixedUpdateNetwork()
//     {
//         if (!HasStateAuthority || _rb == null) return;

//         lastVelocity = _rb.linearVelocity;

//         // Reaplica velocidad si se detiene (opcional)
//         if (_rb.linearVelocity.magnitude < 0.05f)
//         {
//             Debug.Log("⚠️ Velocidad baja, relanzando pelota");
//             Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
//             _rb.linearVelocity = direction * initialSpeed;
//         }
//     }
//      private void OnCollisionEnter(Collision collision)
//     {
//         if (!HasStateAuthority) return;

//         // Rebote en dirección reflejada
//         // Vector3 normal = collision.contacts[0].normal;
//         // Vector3 reflectedVelocity = Vector3.Reflect(_rb.linearVelocity, normal);
//         // _rb.linearVelocity = reflectedVelocity.normalized * initialSpeed;
//         ContactPoint contact = collision.contacts[0];
//         Vector3 reflected = Vector3.Reflect(lastVelocity.normalized, contact.normal);

//         // En caso de que el ángulo sea demasiado plano, fuerza inclinación mínima
//         if (Mathf.Abs(reflected.z) < 0.2f)
//         {
//             reflected.z = Mathf.Sign(reflected.z) * 0.2f;
//         }

//         _rb.linearVelocity = reflected.normalized * initialSpeed;
    


//     }
// }
