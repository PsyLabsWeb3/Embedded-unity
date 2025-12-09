using UnityEngine;
using UnityEngine.UI;
using FusionExamples.Tanknarok;
using Embedded.Platform;

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

    private void Awake()
    {
        // Si quieres asegurar un orden temprano, podrías mover esta lógica a Awake;
        // aquí solo marcamos inicializado en Start para no interferir con otros sistemas.
    }

    private void Start()
    {
        if (autoDetectOnStart)
        {
            bool isMobile = MobilePlatform.IsMobile();
            InputController.CurrentInputMode = isMobile ? InputMode.MOBILE : InputMode.MOUSE_KEYBOARD;
        }

        _initialized = true;
        UpdateUI();

        if (hideToggleIfMobile && InputController.CurrentInputMode == InputMode.MOBILE)
        {
            // Si quieres ocultar solo el botón, desactiva el GameObject del Button; aquí ocultamos todo este componente.
            gameObject.SetActive(false);
        }
    }

    public void ToggleMode()
    {
        if (!_initialized)
            return;

        if (InputController.CurrentInputMode == InputMode.MOUSE_KEYBOARD)
            InputController.CurrentInputMode = InputMode.MOBILE;
        else
            InputController.CurrentInputMode = InputMode.MOUSE_KEYBOARD;

        UpdateUI();
    }

    private void UpdateUI()
    {
        bool isMobileMode = (InputController.CurrentInputMode == InputMode.MOBILE);

        if (mobileJoystickUI != null)
            mobileJoystickUI.SetActive(isMobileMode);

        if (buttonText != null)
            buttonText.text = isMobileMode ? "Switch to Mouse" : "Switch to Mobile";
    }
}
