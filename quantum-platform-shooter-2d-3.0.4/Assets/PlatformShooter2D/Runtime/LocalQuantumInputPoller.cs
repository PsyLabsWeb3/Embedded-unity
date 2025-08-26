
namespace PlatformShooter2D
{
  using Photon.Deterministic;
  using UnityEngine;
  using QuantumMobileInputTools;
  using Quantum;

  /// <summary>
  /// This component is attached to the character avatars and registers to the Quantum poll input callback to provide player input.
  /// It's only supposed to run for a local player, so it will deactivate itself during <see cref="OnActivate(Quantum.Frame)"/> in other cases.
  /// </summary>
  public class LocalQuantumInputPoller : QuantumEntityViewComponent<CustomViewContext>
  {
    /// <summary>
    /// The angle threshold for aim assist.
    /// </summary>
    public float AimAssist = 20;

    /// <summary>
    /// The speed at which the aim assist adjusts the aim direction.
    /// </summary>
    public float AimSpeed = 2;

    /// <summary>
    /// Stores the direction if the aim in the last update. 
    /// </summary>
    private Vector2 _lastPlayerDirection;

    /// <summary>
    /// Initializes the component, setting up player input and default aim direction.
    /// </summary>
    public override void OnInitialize()
    {
      _lastPlayerDirection = Vector2.left;
    }

    /// <summary>
    /// Activates the component and subscribes to Quantum's input polling if this is the local player.
    /// </summary>
    /// <param name="frame">The Quantum frame for data access.</param>
    public override void OnActivate(Frame frame)
    {
      var playerLink = VerifiedFrame.Get<PlayerLink>(EntityRef);

      if (Game.PlayerIsLocal(playerLink.Player))
      {
        QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback),
          onlyIfActiveAndEnabled: true);
      }
    }

    /// <summary>
    /// Updates the view context with the latest local player aim direction.
    /// </summary>
    public override void OnUpdateView()
    {
      var playerLink = VerifiedFrame.Get<PlayerLink>(EntityRef);
      if (Game.PlayerIsLocal(playerLink.Player))
      {
        ViewContext.LocalCharacterLastDirection = GetAimDirection();
      }
    }

    /// <summary>
    /// Polls input data and set it to Quantum's input system.
    /// </summary>
    /// <param name="callback">The input polling callback.</param>
    public void PollInput(CallbackPollInput callback)
    {
      QuantumDemoInputPlatformer2D input = default;

      if (callback.Game.GetLocalPlayers().Count == 0)
      {
        return;
      }
      var control = QuantumLocalInputValuesControl.Instance;

      input.Fire = control.GetControlValue(ControlMap.Fire).BoolValue;

#if UNITY_MOBILE || UNITY_ANDROID
      var aimDirection = control.GetControlValue(ControlMap.Aim).Vector2Value;
      input.Fire = (aimDirection.magnitude >= 0.5f);
#endif
      var movement = GetMovement();
      input.Left = movement < 0;
      input.Right = movement > 0;


      input.Jump =control.GetControlValue(ControlMap.Jump).BoolValue;
      input.AimDirection = GetAimDirection();
      input.Use = control.GetControlValue(ControlMap.ChangeWeapon).BoolValue;
      input.AltFire = control.GetControlValue(ControlMap.CastSkill).BoolValue;

      callback.SetInput(input, DeterministicInputFlags.Repeatable);
    }

    /// <summary>
    /// Reads and returns horizontal movement input.
    /// </summary>
    /// <returns>The horizontal movement input as a fixed-point value.</returns>
    private FP GetMovement()
    {
      var control = QuantumLocalInputValuesControl.Instance;
      FPVector2 directional = control.GetControlValue(ControlMap.Move).Vector2Value.ToFPVector2();
      return directional.X;
    }

    /// <summary>
    /// Calculates and returns the player's aim direction based on input and aim assist.
    /// </summary>
    /// <returns>The aim direction as a fixed-point 2D vector.</returns>
    private FPVector2 GetAimDirection()
    {
      var control = QuantumLocalInputValuesControl.Instance;
      Vector2 direction = Vector2.zero;
      Frame frame = PredictedFrame;
      var isMobile = false;

#if !UNITY_STANDALONE && !UNITY_WEBGL
      isMobile = true;
#endif
      if (frame.TryGet<Transform2D>(EntityRef, out var characterTransform))
      {
        if (isMobile)
        {
          Vector2 directional = control.GetControlValue(ControlMap.Aim).Vector2Value;
          var controlDir = new Vector2(directional.x, directional.y);
          if (controlDir.sqrMagnitude > 0.1f)
          {
            direction = controlDir;
          }
          else if (Mathf.Abs(GetMovement().AsFloat) > 0.1f)
          {
            direction = new Vector2(GetMovement().AsFloat, 0);
          }
          else
          {
            direction = _lastPlayerDirection;
          }

          _lastPlayerDirection = direction;

          // Apply aim assist
          var minorAngle = AimAssist;
          var position = frame.Get<Transform2D>(EntityRef).Position;
          var targetDirection = position - characterTransform.Position;

          if (Vector2.Angle(direction, targetDirection.ToUnityVector2()) <= minorAngle)
          {
            direction = Vector2.Lerp(direction, targetDirection.ToUnityVector2(), Time.deltaTime * AimSpeed);
          }
        }
        else
        {
          var localCharacterPosition = characterTransform.Position.ToUnityVector3();
          var localCharacterScreenPosition = Camera.main.WorldToScreenPoint(localCharacterPosition);
          var mousePos = control.GetControlValue(ControlMap.MousePosition).Vector2Value;;
          if (!Application.isFocused)
          {
            mousePos = Vector2.zero;
          }

          direction = mousePos - new Vector2(localCharacterScreenPosition.x, localCharacterScreenPosition.y);
        }

        return new FPVector2(FP.FromFloat_UNSAFE(direction.x), FP.FromFloat_UNSAFE(direction.y));
      }

      return FPVector2.Zero;
    }
  }
}
