namespace PlatformShooter2D
{
  using Quantum;
  using UnityEngine;

  /// <summary>
  /// Handles the behavior of a character selection button, triggering the associated selection logic when clicked.
  /// </summary>
  public class CharacterSelectButtonDelegate : MonoBehaviour
  {
    /// <summary>
    /// The button associated with character selection.
    /// </summary>
    public UnityEngine.UI.Button SelectButton;

    /// <summary>
    /// The prototype of the character that this button selects.
    /// </summary>
    public AssetRef<EntityPrototype> CharacterPrototype;

    /// <summary>
    /// The controller responsible for managing the character selection UI.
    /// </summary>
    public CharacterSelectionUIController CharacterSelectionUIController;

    /// <summary>
    /// Called when the script is initialized. Adds a listener to the button's click event.
    /// </summary>
    private void Start()
    {
      SelectButton.onClick.AddListener(OnCharacterSelected);
    }

    /// <summary>
    /// Called when the selection button is clicked. Notifies the UI controller of the selected character.
    /// </summary>
    public void OnCharacterSelected()
    {
      CharacterSelectionUIController.OnSelectButtonClicked(CharacterPrototype);
    }
  }
}