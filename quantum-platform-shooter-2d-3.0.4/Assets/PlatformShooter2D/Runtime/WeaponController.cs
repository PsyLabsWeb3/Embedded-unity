using UnityEngine.Serialization;

namespace PlatformShooter2D
{
  using Photon.Deterministic;
  using Quantum;
  using TMPro;
  using UnityEngine;
  using Animator = UnityEngine.Animator;

  /// <summary>
  /// Controls the player's weapon behavior, including animations, position, rotation, and weapon effects.
  /// </summary>
  public sealed class WeaponController : QuantumEntityViewComponent<CustomViewContext>
  {
    /// <summary>
    /// Animator controlling character animations.
    /// </summary>
    public Animator CharacterAnimator;

    /// <summary>
    /// IK control for positioning and aiming.
    /// </summary>
    public IkControl Ik;

    /// <summary>
    /// Horizontal offset of the weapon.
    /// </summary>
    public float X = 0.1f;

    /// <summary>
    /// Root animation controller for the weapon.
    /// </summary>
    private WeaponAnimationRoot _animationRoot;

    /// <summary>
    /// Animator controlling weapon-specific animations.
    /// </summary>
    private Animator _animator;

    /// <summary>
    /// Array containing references to all weapon views.
    /// </summary>
    private WeaponView[] _weapons;

    private static readonly int ChangeWeaponHash = Animator.StringToHash("ChangeWeapon");

    /// <summary>
    /// Called when the component is activated. Initializes weapons and subscribes to events.
    /// </summary>
    public override void OnActivate(Frame frame)
    {
      _animationRoot = GetComponent<WeaponAnimationRoot>();
      _animator = GetComponent<Animator>();

      QuantumEvent.Subscribe<EventOnWeaponShoot>(this, ShootEffect);
      QuantumEvent.Subscribe<EventOnCharacterChangeWeapon>(this, ChangeWeapon);

      var weaponInventory = VerifiedFrame.Get<WeaponInventory>(EntityRef);
      _weapons = new WeaponView[weaponInventory.Weapons.Length];
      for (int i = 0; i < weaponInventory.Weapons.Length; i++)
      {
        var weaponData = QuantumUnityDB.GetGlobalAsset<WeaponData>(weaponInventory.Weapons[i].WeaponData.Id);
        var temp = Instantiate(weaponData.Prefab, transform);
        _weapons[i] = temp.GetComponent<WeaponView>();
        _weapons[i].transform.localPosition = Vector3.zero;
        _weapons[i].transform.localRotation = Quaternion.identity;
      }

      UpdateWeapon(EntityRef);
    }

    /// <summary>
    /// Called every frame to update the weapon's position, rotation, and state.
    /// </summary>
    public override void OnUpdateView()
    {
      if (PredictedFrame.TryGet<WeaponInventory>(EntityRef, out var weaponInventory))
      {
        var weapon = weaponInventory.Weapons[weaponInventory.CurrentWeaponIndex];
        var weaponData = PredictedFrame.FindAsset(weapon.WeaponData);
        var zAngle = GetPlayerAngle(PredictedFrame);
        var finalRotation = Quaternion.Euler(Mathf.Rad2Deg * zAngle * -1, 0, 0);
        UpdateWeapon(EntityRef);

        if (!CharacterAnimator.GetBool("IsFacingRight"))
        {
          finalRotation = Quaternion.Euler(180 - Mathf.Rad2Deg * zAngle * -1, 0, 0);
        }

        Vector3 positionOffset =
          new Vector3(X, weaponData.PositionOffset.Y.AsFloat, weaponData.PositionOffset.X.AsFloat); 
        transform.localPosition = positionOffset + _animationRoot.WeaponOffset;
        transform.localRotation = finalRotation;
      }
    }

    /// <summary>
    /// Calculates the player's aiming angle based on input or direction.
    /// </summary>
    private float GetPlayerAngle(Frame frame)
    {
      FPVector2 aimDirection;
      if (ViewContext.LocalCharacterView != null && EntityRef == ViewContext.LocalCharacterView.EntityRef)
      {
        aimDirection = ViewContext.LocalCharacterLastDirection;
      }
      else
      {
        if (frame.TryGet(EntityRef, out PlayerLink playerLink))
        {
          unsafe
          {
            Quantum.Input playerInput = Quantum.Input.GetPlayerInputValue(frame, playerLink.Player);
            aimDirection = playerInput.AimDirection;
          }
        }
        else
        {
          aimDirection = FPVector2.Zero;
        }
      }

      var angle = Mathf.Atan2(aimDirection.Y.AsFloat, aimDirection.X.AsFloat);
      angle = Mathf.Repeat((angle + 2 * Mathf.PI), 2 * Mathf.PI);
      return angle;
    }

    /// <summary>
    /// Visualizes weapon-related data in the editor using Gizmos.
    /// </summary>
    private void OnDrawGizmos()
    {
      if(!Application.isPlaying || VerifiedFrame == null) return;
      if (!VerifiedFrame.Exists(EntityRef)) return;

      var playerLink = VerifiedFrame.Get<PlayerLink>(EntityRef);
      var weaponInventory = VerifiedFrame.Get<WeaponInventory>(EntityRef);

      Quantum.Input playerInput = Quantum.Input.GetPlayerInputValue(VerifiedFrame, playerLink.Player);
      var aimDirection = playerInput.AimDirection;
      var weapon = weaponInventory.Weapons[weaponInventory.CurrentWeaponIndex];
      var weaponData = VerifiedFrame.FindAsset(weapon.WeaponData);

      var fireSpotOffset = WeaponHelper.GetFireSpotWorldOffset(weaponData, aimDirection);
      var weaponFireSpotPosition = transform.position + fireSpotOffset.ToUnityVector3();

      Gizmos.color = Color.red;
      Gizmos.DrawWireSphere(transform.position, 0.2f);
      Gizmos.DrawWireSphere(weaponFireSpotPosition, 0.2f);
      Gizmos.DrawLine(transform.position, weaponFireSpotPosition);

      Gizmos.color = Color.blue;
      Gizmos.DrawRay(weaponFireSpotPosition, aimDirection.ToUnityVector3());
    }

    /// <summary>
    /// Handles weapon change events by updating the weapon and triggering animations.
    /// </summary>
    private void ChangeWeapon(EventOnCharacterChangeWeapon eventData)
    {
      if (eventData.Character.Equals(EntityRef))
      {
        _animator.SetTrigger(ChangeWeaponHash);
        UpdateWeapon(eventData.Character);
      }
    }

    /// <summary>
    /// Updates the currently equipped weapon and its associated IK targets.
    /// </summary>
    private void UpdateWeapon(EntityRef character)
    {
      if (_weapons == null) return;
      
      var currentWeaponIndex = VerifiedFrame.Get<WeaponInventory>(character).CurrentWeaponIndex;
      var characterStatus = VerifiedFrame.Get<Status>(character);
      

      for (var i = 0; i < _weapons.Length; i++)
      {
        if (characterStatus.IsDead)
        {
          _weapons[i].gameObject.SetActive(false);
          continue;
        }
        if (i == currentWeaponIndex)
        {
          _weapons[i].gameObject.SetActive(true);
          Ik.RightHandObj = _weapons[i].RightHand;
          Ik.LeftHandObj = _weapons[i].LeftHand;
          Ik.LookObj = _weapons[i].LookDir;
        }
        else
        {
          _weapons[i].gameObject.SetActive(false);
        }
      }
    }

    /// <summary>
    /// Triggers shooting effects for the currently equipped weapon.
    /// </summary>
    private void ShootEffect(EventOnWeaponShoot eventData)
    {
      if (eventData.Character.Equals(EntityRef))
      {
        var characterInventory = PredictedFrame.Get<WeaponInventory>(eventData.Character);
        _weapons[characterInventory.CurrentWeaponIndex].ShootFx();
      }
    }
  }
}