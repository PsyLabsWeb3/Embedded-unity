using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Helper script to create mobile control UI elements programmatically
/// This script can be attached to a GameObject and run in the editor or at runtime
/// to generate the mobile controls UI
/// </summary>
public class MobileControlsCreator : MonoBehaviour
{
    [Header("Canvas Settings")]
    [SerializeField] private Canvas _targetCanvas;
    [SerializeField] private bool _createOnStart = false;
    [SerializeField] private bool _autoCreateOnMobile = true;
    [SerializeField] private bool _enableTestingOnPC = false;
    
    [Header("Joystick Settings")]
    [SerializeField] private Vector2 _joystickPosition = new Vector2(300, 250);
    [SerializeField] private float _joystickSize = 500f;
    
    [Header("Button Settings")]
    [SerializeField] private Vector2 _powerupButtonPosition = new Vector2(-250, 250);
    [SerializeField] private float _buttonSize = 225f;
    
    [Header("Colors")]
    [SerializeField] private Color _joystickBackgroundColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private Color _joystickHandleColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private Color _buttonColor = new Color(0.2f, 0.6f, 1f, 0.8f);
    
    private void Start()
    {
        bool shouldCreate = _createOnStart;
        
        // Auto-create on mobile platforms if enabled
        if (_autoCreateOnMobile && !shouldCreate)
        {
            shouldCreate = IsMobilePlatform();
        }
        
        // Enable testing on PC if specified
        if (_enableTestingOnPC && !shouldCreate)
        {
            shouldCreate = true; // Create on any platform when testing is enabled
        }

        if (shouldCreate)
        {
            CreateMobileControls();
        }
        else
        {
            Debug.Log($"Mobile controls NOT created. shouldCreate: {shouldCreate}, _createOnStart: {_createOnStart}, _autoCreateOnMobile: {_autoCreateOnMobile}, _enableTestingOnPC: {_enableTestingOnPC}, Platform: {Application.platform}");
        }
    }    [ContextMenu("Create Mobile Controls")]
    public void CreateMobileControls()
    {
        if (_targetCanvas == null)
        {
            Debug.LogError("Target Canvas is not assigned!");
            return;
        }
        
        // Check if mobile controls already exist
        if (GameObject.Find("MobileControlsContainer") != null)
        {
            Debug.LogWarning("Mobile controls already exist! Skipping creation.");
            return;
        }
        
        // Create main container
        GameObject mobileControlsContainer = CreateMobileControlsContainer();
        
        // Create virtual joystick
        CreateVirtualJoystick(mobileControlsContainer);
        
        // Create action buttons
        CreatePowerupButton(mobileControlsContainer);
        
        // Add MobileInputManager if it doesn't exist
        MobileInputManager mobileInputManager = FindObjectOfType<MobileInputManager>();
        if (mobileInputManager == null)
        {
            GameObject managerObject = new GameObject("MobileInputManager");
            mobileInputManager = managerObject.AddComponent<MobileInputManager>();
        }
        
        // Enable mobile mode for testing if we're on PC and testing is enabled
        if (_enableTestingOnPC && (Application.platform == RuntimePlatform.WindowsEditor ||
                                   Application.platform == RuntimePlatform.WindowsPlayer ||
                                   Application.platform == RuntimePlatform.OSXEditor ||
                                   Application.platform == RuntimePlatform.OSXPlayer ||
                                   Application.platform == RuntimePlatform.LinuxEditor ||
                                   Application.platform == RuntimePlatform.LinuxPlayer))
        {
            mobileInputManager.SetMobileMode(true);
            Debug.Log("Mobile controls testing enabled on PC!");
        }
        
        // Connect the container to the mobile input manager
        var containerField = typeof(MobileInputManager).GetField("_mobileControlsContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (containerField != null)
        {
            containerField.SetValue(mobileInputManager, mobileControlsContainer);
        }
        
        Debug.Log("Mobile controls created successfully!");
    }
    
    [ContextMenu("Force Enable Mobile Testing")]
    public void ForceEnableMobileTesting()
    {
        MobileInputManager mobileInputManager = FindObjectOfType<MobileInputManager>();
        if (mobileInputManager != null)
        {
            mobileInputManager.SetMobileMode(true);
            Debug.Log("Mobile testing mode ENABLED! Virtual controls should respond to mouse input.");
        }
        else
        {
            Debug.LogError("MobileInputManager not found! Please create mobile controls first.");
        }
    }
    
    [ContextMenu("Disable Mobile Testing")]
    public void DisableMobileTesting()
    {
        MobileInputManager mobileInputManager = FindObjectOfType<MobileInputManager>();
        if (mobileInputManager != null)
        {
            mobileInputManager.SetMobileMode(false);
            Debug.Log("Mobile testing mode DISABLED.");
        }
    }
    
    [ContextMenu("Debug Mobile Controls Status")]
    public void DebugMobileControlsStatus()
    {
        Debug.Log("=== MOBILE CONTROLS DEBUG ===");
        
        // Check MobileInputManager
        MobileInputManager mobileInputManager = FindObjectOfType<MobileInputManager>();
        if (mobileInputManager != null)
        {
            Debug.Log($"MobileInputManager found: IsMobilePlatform={mobileInputManager.IsMobilePlatform}");
        }
        else
        {
            Debug.Log("MobileInputManager NOT found!");
        }
        
        // Check if mobile controls container exists
        GameObject container = GameObject.Find("MobileControlsContainer");
        if (container != null)
        {
            Debug.Log($"MobileControlsContainer found: Active={container.activeSelf}");
        }
        else
        {
            Debug.Log("MobileControlsContainer NOT found!");
        }
        
        // Check if powerup button exists
        GameObject powerupButton = GameObject.Find("PowerupButton");
        if (powerupButton != null)
        {
            Text buttonText = powerupButton.GetComponentInChildren<Text>();
            VirtualButton virtualButton = powerupButton.GetComponent<VirtualButton>();
            Debug.Log($"PowerupButton found: Active={powerupButton.activeSelf}, Text='{(buttonText != null ? buttonText.text : "NO TEXT")}', VirtualButton={virtualButton != null}");
        }
        else
        {
            Debug.Log("PowerupButton NOT found!");
        }
        
        Debug.Log("========================");
    }
    
    /// <summary>
    /// Detects if the current platform is mobile, including WebGL on mobile devices
    /// </summary>
    private bool IsMobilePlatform()
    {
        // Check native mobile platforms
        if (Application.platform == RuntimePlatform.Android || 
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return true;
        }
        
        // For WebGL builds, detect mobile devices using SystemInfo
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            // Check if device has touch support (most reliable for WebGL)
            if (Input.touchSupported)
            {
                return true;
            }
            
            // Additional checks for WebGL mobile detection
            string deviceModel = SystemInfo.deviceModel.ToLower();
            string deviceName = SystemInfo.deviceName.ToLower();
            
            // Common mobile device indicators
            bool isMobileDevice = deviceModel.Contains("iphone") || 
                                 deviceModel.Contains("ipad") || 
                                 deviceModel.Contains("android") ||
                                 deviceName.Contains("mobile") ||
                                 deviceName.Contains("tablet");
            
            // Check screen size as additional indicator (mobile devices typically have smaller screens)
            bool isMobileScreen = Screen.width <= 1024 || Screen.height <= 1024;
            
            return isMobileDevice || isMobileScreen;
        }
        
        return false;
    }
    
    private GameObject CreateMobileControlsContainer()
    {
        GameObject container = new GameObject("MobileControlsContainer");
        container.transform.SetParent(_targetCanvas.transform, false);
        
        RectTransform rectTransform = container.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
        // Ensure the canvas has necessary components for UI interaction
        if (_targetCanvas.GetComponent<GraphicRaycaster>() == null)
        {
            _targetCanvas.gameObject.AddComponent<GraphicRaycaster>();
            Debug.Log("Added GraphicRaycaster to canvas for UI interaction.");
        }
        
        // Ensure there's an EventSystem in the scene
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
            Debug.Log("Created EventSystem for UI interaction.");
        }
        
        return container;
    }
    
    private void CreateVirtualJoystick(GameObject parent)
    {
        // Create joystick container
        GameObject joystickContainer = new GameObject("VirtualJoystick");
        joystickContainer.transform.SetParent(parent.transform, false);
        
        RectTransform containerRect = joystickContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(0, 0);
        containerRect.anchoredPosition = _joystickPosition;
        containerRect.sizeDelta = new Vector2(_joystickSize, _joystickSize);
        
        // Add VirtualJoystick component
        VirtualJoystick virtualJoystick = joystickContainer.AddComponent<VirtualJoystick>();
        
        // Create background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(joystickContainer.transform, false);
        
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = _joystickBackgroundColor;
        bgImage.sprite = CreateCircleSprite();
        
        // Create handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(background.transform, false);
        
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0.5f, 0.5f);
        handleRect.anchorMax = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(_joystickSize * 0.4f, _joystickSize * 0.4f);
        handleRect.anchoredPosition = Vector2.zero;
        
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = _joystickHandleColor;
        handleImage.sprite = CreateCircleSprite();
        
        // Set references in VirtualJoystick
        var backgroundField = typeof(VirtualJoystick).GetField("_background", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var handleField = typeof(VirtualJoystick).GetField("_handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var snapToCenterField = typeof(VirtualJoystick).GetField("_snapToCenter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var maxDistanceField = typeof(VirtualJoystick).GetField("_maxDistance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var deadZoneField = typeof(VirtualJoystick).GetField("_deadZone", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (backgroundField != null) backgroundField.SetValue(virtualJoystick, bgRect);
        if (handleField != null) handleField.SetValue(virtualJoystick, handleRect);
        if (snapToCenterField != null) snapToCenterField.SetValue(virtualJoystick, false); // Disable snap to center
        if (maxDistanceField != null) maxDistanceField.SetValue(virtualJoystick, _joystickSize * 0.4f); // Set proper max distance
        if (deadZoneField != null) deadZoneField.SetValue(virtualJoystick, 0.1f); // Small dead zone
        
        Debug.Log($"Created VirtualJoystick with maxDistance: {_joystickSize * 0.4f}, snapToCenter: false");
    }
    
    private void CreatePowerupButton(GameObject parent)
    {
        CreateButton(parent, "PowerupButton", "FIRE", _powerupButtonPosition);
    }
    
    private void CreateButton(GameObject parent, string name, string text, Vector2 position)
    {
        GameObject button = new GameObject(name);
        button.transform.SetParent(parent.transform, false);
        
        RectTransform buttonRect = button.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1, 0);
        buttonRect.anchorMax = new Vector2(1, 0);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(_buttonSize, _buttonSize);
        
        Image buttonImage = button.AddComponent<Image>();
        buttonImage.color = _buttonColor;
        buttonImage.sprite = CreateCircleSprite();
        
        VirtualButton virtualButton = button.AddComponent<VirtualButton>();
        
        // Create button text
        GameObject buttonText = new GameObject("Text");
        buttonText.transform.SetParent(button.transform, false);
        
        RectTransform textRect = buttonText.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        Text textComponent = buttonText.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComponent.fontSize = 24; // Increased font size
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.color = Color.white;
        textComponent.fontStyle = FontStyle.Bold; // Make it bold
        
        Debug.Log($"Created button '{name}' with text '{text}' at position {position}");
    }

    private Sprite CreateCircleSprite()
    {
        // Try to find existing UI sprite, or create a simple white texture
        Sprite[] sprites = Resources.FindObjectsOfTypeAll<Sprite>();
        foreach (Sprite sprite in sprites)
        {
            if (sprite.name == "Knob" || sprite.name == "UISprite" || sprite.name == "Background")
            {
                return sprite;
            }
        }

        // Fallback to default UI sprite
        return Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
    }
}