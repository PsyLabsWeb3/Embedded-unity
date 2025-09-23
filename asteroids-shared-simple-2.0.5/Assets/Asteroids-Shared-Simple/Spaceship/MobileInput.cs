using UnityEngine;
using UnityEngine.UI;

public class MobileInput : MonoBehaviour
{
    [SerializeField] private JoystickHandler leftJoystick;
    [SerializeField] private JoystickHandler rightJoystick;
    // Lecturas públicas (las usará tu poller)
    public Vector2 MoveInput => leftJoystick ? leftJoystick.Input : Vector2.zero; // Izquierdo: Y = thrust, X = rotación
    public Vector2 AimInput => rightJoystick ? rightJoystick.Input : Vector2.zero; // Derecho: disparo por mover
    public bool FireHeld { get; private set; } // true si se mantiene disparo

    [Header("Botón de disparo (opcional)")]
    [SerializeField] private Button fireButton;

    public string GetLeftName() => leftJoystick ? $"{leftJoystick.name}#{leftJoystick.GetInstanceID()}" : "NULL";
    public string GetRightName() => rightJoystick ? $"{rightJoystick.name}#{rightJoystick.GetInstanceID()}" : "NULL";

    private void Awake()
    {
        var poller = FindFirstObjectByType<Asteroids.SharedSimple.LocalInputPoller>(FindObjectsInactive.Include);
        if (poller) poller.SetMobileInput(this);
        // Si hay botón, un “tap” marca FireHeld por un instante
        if (fireButton)
        {
            fireButton.onClick.AddListener(() =>
            {
                FireHeld = true;
                Invoke(nameof(ReleaseFire), 0.05f);
            });
        }
    }

    private void Update()
    {
        Debug.Log($"[MobileInput:{name}#{GetInstanceID()}] move={MoveInput} aim={AimInput} left={GetLeftName()} right={GetRightName()} active={gameObject.activeInHierarchy}");
        // Si NO hay botón, considera “held” si mueves el joystick derecho
        if (!fireButton)
        {
            var r = AimInput;
            FireHeld = r.sqrMagnitude > 0.2f * 0.2f; // umbral
        }

        // Debug opcional
        // Debug.Log($"📲 MOBILE INPUT: move={MoveInput}, aim={AimInput}, fireHeld={FireHeld}");
    }

    private void ReleaseFire() => FireHeld = false;
}
