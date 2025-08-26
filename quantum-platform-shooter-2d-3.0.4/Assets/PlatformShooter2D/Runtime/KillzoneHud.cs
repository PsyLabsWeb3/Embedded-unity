namespace PlatformShooter2D
{
  using Quantum;
  using UnityEngine;

  /// <summary>
  /// Handles displaying kill notifications in the HUD for character deaths.
  /// </summary>
  public class KillzoneHud : MonoBehaviour
  {
    /// <summary>
    /// Prefab used for creating a kill notification entry.
    /// </summary>
    public KillzoneHudEntry HudEntry;

    /// <summary>
    /// Parent layout where kill notification entries will be added.
    /// </summary>
    public RectTransform HudEntryLayout;

    /// <summary>
    /// Subscribes to character death events when the component starts.
    /// </summary>
    void Start()
    {
      QuantumEvent.Subscribe<EventOnCharacterDeath>(this, HandleCharacterDeath);
    }

    /// <summary>
    /// Handles a character death event and creates a new HUD entry to display the event.
    /// </summary>
    /// <param name="deathEvent">The death event data.</param>
    private void HandleCharacterDeath(EventOnCharacterDeath deathEvent)
    {
      // Instantiate a new HUD entry and assign the death event data to it.
      var entry = Instantiate(HudEntry, HudEntryLayout);
      entry.DeathEvent = deathEvent;

      // Set the new entry as the first child in the layout.
      entry.transform.SetAsFirstSibling();
    }
  }
}