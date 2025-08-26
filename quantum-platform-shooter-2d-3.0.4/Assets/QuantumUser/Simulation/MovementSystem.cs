namespace Quantum
{
  using Photon.Deterministic;
  using UnityEngine.Scripting;
  
  /// <summary>
  ///   The <c>MovementSystem</c> Handles movement and input for players managing:
  ///   position, jump, double jump, movement events and  KCC2D integration.
  /// </summary>
  [Preserve]
  public unsafe class MovementSystem : SystemMainThreadFilter<MovementSystem.Filter>
  {
    /// <summary>
    /// The <c>Filter</c> struct represents the components required for the system's operations,
    /// including an entity reference, its Transform2D, PlayerLink, Status and KCC2D.
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
      /// Pointer to the entity's Status component.
      /// </summary>
      public Status* Status;
      /// <summary>
      /// Pointer to the entity's MovementData component.
      /// </summary>
      public MovementData* MovementData;
      /// <summary>
      /// Pointer to the entity's KCC2D component.
      /// </summary>
      public KCC2D* KCC;
    }

    /// <summary>
    /// It is called for each entity that matches the required components.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="filter">The filter containing the entity, Transform2D, PlayerLink, Status, MovementData and KCC2D.</param>
    public override void Update(Frame frame, ref Filter filter)
    {
      if (filter.Status->IsDead == true)
      {
        return;
      }

      QuantumDemoInputPlatformer2D input = *frame.GetPlayerInput(filter.PlayerLink->Player);
      var config = frame.FindAsset(filter.KCC->Config);
      filter.KCC->Input = input;
      config.Move(frame, filter.Entity, filter.Transform, filter.KCC);
      UpdateIsFacingRight(frame, ref filter, input);
    }

    /// <summary>
    /// Changes the facing direction data on MovementData component.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="filter">The filter containing the entity, Transform2D, PlayerLink, Status, MovementData and KCC2D.</param>
    /// <param name="input">The current player input.</param>
    private void UpdateIsFacingRight(Frame frame, ref Filter filter, QuantumDemoInputPlatformer2D input)
    {
      filter.MovementData->IsFacingRight = input.AimDirection.X > FP._0;
    }
  }
}