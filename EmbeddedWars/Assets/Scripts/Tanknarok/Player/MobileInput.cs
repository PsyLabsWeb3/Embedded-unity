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
        Debug.Log($"📲 MOBILE INPUT: moveDelta={MoveInput}, aimDelta={AimInput}");
    }
}


// using Fusion;
// using FusionExamples.Tanknarok;
// using UnityEngine;

// public class MobileInput : MonoBehaviour
// {
//     [SerializeField] private JoystickHandler leftJoystick;
//     [SerializeField] private JoystickHandler rightJoystick;

//     private Vector2 _moveInput;
//     private Vector2 _aimInput;

//     public Vector2 MoveInput => _moveInput;
//     public Vector2 AimInput => _aimInput;

//     private bool _leftDown;
//     private bool _rightDown;

//     private bool _leftReleased;
//     private bool _rightReleased;

//     public bool LeftReleasedThisFrame() => _leftReleased;
//     public bool RightReleasedThisFrame() => _rightReleased;

//     private void Awake()
//     {
//         // Nada que inicializar aquí por ahora
//     }

//     private void LateUpdate()
//     {
//         // Reset flags para el siguiente frame
//         _leftReleased = false;
//         _rightReleased = false;
//     }

//     private void Update()
//     {
//         if (!Application.isMobilePlatform && !Application.isEditor)
//             return;

//         bool leftHandled = false;
//         bool rightHandled = false;

//         for (int i = 0; i < Input.touchCount; i++)
//         {
//             Touch touch = Input.GetTouch(i);
//             Vector2 pos = touch.position;

//             if (pos.x < Screen.width / 2)
//             {
//                 if (!leftHandled)
//                 {
//                     if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
//                         SetLeft(true);
//                     else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
//                         SetLeft(false);

//                     leftHandled = true;
//                 }
//             }
//             else
//             {
//                 if (!rightHandled)
//                 {
//                     if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
//                         SetRight(true);
//                     else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
//                         SetRight(false);

//                     rightHandled = true;
//                 }
//             }
//         }

//         if (!leftHandled)
//             SetLeft(false);

//         if (!rightHandled)
//             SetRight(false);

//         // ⬇️ **Aquí asignas los valores reales**
//         _moveInput = leftJoystick != null ? leftJoystick.Input : Vector2.zero;
//         _aimInput  = rightJoystick != null ? rightJoystick.Input : Vector2.zero;

//         Debug.Log($"📲 MOBILE INPUT: moveDelta={_moveInput}, aimDelta={_aimInput}");
//     }


//     private void SetLeft(bool active)
//     {
//         if (_leftDown && !active)
//             _leftReleased = true;

//         _leftDown = active;

//         leftJoystick.gameObject.SetActive(active);
//         _moveInput = leftJoystick.Input;

//         Debug.Log($"📲 MoveInput: {_moveInput}");
//     }

//     private void SetRight(bool active)
//     {
//         if (_rightDown && !active)
//             _rightReleased = true;

//         _rightDown = active;

//         rightJoystick.gameObject.SetActive(active);
//         _aimInput = rightJoystick.Input;

//         Debug.Log($"🎯 AimInput: {_aimInput}");
//     }
// }
