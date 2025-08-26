
namespace Quantum
{
  using Photon.Deterministic;
  using UnityEngine.Scripting;
  
  /// <summary>
  ///   The <c>SkillSystem</c> handles all skill entity interactions: life cycle, activation timers,
  ///   projectile movement and collision checks.
  /// </summary>
  [Preserve]
  public unsafe class SkillSystem : SystemMainThreadFilter<SkillSystem.Filter>
  {
    /// <summary>
    /// The <c>Filter</c> struct represents the components required for the system's operations,
    /// including an entity reference, it's Transform2D and SkillFields.
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
      /// Pointer to the entity's SkillFields component.
      /// </summary>
      public SkillFields* SkillFields;
    }

    /// <summary>
    /// It is called for each entity that matches the required components.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="filter">The filter containing the entity, its Transform2D and BulletsFields.</param>
    public override void Update(Frame frame, ref Filter filter)
    {
      if (filter.SkillFields->TimeToActivate <= FP._0)
      {
        DealAreaDamage(frame, ref filter);
        frame.Destroy(filter.Entity);
      }
      else
      {
        filter.SkillFields->TimeToActivate -= frame.DeltaTime;
      }
    }

    /// <summary>
    /// Check if the bullet is to far from the initial position, performing the bullet actions when needed.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="filter">The filter containing the entity, its Transform2D and SkillFields.</param>
    private static void DealAreaDamage(Frame frame, ref Filter filter)
    {
      var skillData = frame.FindAsset(frame.Get<SkillFields>(filter.Entity).SkillData);
      frame.Events.OnSkillActivated(filter.Transform->Position);

      Physics2D.HitCollection hits =
        frame.Physics2D.OverlapShape(*filter.Transform, skillData.ShapeConfig.CreateShape(frame));
      for (int i = 0; i < hits.Count; i++)
      {
        var targetEntity = hits[i].Entity;
        if (targetEntity == filter.Entity)
        {
          continue;
        }
        

        var skillFields = frame.Get<SkillFields>(filter.Entity);
        
        //Don't hit the caster character!
        if (targetEntity == skillFields.Source)
        {
          continue;
        }

        // Only consider character for damage
        if (targetEntity == EntityRef.None || frame.Has<Status>(targetEntity) == false)
        {
          continue;
        }

        // Only deal damages to character not behind walls 
        var characterPosition = frame.Get<Transform2D>(targetEntity).Position;
        if (LineOfSightHelper.HasLineOfSight(frame, filter.Transform->Position, characterPosition) == false)
        {
          continue;
        }

        frame.Signals.OnCharacterSkillHit(filter.Entity, targetEntity);
        frame.Events.OnSkillHitTarget(filter.Transform->Position, skillFields.SkillData.Id.Value, targetEntity);
      }
    }
  }
}