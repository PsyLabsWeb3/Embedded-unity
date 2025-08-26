namespace PlatformShooter2D
{
  using System.Collections.Generic;
  using Quantum;
  using UnityEngine;

  /// <summary>
  /// Handles the display of floating combat text, such as damage numbers, in the game.
  /// </summary>
  public class FloatingCombatText : QuantumSceneViewComponent<CustomViewContext>
  {
    // ReSharper disable once InvalidXmlDocComment
    [Header("References")] 
    /// <summary>
    /// Reference to the canvas where floating combat text will be displayed.
    /// </summary>
    public RectTransform CanvasTransform;

    /// <summary>
    /// Prefab used to instantiate new floating combat text entries.
    /// </summary>
    public FloatingCombatTextEntry EntryPrefab;

    /// <summary>
    /// Pool of reusable floating combat text entries.
    /// </summary>
    private Queue<FloatingCombatTextEntry> _entryPool = new Queue<FloatingCombatTextEntry>();

    /// <summary>
    /// Number of entries to pre-spawn for the pool.
    /// </summary>
    private readonly int _preSpawnAmount = 16;

    /// <summary>
    /// Initializes the floating combat text system by pre-spawning entries and subscribing to damage events.
    /// </summary>
    private void Start()
    {
      // Pre-spawn entries for reuse.
      SpawnEntries(_preSpawnAmount);
      QuantumEvent.Subscribe<EventOnCharacterTakeDamage>(this, OnCharacterTookDamage);
    }

    /// <summary>
    /// Called when a character takes damage, displays floating text if relevant.
    /// </summary>
    /// <param name="eventData">Data about the damage event.</param>
    private void OnCharacterTookDamage(EventOnCharacterTakeDamage eventData)
    {
      var playerLink = VerifiedFrame.Get<PlayerLink>(eventData.Character);
      var sourcePlayerLink = VerifiedFrame.Get<PlayerLink>(eventData.Source);

      // Skip if neither the target nor source are controlled by the local player.
      if (!Game.PlayerIsLocal(playerLink.Player) &&
          !Game.PlayerIsLocal(sourcePlayerLink.Player))
      {
        return;
      }

      // Spawn more entries if the pool is empty.
      if (_entryPool.Count == 0)
      {
        SpawnEntries(_preSpawnAmount);
      }

      // Dequeue an entry from the pool.
      var entry = _entryPool.Dequeue();

      // Calculate screen position of the damaged character.
      var characterPosition = VerifiedFrame.Get<Transform2D>(eventData.Character).Position;
      var sourcePosition = VerifiedFrame.Get<Transform2D>(eventData.Source).Position;

      Vector3 viewportPos = Camera.main.WorldToViewportPoint(characterPosition.ToUnityVector3());

      // Calculate position on the canvas.
      float width = CanvasTransform.sizeDelta.x;
      float height = CanvasTransform.sizeDelta.y;
      var pos = new Vector3(width * viewportPos.x - width / 2, height * viewportPos.y - height / 2);

      viewportPos.x = Mathf.FloorToInt(pos.x);
      viewportPos.y = Mathf.FloorToInt(pos.y);
      viewportPos.z = 0f;

      // Set the entry's position and activate it.
      entry.RectTransform.anchoredPosition = viewportPos;

      var direction = (characterPosition - sourcePosition).ToUnityVector2();
      entry.Activate(eventData.Damage.AsInt, direction);
    }

    /// <summary>
    /// Spawns a specified number of floating combat text entries into the pool.
    /// </summary>
    /// <param name="amount">The number of entries to spawn.</param>
    private void SpawnEntries(int amount)
    {
      for (int i = 0; i < amount; i++)
      {
        var entry = Instantiate(EntryPrefab, transform);
        entry.OnAnimationFinished += ReturnObjectToPool; // Subscribe to return event.
        entry.Deactivate(); // Deactivate entry initially.
      }
    }

    /// <summary>
    /// Returns a floating combat text entry to the pool when its animation finishes.
    /// </summary>
    /// <param name="entry">The entry to return to the pool.</param>
    private void ReturnObjectToPool(FloatingCombatTextEntry entry)
    {
      _entryPool.Enqueue(entry);
    }
  }
}