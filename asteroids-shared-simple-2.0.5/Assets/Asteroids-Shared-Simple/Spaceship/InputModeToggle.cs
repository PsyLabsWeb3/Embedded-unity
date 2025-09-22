using UnityEngine;
using UnityEngine.UI;
using Asteroids.SharedSimple;
using Asteroids.SharedSimple.Utility;   // Aquí está tu InputController/InputMode

public class InputModeToggle : MonoBehaviour
{
    [Header("UI refs")]
    [SerializeField] private GameObject mobileJoystickUI; // Canvas/raíz del UI móvil
    [SerializeField] private Text buttonText;

    [Header("Behavior")]
    [Tooltip("Auto-detectar móvil al iniciar y fijar el modo de input.")]
    [SerializeField] private bool autoDetectOnStart = true;

    [Tooltip("Ocultar este botón si el modo quedó en MOBILE tras auto-detección.")]
    [SerializeField] private bool hideToggleIfMobile = false;

    private bool _initialized;

    private void Start()
    {
        if (autoDetectOnStart)
        {
            bool isMobile = MobilePlatform.IsMobile();
            LocalInputPoller.CurrentInputMode = isMobile ? InputMode.MOBILE : InputMode.MOUSE_KEYBOARD;
        }

        _initialized = true;
        UpdateUI();

        if (hideToggleIfMobile && LocalInputPoller.CurrentInputMode == InputMode.MOBILE)
        {
            gameObject.SetActive(false);
        }
    }

    public void ToggleMode()
    {
        if (!_initialized) return;

        if (LocalInputPoller.CurrentInputMode == InputMode.MOUSE_KEYBOARD)
            LocalInputPoller.CurrentInputMode = InputMode.MOBILE;
        else
            LocalInputPoller.CurrentInputMode = InputMode.MOUSE_KEYBOARD;

        UpdateUI();
    }

    private void UpdateUI()
    {
        bool isMobileMode = (LocalInputPoller.CurrentInputMode == InputMode.MOBILE);

        if (mobileJoystickUI != null)
            mobileJoystickUI.SetActive(isMobileMode);

        if (buttonText != null)
            buttonText.text = isMobileMode ? "Switch to Mouse" : "Switch to Mobile";
    }
}