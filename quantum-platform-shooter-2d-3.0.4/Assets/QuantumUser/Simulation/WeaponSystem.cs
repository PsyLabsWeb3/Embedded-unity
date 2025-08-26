
namespace Quantum
{
  using Photon.Deterministic;
  using UnityEngine.Scripting;

  /// <summary>
  ///   The <c>WeaponSystem</c> handles all procedures weapon related: weapon ammo recharge and firing bullets
  /// </summary>
  [Preserve]
  public unsafe class WeaponSystem : SystemMainThreadFilter<WeaponSystem.Filter>, ISignalOnCharacterRespawn
  {
    /// <summary>
    /// The <c>Filter</c> struct represents the components required for the system's operations,
    /// including an entity reference, it's PlayerLink, Status and WeaponInventory.
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
    /// <param name="filter">The filter containing the entity reference, it's PlayerLink, Status and WeaponInventory.</param>
    public override void Update(Frame frame, ref Filter filter)
    {
      if (filter.Status->IsDead) return;

      UpdateWeaponRecharge(frame, ref filter);
      UpdateWeaponFire(frame, ref filter);
    }

    /// <summary>
    /// Handles the weapon increase in ammunition amount.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="filter">The filter containing the entity reference, it's PlayerLink, Status and WeaponInventory.</param>
    private void UpdateWeaponRecharge(Frame frame, ref Filter filter)
    {
      var currentWeaponIndex = filter.WeaponInventory->CurrentWeaponIndex;
      var currentWeapon = filter.WeaponInventory->Weapons.GetPointer(currentWeaponIndex);

      var weaponData = frame.FindAsset(currentWeapon->WeaponData);
      if (currentWeapon->DelayToStartRechargeTimer.IsRunning(frame) == false
          && currentWeapon->RechargeRate.IsRunning(frame) == false
          && currentWeapon->CurrentAmmo < weaponData.MaxAmmo)
      {
        currentWeapon->RechargeRate = FrameTimer.FromSeconds(frame, weaponData.RechargeTimer / (FP)weaponData.MaxAmmo);
        currentWeapon->CurrentAmmo++;

        if (currentWeapon->CurrentAmmo == weaponData.MaxAmmo)
        {
          currentWeapon->IsRecharging = false;
        }
      }
    }

    /// <summary>
    /// Handles the weapon fire action and decreases the ammo amount.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="filter">The filter containing the entity reference, it's PlayerLink, Status and WeaponInventory.</param>
    private void UpdateWeaponFire(Frame frame, ref Filter filter)
    {
      var currentWeaponIndex = filter.WeaponInventory->CurrentWeaponIndex;
      var currentWeapon = filter.WeaponInventory->Weapons.GetPointer(currentWeaponIndex);
      var weaponData = frame.FindAsset(currentWeapon->WeaponData);

      QuantumDemoInputPlatformer2D input = *frame.GetPlayerInput(filter.PlayerLink->Player);
      if (input.Fire)
      {
        // Checks if the weapon is ready to fire
        if (currentWeapon->FireRateTimer.IsRunning(frame) == false 
            && !currentWeapon->IsRecharging 
            && currentWeapon->CurrentAmmo > 0)
        {
          SpawnBullet(frame, filter.Entity, currentWeapon, input.AimDirection);
          currentWeapon->FireRateTimer = FrameTimer.FromSeconds(frame, FP._1 / weaponData.FireRate);
          currentWeapon->ChargeTime = FP._0;
        }
      }
    }

    /// <summary>
    /// Creates and setup the bullet entity 
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="character">The shooter character entity reference.</param>
    /// <param name="weapon">The weapon component pointer.</param>
    /// <param name="direction">The direction player input is pointing.</param>
    private static void SpawnBullet(Frame frame, EntityRef character, Weapon* weapon, FPVector2 direction)
    {
      weapon->CurrentAmmo -= 1;
      if (weapon->CurrentAmmo == 0)
      {
        weapon->IsRecharging = true;
      }

      var weaponData = frame.FindAsset(weapon->WeaponData);
      var bulletData = frame.FindAsset(weaponData.BulletData);
      var prototypeAsset = frame.FindAsset(bulletData.BulletPrototype);

      //Created the bullet entity
      var bullet = frame.Create(prototypeAsset);
      var bulletFields = frame.Unsafe.GetPointer<BulletFields>(bullet);
      var bulletTransform = frame.Unsafe.GetPointer<Transform2D>(bullet);
      
      // Bullet setup
      var characterTransform = frame.Unsafe.GetPointer<Transform2D>(character);
      var fireSpotWorldOffset = WeaponHelper.GetFireSpotWorldOffset(frame.FindAsset(weapon->WeaponData), direction);
      bulletTransform->Position = characterTransform->Position + fireSpotWorldOffset;
      bulletFields->Direction = direction * weaponData.ShootForce;
      bulletFields->Source = character;
      bulletFields->BulletData = bulletData;

      // Restarts the timer necessary to auto reload
      weapon->DelayToStartRechargeTimer = FrameTimer.FromSeconds(frame, weaponData.TimeToRecharge);
      
      frame.Events.OnWeaponShoot(character);
    }

    /// <summary>
    /// Triggered by OnCharacterRespawn signal, restores the weapon data when character respawns. 
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="character">The character entity reference.</param>
    public void OnCharacterRespawn(Frame frame, EntityRef character)
    {
      WeaponInventory* weaponInventory = frame.Unsafe.GetPointer<WeaponInventory>(character);

      for (var i = 0; i < weaponInventory->Weapons.Length; i++)
      {
        var weapon = weaponInventory->Weapons.GetPointer(i);
        var weaponData = frame.FindAsset(weapon->WeaponData);

        weapon->IsRecharging = false;
        weapon->CurrentAmmo = weaponData.MaxAmmo;
        weapon->FireRateTimer = FrameTimer.FromFrames(frame, 0);
        weapon->DelayToStartRechargeTimer = FrameTimer.FromFrames(frame, 0);
        weapon->RechargeRate = FrameTimer.FromFrames(frame, 0);
      }
    }
  }
}