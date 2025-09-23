using UnityEngine;

public class MobileInput : MonoBehaviour
{
    [SerializeField] private JoystickHandler leftJoystick;
    [SerializeField] private JoystickHandler rightJoystick;
// Lecturas públicas (las usará tu poller)
    public Vector2 MoveInput  => leftJoystick  ? leftJoystick.Input  : Vector2.zero; // Izquierdo: Y = thrust, X = rotación
    public Vector2 AimInput   => rightJoystick ? rightJoystick.Input : Vector2.zero; // Derecho: disparo por mover
    public bool FireHeld { get; private set; } // true si se mantiene disparo

  

    void Awake()
    {
        if (fireButton)
        {
            fireButton.onClick.AddListener(() => { FireHeld = true; Invoke(nameof(ReleaseFire), 0.05f); });
        }
    }
    // public void OnSecondaryFireButtonDown() => _secondaryFirePressed = true;
    // public void OnSecondaryFireButtonUp() => _secondaryFirePressed = false;
    // public bool SecondaryFirePressed() => _secondaryFirePressed;

    private void Update()
    {
        if (!fireButton)
        {
            FireHeld = RightStick.sqrMagnitude > 0.2f * 0.2f;
        }
        // Solo para debug
        Debug.Log($"📲 MOBILE INPUT: moveDelta={MoveInput}, aimDelta={AimInput}");
    }
      void ReleaseFire() => FireHeld = false;
}

