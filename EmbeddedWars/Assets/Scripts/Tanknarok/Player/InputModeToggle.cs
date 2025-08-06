using UnityEngine;
using UnityEngine.UI;
using FusionExamples.Tanknarok;

public class InputModeToggle : MonoBehaviour
{
    [SerializeField] private GameObject mobileJoystickUI; // Referencia al MobileInput
    [SerializeField] private Text buttonText;

    private void Start()
    {
        UpdateUI();
    }

    public void ToggleMode()
    {
        if (InputController.CurrentInputMode == InputMode.MOUSE_KEYBOARD)
            InputController.CurrentInputMode = InputMode.MOBILE;
        else
            InputController.CurrentInputMode = InputMode.MOUSE_KEYBOARD;

        UpdateUI();
    }

    private void UpdateUI()
    {
        bool isMobile = InputController.CurrentInputMode == InputMode.MOBILE;
        if (mobileJoystickUI != null)
            mobileJoystickUI.SetActive(isMobile);

        if (buttonText != null)
            buttonText.text = isMobile ? "Switch to Mouse" : "Switch to Mobile";
    }
}
