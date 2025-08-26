namespace PlatformShooter2D
{
  using UnityEngine;
  using System.Collections;
  using Quantum;

  /// <summary>
  /// Manages the character selection UI, handles player selection events, 
  /// and communicates the selected character to the game.
  /// </summary>
  public class CharacterSelectionUIController : MonoBehaviour
  {
    /// <summary>
    /// UI object containing the character selection buttons.
    /// </summary>
    public GameObject ScreenButtonsUI;

    /// <summary>
    /// UI object that is enabled when the scene starts.
    /// </summary>
    public GameObject EnableOnStart;

    /// <summary>
    /// Animator component for UI animations.
    /// </summary>
    private Animator _animator;
    
    /// <summary>
    /// Reference to the coroutine for hiding the UI.
    /// </summary>
    private Coroutine _hideCoroutine;
    
    /// <summary>
    /// Array of buttons for character selection.
    /// </summary>
    private UnityEngine.UI.Button[] _selectButtons; 

    /// <summary>
    /// Initializes the character selection UI and subscribes to relevant events.
    /// </summary>
    void Start()
    {
      EnableOnStart.SetActive(true);
      _selectButtons = GetComponentsInChildren<UnityEngine.UI.Button>();
      TryGetComponent(out _animator);
      ScreenButtonsUI.SetActive(false);
      QuantumEvent.Subscribe<EventOnPlayerSelectedCharacter>(this, OnPlayerSelected);
    }

    /// <summary>
    /// Handles the event triggered when a player selects a character.
    /// </summary>
    /// <param name="e">Event data containing player selection information.</param>
    private void OnPlayerSelected(EventOnPlayerSelectedCharacter e)
    {
      ScreenButtonsUI.SetActive(true);

      if (_animator)
      {
        if (_hideCoroutine != null)
        {
          StopCoroutine(_hideCoroutine);
        }

        _hideCoroutine = StartCoroutine(HideAnimCoroutine());
        return;
      }
      else
      {
        // Disable the game object if no animator is present.
        gameObject.SetActive(false); 
      }
    }

    /// <summary>
    /// Called when a character selection button is clicked.
    /// </summary>
    /// <param name="characterPrototype">The prototype of the selected character.</param>
    public void OnSelectButtonClicked(AssetRef<EntityPrototype> characterPrototype)
    {
      QuantumRunner runner = QuantumRunner.Default;
      if (runner == null) return;

      // Create player data with the selected character.
      RuntimePlayer playerData = new RuntimePlayer();
      playerData.PlayerAvatar = characterPrototype;

      // Attempt to set the player's nickname from the menu.
      var menu =
        FindAnyObjectByType(typeof(Quantum.Menu.QuantumMenuUIController)) as Quantum.Menu.QuantumMenuUIController;
      if (menu != null)
      {
        playerData.PlayerNickname = menu.ConnectArgs.Username;
      }

      // Add the player to the game.
      runner.Game.AddPlayer(playerData);

      // Disable all selection buttons after a character is selected.
      foreach (var button in _selectButtons)
      {
        button.interactable = false;
      }
    }

    /// <summary>
    /// Coroutine to play the "Hide" animation and disable the game object after it finishes.
    /// </summary>
    private IEnumerator HideAnimCoroutine()
    {
      _animator.Play("Hide");
      yield return null;

      // Wait until the animation completes.
      while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
      {
        yield return null;
      }

      // Disable the game object after the animation.
      gameObject.SetActive(false); 
    }
  }
}