using UnityEngine;
using Photon.Deterministic;
using Quantum;

public class MobileInputManager : MonoBehaviour
{
    [Header("Mobile Detection")]
    [SerializeField] private bool _forceMobileMode = false;
    
    [Header("Virtual Controls")]
    [SerializeField] private VirtualJoystick _virtualJoystick;
    [SerializeField] private VirtualButton _powerupButton;
    
    [Header("Mobile UI")]
    [SerializeField] private GameObject _mobileControlsContainer;
    
    private bool _isMobilePlatform;
    
    // Input values
    public FPVector2 Direction { get; private set; }
    public bool Powerup { get; private set; }
    public bool Respawn { get; private set; }
    
    public bool IsMobilePlatform => _isMobilePlatform;
    
    private void Awake()
    {
        // Detect if we're on a mobile platform (including WebGL on mobile)
        _isMobilePlatform = _forceMobileMode || DetectMobilePlatform();
        
        Debug.Log($"MobileInputManager: _forceMobileMode={_forceMobileMode}, Platform={Application.platform}, _isMobilePlatform={_isMobilePlatform}");
        
        // Enable/disable mobile controls based on platform
        if (_mobileControlsContainer != null)
        {
            _mobileControlsContainer.SetActive(_isMobilePlatform);
            Debug.Log($"MobileInputManager: Set mobile controls container active={_isMobilePlatform}");
        }
        else
        {
            Debug.Log("MobileInputManager: _mobileControlsContainer is null");
        }
        
        // Initialize virtual controls if on mobile
        if (_isMobilePlatform)
        {
            InitializeVirtualControls();
        }
    }
    
    private void InitializeVirtualControls()
    {
        // Find virtual controls if not assigned
        if (_virtualJoystick == null)
            _virtualJoystick = FindObjectOfType<VirtualJoystick>();
            
        if (_powerupButton == null)
            _powerupButton = GameObject.Find("PowerupButton")?.GetComponent<VirtualButton>();
    }
    
    private void Update()
    {
        if (_isMobilePlatform)
        {
            UpdateMobileInput();
        }
    }
    
    private void UpdateMobileInput()
    {
        // Get input from virtual controls
        Direction = _virtualJoystick != null ? _virtualJoystick.Direction : FPVector2.Zero;
        
        // PowerUp button serves dual purpose based on game state:
        // - Ready up when race is waiting (maps to Respawn)
        // - Fire weapons during race (maps to Powerup)
        bool powerupPressed = _powerupButton != null && _powerupButton.IsPressed;

        // Determine current game state to decide button behavior
        bool isWaitingState = IsInWaitingState();
        
        if (isWaitingState)
        {
            // During waiting phase: PowerUp button = Ready (Respawn input)
            Respawn = powerupPressed;
            Powerup = false; // Don't fire weapons while waiting
        }
        else
        {
            // During race: PowerUp button = Fire weapons (Powerup input)
            Powerup = powerupPressed;
            Respawn = false; // Don't respawn during race from this button
        }
    }
    
    private unsafe bool IsInWaitingState()
    {
        // Try to get the current race state from Quantum
        // If we can't access it, assume we're not in waiting state (safer default)
        try
        {
            var game = QuantumRunner.Default?.Game;
            if (game?.Frames?.Verified != null)
            {
                var frame = game.Frames.Verified;
                if (frame.Unsafe.TryGetPointerSingleton(out Quantum.Race* race))
                {
                    return race->CurrentRaceState == Quantum.RaceState.Waiting;
                }
            }
        }
        catch
        {
            // If we can't determine race state, default to not waiting
        }
        
        return false;
    }

    /// <summary>
    /// Detects if running on mobile platform, including WebGL on mobile browsers
    /// </summary>
    private bool DetectMobilePlatform()
    {
        // Check traditional mobile platforms first
        if (Application.platform == RuntimePlatform.Android || 
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return true;
        }

        // For WebGL, check if touch is supported (primary indicator)
        if (Application.platform == RuntimePlatform.WebGLPlayer && UnityEngine.Input.touchSupported)
        {
            return true;
        }

        // Additional WebGL mobile detection using device info
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            string deviceModel = SystemInfo.deviceModel.ToLower();
            string deviceName = SystemInfo.deviceName.ToLower();
            
            // Check for mobile device indicators
            if (deviceModel.Contains("iphone") || deviceModel.Contains("ipad") || 
                deviceModel.Contains("android") || deviceModel.Contains("mobile") ||
                deviceName.Contains("mobile") || deviceName.Contains("tablet"))
            {
                return true;
            }

            // Screen size heuristic - mobile devices typically have smaller screens
            if (Screen.width <= 1024 && Screen.height <= 1366)
            {
                return true;
            }
        }

        return false;
    }
    
    // Fallback to desktop input when not on mobile
    public void UpdateDesktopInput()
    {
        var x = UnityEngine.Input.GetAxis("Horizontal");
        var y = UnityEngine.Input.GetAxis("Vertical");
        Direction = new Vector2(x, y).ToFPVector2();
        
        Powerup = UnityEngine.Input.GetButton("Fire1");
        Respawn = UnityEngine.Input.GetKey(KeyCode.R);
    }
    
    // Public method to enable/disable mobile controls (useful for debugging)
    public void SetMobileMode(bool enabled)
    {
        _forceMobileMode = enabled;
        _isMobilePlatform = enabled || DetectMobilePlatform();
        
        Debug.Log($"MobileInputManager: SetMobileMode({enabled}) - _isMobilePlatform now={_isMobilePlatform}");
        
        if (_mobileControlsContainer != null)
        {
            _mobileControlsContainer.SetActive(_isMobilePlatform);
            Debug.Log($"MobileInputManager: Mobile controls container set to active={_isMobilePlatform}");
        }
        else
        {
            Debug.Log("MobileInputManager: Cannot set mobile controls container - it's null");
        }
        
        // Re-initialize virtual controls if enabling mobile mode
        if (_isMobilePlatform)
        {
            InitializeVirtualControls();
        }
    }
}