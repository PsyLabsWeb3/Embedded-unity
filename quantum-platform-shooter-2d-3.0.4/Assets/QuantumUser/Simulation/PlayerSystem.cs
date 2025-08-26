namespace Quantum
{
  using UnityEngine.Scripting;

  /// <summary>
  ///   The <c>PlayerSystem</c> handles the player joining operation, such as character creation.
  /// </summary>
  [Preserve]
  public unsafe class PlayerSystem : SystemSignalsOnly, ISignalOnPlayerAdded
  {
    /// <summary>
    /// Callback triggered when the player is successfully added to the simulation.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="player">The PlayerRef.</param>
    /// <param name="firstTime">Indicates that is not a late join.</param>
    public void OnPlayerAdded(Frame frame, PlayerRef player, bool firstTime)
    {
      var data = frame.GetPlayerData(player);

      if (data.PlayerAvatar != null)
      {
        SetPlayerCharacter(frame, player, data.PlayerAvatar);
      }
      else
      {
        Log.Warn(
          "Character prototype is null on RuntimePlayer, check QuantumMenuConnectionBehaviourSDK to prevent adding player automatically!");
      }
    }

    /// <summary>
    /// Creates and prepare the character, calls OnCharacterRespawn signal.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="player">The PlayerRef.</param>
    /// <param name="prototypeAsset">Reference to the entity prototype to be created.</param>
    private void SetPlayerCharacter(Frame frame, PlayerRef player, AssetRef<EntityPrototype> prototypeAsset)
    {
      var characterEntity = frame.Create(prototypeAsset);

      var playerLink = frame.Unsafe.GetPointer<PlayerLink>(characterEntity);
      playerLink->Player = player;

      frame.Signals.OnCharacterRespawn(characterEntity);

      frame.Events.OnCharacterCreated(characterEntity);
      frame.Events.OnPlayerSelectedCharacter(player);
    }
  }
}