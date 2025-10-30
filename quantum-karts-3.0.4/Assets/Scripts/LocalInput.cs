using Photon.Deterministic;
using Quantum;
using UnityEngine;

public class LocalInput : MonoBehaviour
{
    [Header("Mobile Input")]
    [SerializeField] private MobileInputManager _mobileInputManager;
    
    private void Start()
    {
        QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
        
        // Find mobile input manager if not assigned
        if (_mobileInputManager == null)
        {
            _mobileInputManager = FindObjectOfType<MobileInputManager>();
        }
    }

    public void PollInput(CallbackPollInput callback)
    {
        Quantum.Input input = new Quantum.Input();

        // Check if we should use mobile input or desktop input
        if (_mobileInputManager != null && _mobileInputManager.IsMobilePlatform)
        {
            // Use mobile input
            input.Direction = _mobileInputManager.Direction;
            input.Powerup = _mobileInputManager.Powerup;
            input.Respawn = _mobileInputManager.Respawn;
        }
        else
        {
            // Use desktop input (original behavior)
            if (_mobileInputManager != null)
            {
                // Update desktop input through mobile manager for consistency
                _mobileInputManager.UpdateDesktopInput();
                input.Direction = _mobileInputManager.Direction;
                input.Powerup = _mobileInputManager.Powerup;
                input.Respawn = _mobileInputManager.Respawn;
            }
            else
            {
                // Fallback to direct input if mobile manager is missing
                input.Drift = UnityEngine.Input.GetButton("Jump");
                input.Powerup = UnityEngine.Input.GetButton("Fire1");
                input.Respawn = UnityEngine.Input.GetKey(KeyCode.R);

                var x = UnityEngine.Input.GetAxis("Horizontal");
                var y = UnityEngine.Input.GetAxis("Vertical");
                input.Direction = new Vector2(x, y).ToFPVector2();
            }
        }

        callback.SetInput(input, DeterministicInputFlags.Repeatable);
    }
}
