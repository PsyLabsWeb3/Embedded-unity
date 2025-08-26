namespace PlatformShooter2D
{
  using Quantum;
  using UnityEngine.UI;

  /// <summary>
  /// Updates the weapon icon on the touch UI hub when the local player changes their weapon.
  /// Used in the TouchUI game object on the ChangeWeaponButton.
  /// </summary>
  public class ChangeWeaponHud : QuantumSceneViewComponent<CustomViewContext>
  {
    /// <summary>
    /// The UI image component used to display the weapon icon.
    /// </summary>
    public Image WeaponIconImage;

    /// <summary>
    /// Called when the component is activated. Subscribes to the weapon change event.
    /// </summary>
    /// <param name="frame">The current Quantum frame.</param>
    public override void OnActivate(Frame frame)
    {
      QuantumEvent.Subscribe<EventOnCharacterChangeWeaponLocal>(this, OnWeaponChanged);
    }

    /// <summary>
    /// Updates the weapon icon when the weapon change event is triggered.
    /// </summary>
    /// <param name="eventData">Data associated with the weapon change event.</param>
    public void OnWeaponChanged(EventOnCharacterChangeWeaponLocal eventData)
    {
      // Retrieve the weapon data from the predicted frame and update the UI icon.
      var weaponData = PredictedFrame.FindAsset(eventData.WeaponData);
      WeaponIconImage.sprite = weaponData.UIIcon;
    }
  }
}