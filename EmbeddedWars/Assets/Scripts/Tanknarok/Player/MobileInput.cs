using UnityEngine;

public class MobileInput : MonoBehaviour
{
    [SerializeField] private JoystickHandler leftJoystick;
    [SerializeField] private JoystickHandler rightJoystick;

    public Vector2 MoveInput => leftJoystick != null ? leftJoystick.Input : Vector2.zero;
    public Vector2 AimInput  => rightJoystick != null ? rightJoystick.Input : Vector2.zero;

    private bool _secondaryFirePressed;
    public void OnSecondaryFireButtonDown() => _secondaryFirePressed = true;
    public void OnSecondaryFireButtonUp() => _secondaryFirePressed = false;
    public bool SecondaryFirePressed() => _secondaryFirePressed;

    private void Update()
    {
        // Solo para debug
        // Debug.Log($"📲 MOBILE INPUT: moveDelta={MoveInput}, aimDelta={AimInput}");
    }
}


