
namespace Quantum
{
  using Photon.Deterministic;
  using UnityEngine.Scripting;
  using Collections;

  /// <summary>
  ///   The <c>RespawnSystem</c> Handles respawn logic, timer and actually respawning.
  /// </summary>
  [Preserve]
  public unsafe class RespawnSystem : SystemMainThread, ISignalOnCharacterRespawn,
    ISignalOnComponentAdded<SpawnIdentifier>
  {
    /// <summary>
    /// Callback called when a new SpawnIdentifier is created.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="entity">The spawn entity.</param>
    /// <param name="component">The SpawnIdentifier component.</param>
    public void OnAdded(Frame frame, EntityRef entity, SpawnIdentifier* component)
    {
      var spawnPlaces = frame.Unsafe.GetPointerSingleton<SpawnPlaces>();
      if (frame.TryResolveList(spawnPlaces->Spawners, out var spawns) == false)
      {
        spawns = InitSpawns(frame);
      }
      spawns.Add(entity);
    }

    /// <summary>
    /// SpawnPlaces list allocation.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    private QList<EntityRef> InitSpawns(Frame frame)
    {
      var spawnPlaces = frame.Unsafe.GetPointerSingleton<SpawnPlaces>();
      frame.AllocateList(out spawnPlaces->Spawners);
      return frame.ResolveList(spawnPlaces->Spawners);
    }


    /// <summary>
    /// Regular Quantum Update called every tick
    /// </summary>
    /// <param name="frame">The game frame.</param>
    public override void Update(Frame frame)
    {
      foreach (var (character, characterStatus) in frame.Unsafe.GetComponentBlockIterator<Status>())
      {
        if (characterStatus->IsDead)
        {
          if (characterStatus->RespawnTimer.IsRunning(frame) == false)
          {
            frame.Signals.OnCharacterRespawn(character);
          }
        }
      }
    }

    /// <summary>
    /// Signal triggered for spawning character.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    public void OnCharacterRespawn(Frame frame, EntityRef character)
    {
      var position = FPVector2.One;

      var spawnPlaces = frame.Unsafe.GetPointerSingleton<SpawnPlaces>();
      var spawns = frame.ResolveList(spawnPlaces->Spawners);

      if (spawns.Count != 0)
      {
        int index = frame.RNG->Next(0, spawns.Count);
        position = frame.Get<Transform2D>(spawns[index]).Position;
      }

      var characterTransform = frame.Unsafe.GetPointer<Transform2D>(character);
      var collider = frame.Unsafe.GetPointer<PhysicsCollider2D>(character);

      characterTransform->Position = position;
      collider->IsTrigger = false;

      frame.Events.OnCharacterRespawn(character);
    }
  }
}