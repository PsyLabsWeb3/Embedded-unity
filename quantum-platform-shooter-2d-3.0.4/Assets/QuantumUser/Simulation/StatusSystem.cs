namespace Quantum
{
  using Photon.Deterministic;
  using UnityEngine.Scripting;
  
  /// <summary>
  /// The <c>StatusSystem</c> manages health and status effects such as invincibility and death
  /// </summary>
  [Preserve]
  public unsafe class StatusSystem : SystemMainThreadFilter<StatusSystem.Filter>, ISignalOnCharacterRespawn,
    ISignalOnCharacterHit, ISignalOnCharacterSkillHit
  {
    /// <summary>
    /// The <c>Filter</c> struct represents the components required for the system's operations,
    /// including an entity reference, it's Transform2D and Status.
    /// </summary>
    public struct Filter
    {
      /// <summary>
      /// The reference to the entity being processed.
      /// </summary>
      public EntityRef Entity;
      /// <summary>
      /// Pointer to the entity's Transform2D component.
      /// </summary>
      public Transform2D* Transform;
      /// <summary>
      /// Pointer to the entity's Status component.
      /// </summary>
      public Status* Status;
    }
    
    /// <summary>
    /// It is called for each entity that matches the required components.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="filter">The filter containing the entity, its Transform2D and Status.</param>
    public override void Update(Frame frame, ref Filter filter)
    {
      UpdateCharacterRegeneration(frame, ref filter);
    }

    public void UpdateCharacterRegeneration(Frame frame, ref Filter filter)
    {
      var status = filter.Status;
      var statusData = frame.FindAsset(status->StatusData);
      
      if (status->RegenTimer.IsRunning(frame) == false)
      {
        status->CurrentHealth += frame.DeltaTime * statusData.RegenRate;
        status->CurrentHealth = FPMath.Clamp(status->CurrentHealth, status->CurrentHealth,
          statusData.MaxHealth);
      }
    }

    /// <summary>
    /// Triggered by OnCharacterHit signal when character gets hit by bullets
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="bulletEntity">The bullet entity reference.</param>
    /// <param name="characterEntity">The character entity reference.</param>
    /// <param name="damage">Damage amount to be applied.</param>
    public void OnCharacterHit(Frame frame, EntityRef bulletEntity, EntityRef characterEntity, FP damage)
    {
      var bulletSourceEntity = frame.Get<BulletFields>(bulletEntity).Source;
      ApplyDamage(frame, bulletSourceEntity, characterEntity, damage);
    }

    /// <summary>
    /// Triggered by OnCharacterRespawn signal, restores character status.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="character">The character entity reference.</param>
    public void OnCharacterRespawn(Frame frame, EntityRef character)
    {
      var status = frame.Unsafe.GetPointer<Status>(character);
      var statusData = frame.FindAsset(status->StatusData);

      status->IsDead = false;
      status->CurrentHealth = statusData.MaxHealth;
      status->InvincibleTimer = FrameTimer.FromSeconds(frame, statusData.InvincibleTime);
    }

    /// <summary>
    /// Triggered by OnCharacterSkillHit signal, apply skill damage to the character.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="character">The character entity reference.</param>
    public void OnCharacterSkillHit(Frame frame, EntityRef skillEntity, EntityRef characterEntity)
    {
      var skillFields = frame.Get<SkillFields>(skillEntity);
      var skillData = frame.FindAsset(skillFields.SkillData);
      ApplyDamage(frame, skillFields.Source, characterEntity, skillData.Damage);
    }

    /// <summary>
    /// Apply damage to the character and activates destruction when necessary.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="sourceCharacter">The character entity reference that causes the damage.</param>
    /// <param name="targetCharacter">The character entity reference that receives the damage.</param>
    private static void ApplyDamage(Frame frame, EntityRef sourceCharacter, EntityRef targetCharacter, FP damage)
    {
      var characterStatus = frame.Unsafe.GetPointer<Status>(targetCharacter);
      var statusData = frame.FindAsset<StatusData>(characterStatus->StatusData.Id);

      if (characterStatus->InvincibleTimer.IsRunning(frame) || damage < statusData.MinimumDamage)
      {
        return;
      }

      characterStatus->RegenTimer = FrameTimer.FromSeconds(frame, statusData.TimeUntilRegen);
      characterStatus->CurrentHealth -= damage;
      frame.Events.OnCharacterTakeDamage(targetCharacter, damage, sourceCharacter);
      frame.Events.OnCharacterBlink(targetCharacter);

      if (characterStatus->CurrentHealth <= 0)
      {
        DestroyCharacter(frame, sourceCharacter, targetCharacter, characterStatus);
      }
    }
    
    /// <summary>
    /// Destroy the character entity.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="sourceCharacter">The character entity reference that causes the damage.</param>
    /// <param name="targetCharacter">The character entity reference that receives the damage.</param>
    /// <param name="status">The target character status component.</param>
    private static void DestroyCharacter(Frame frame, EntityRef sourceCharacter, EntityRef targetCharacter, Status* status)
    {
      var characterController = frame.Unsafe.GetPointer<KCC2D>(targetCharacter);
      var collider = frame.Unsafe.GetPointer<PhysicsCollider2D>(targetCharacter);
      var statusData = frame.FindAsset<StatusData>(status->StatusData.Id);

      status->CurrentHealth = FP._0;
      status->IsDead = true;
      status->RespawnTimer = FrameTimer.FromSeconds(frame, statusData.RespawnTime);
      characterController->KinematicHorizontalSpeed = FP._0;
      collider->IsTrigger = true;

      frame.Signals.OnCharacterDeath(targetCharacter, sourceCharacter);
      frame.Events.OnCharacterDeath(targetCharacter, sourceCharacter);
    }
  }
}