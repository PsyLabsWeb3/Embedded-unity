namespace Quantum
{
  using Photon.Deterministic;
  using UnityEngine.Scripting;

  /// <summary>
  ///   The <c>SkillInventorySystem</c> handles input logic and creation of entities for the Skills.
  /// </summary>
  [Preserve]
  public unsafe class SkillInventorySystem : SystemMainThreadFilter<SkillInventorySystem.Filter>
  {
    /// <summary>
    /// The <c>Filter</c> struct represents the components required for the system's operations,
    /// including an entity reference, Transform2D, PlayerLink, SkillInventory and Status.
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
      /// Pointer to the entity's PlayerLink component.
      /// </summary>
      public PlayerLink* PlayerLink;

      /// <summary>
      /// Pointer to the entity's SkillInventory component.
      /// </summary>
      public SkillInventory* SkillInventory;

      /// <summary>
      /// Pointer to the entity's Status component.
      /// </summary>
      public Status* Status;
    }

    /// <summary>
    /// It is called for each entity that matches the required components.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="filter">The filter containing the entity, defined on the filter declaration.</param>
    public override void Update(Frame frame, ref Filter filter)
    {
      if (filter.Status->IsDead)
      {
        return;
      }

      QuantumDemoInputPlatformer2D input = *frame.GetPlayerInput(filter.PlayerLink->Player);

      if (filter.SkillInventory->CastRateTimer.IsRunning(frame) == false)
      {
        if (input.AltFire.WasPressed)
        {
          CastSkill(frame, ref filter, input.AimDirection);
        }
      }
    }

    /// <summary>
    /// Creates a new Skill Entity in the world and setup it's values
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="filter">The filter containing the entity, defined on the filter declaration.</param>
    /// <param name="direction">The direction to cast the skill entity.</param>
    private void CastSkill(Frame frame, ref Filter filter, FPVector2 direction)
    {
      var skillInventoryData =
        frame.FindAsset(filter.SkillInventory->SkillInventoryData);
      var skillData = frame.FindAsset(skillInventoryData.SkillData);

      //Skill entity creation
      var skillPrototype = frame.FindAsset(skillData.SkillPrototype);
      var skill = frame.Create(skillPrototype);


      //Skill fields setup
      var skillFields = frame.Unsafe.GetPointer<SkillFields>(skill);
      skillFields->SkillData = skillData;
      skillFields->Source = filter.Entity;
      skillFields->TimeToActivate = skillData.ActivationDelay;

      //Skill transform position setup
      var skillTransform = frame.Unsafe.GetPointer<Transform2D>(skill);
      skillTransform->Position = filter.Transform->Position;
      
      //Skill physics velocity setup 
      var skillPhysics = frame.Unsafe.GetPointer<PhysicsBody2D>(skill);
      skillPhysics->Velocity = direction * skillInventoryData.CastForce;
      filter.SkillInventory->CastRateTimer = FrameTimer.FromSeconds(frame, skillInventoryData.CastRate);
      
      frame.Events.OnSkillCasted(skill);
    }
  }
}