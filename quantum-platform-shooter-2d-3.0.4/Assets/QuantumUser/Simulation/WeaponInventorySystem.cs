namespace Quantum
{
  using Photon.Deterministic;
  using UnityEngine.Scripting;

  
  /// <summary>
  ///   The <c>WeaponInventorySystem</c> handles changing weapon and current used weapon.
  /// </summary>
  [Preserve]
  public unsafe class WeaponInventorySystem : SystemMainThreadFilter<WeaponInventorySystem.Filter>
  {
    /// <summary>
    /// The <c>Filter</c> struct represents the components required for the system's operations,
    /// including an entity reference, it's PlayerLink, Status, and WeaponInventory.
    /// </summary>
    public struct Filter
    {
      /// <summary>
      /// The reference to the entity being processed.
      /// </summary>
      public EntityRef Entity;
      /// <summary>
      /// Pointer to the entity's PlayerLink component.
      /// </summary>
      public PlayerLink* PlayerLink;
      /// <summary>
      /// Pointer to the entity's Status component.
      /// </summary>
      public Status* Status;
      /// <summary>
      /// Pointer to the entity's WeaponInventory component.
      /// </summary>
      public WeaponInventory* WeaponInventory;
    }

    /// <summary>
    /// It is called for each entity that matches the required components.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="filter">The filter containing the entity, its Transform2D and BulletsFields.</param>
    public override void Update(Frame frame, ref Filter filter)
    {
      if (filter.Status->IsDead) return;

      QuantumDemoInputPlatformer2D input = *frame.GetPlayerInput(filter.PlayerLink->Player);
      if (input.Use.WasPressed)
      {
        ChangeWeapon(frame, filter.Entity, filter.WeaponInventory, filter.PlayerLink);
      }
    }
    /// <summary>
    /// Handles the change weapon procedures.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="character">The entity reference to the character.</param>
    /// <param name="weaponInventory">The WeaponInventory pointer.</param>
    /// <param name="playerLink">The PlayerLink pointer.</param>
    private void ChangeWeapon(Frame frame, EntityRef character, WeaponInventory* weaponInventory, PlayerLink* playerLink)
    {
      weaponInventory->CurrentWeaponIndex = (weaponInventory->CurrentWeaponIndex + 1) % weaponInventory->Weapons.Length;
      var currentWeapon = weaponInventory->Weapons.GetPointer(weaponInventory->CurrentWeaponIndex);
      currentWeapon->ChargeTime = FP._0;

      frame.Events.OnCharacterChangeWeapon(character);
      frame.Events.OnCharacterChangeWeaponLocal(playerLink->Player, currentWeapon->WeaponData);
    }
  }
}