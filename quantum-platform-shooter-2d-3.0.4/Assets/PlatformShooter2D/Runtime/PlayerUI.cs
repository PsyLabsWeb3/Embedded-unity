namespace PlatformShooter2D
{
  using TMPro;
  using Quantum;
  using UnityEngine;

  // ReSharper disable InvalidXmlDocComment
  /// <summary>
  /// Displays and updates the player UI, including health, weapon ammo, fire rate, and player name.
  /// </summary>
  public class PlayerUI : QuantumEntityViewComponent
  {
    [Header("References")] 
    /// <summary>
    /// Reference to the health bar sprite.
    /// </summary>
    public SpriteRenderer Health;

    /// <summary>
    /// Reference to the weapon ammo bar sprite.
    /// </summary>
    public SpriteRenderer WeaponAmmo;

    /// <summary>
    /// Reference to the weapon fire rate bar sprite.
    /// </summary>
    public SpriteRenderer WeaponFireRate;

    /// <summary>
    /// Text component to display the player's name.
    /// </summary>
    public TMP_Text PlayerName;

    [Header("Configurations")] 
    /// <summary>
    /// Color configuration for enemy players.
    /// </summary>
    public Color EnemyColor;

    /// <summary>
    /// Color for the weapon-related UI elements.
    /// </summary>
    public Color WeaponColor;

    /// <summary>
    /// Alpha value for the weapon color when it is active.
    /// </summary>
    public float WeaponColorAlpha = 0.5f;

    /// <summary>
    /// Alpha value for the weapon color when it is recharging.
    /// </summary>
    public float RechargingWeaponColorAlpha = 0.5f;

    /// <summary>
    /// Initial size of the health bar sprite.
    /// </summary>
    private Vector2 _healthInitialSize;

    /// <summary>
    /// Initial size of the weapon ammo bar sprite.
    /// </summary>
    private Vector2 _weaponAmmoInitialSize;

    /// <summary>
    /// Initial size of the weapon fire rate bar sprite.
    /// </summary>
    private Vector2 _weaponFireRateInitialSize;

    /// <summary>
    /// Called when the component is activated. Initializes player UI values.
    /// </summary>
    /// <param name="frame">The current Quantum frame.</param>
    public override void OnActivate(Frame frame)
    {
      var player = VerifiedFrame.Get<PlayerLink>(EntityRef).Player;
      PlayerName.text = VerifiedFrame.GetPlayerData(player).PlayerNickname;

      if (Game.PlayerIsLocal(player) == false)
      {
        Health.color = EnemyColor;
        PlayerName.color = EnemyColor;
      }

      _healthInitialSize = Health.size;
      _weaponAmmoInitialSize = WeaponAmmo.size;
      _weaponFireRateInitialSize = WeaponFireRate.size;
    }

    /// <summary>
    /// Updates the player UI elements based on the current game state.
    /// </summary>
    public override void OnUpdateView()
    {
      var weaponInventory = VerifiedFrame.Get<WeaponInventory>(EntityRef);
      var currentWeapon = weaponInventory.Weapons[weaponInventory.CurrentWeaponIndex];
      var weaponData = VerifiedFrame.FindAsset(currentWeapon.WeaponData);

      var status = VerifiedFrame.Get<Status>(EntityRef);
      var statusData = VerifiedFrame.FindAsset(status.StatusData);

      var healthRatio = (status.CurrentHealth / statusData.MaxHealth).AsFloat;
      var ammoRatio = (float)currentWeapon.CurrentAmmo / weaponData.MaxAmmo;
      var remainingFireTimer = (float)currentWeapon.FireRateTimer.RemainingSeconds(VerifiedFrame);
      var fireRateRatio = Mathf.Clamp01( remainingFireTimer / weaponData.FireRate.AsFloat);

      Health.size = new Vector2(healthRatio * _healthInitialSize.x, _healthInitialSize.y);
      WeaponAmmo.size = new Vector2(ammoRatio * _weaponAmmoInitialSize.x, _weaponAmmoInitialSize.y);
      WeaponFireRate.size = new Vector2((1 - fireRateRatio) * _weaponFireRateInitialSize.x, _weaponFireRateInitialSize.y);

      WeaponColor.a = currentWeapon.IsRecharging ? RechargingWeaponColorAlpha : WeaponColorAlpha;
      WeaponAmmo.color = WeaponColor;
    }
  }
}
