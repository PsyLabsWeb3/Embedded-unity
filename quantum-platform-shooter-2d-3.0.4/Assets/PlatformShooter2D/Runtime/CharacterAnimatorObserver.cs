namespace PlatformShooter2D
{
  using UnityEngine;
  using Quantum;

  /// <summary>
  /// Observes character movement and events to update the character's animations accordingly.
  /// </summary>
  public sealed class CharacterAnimatorObserver : QuantumEntityViewComponent<CustomViewContext>
  {
    /// <summary>
    /// Animator to control the character's animation state.
    /// </summary>
    public Animator Animator;

    /// <summary>
    /// Current velocity of the character.
    /// </summary>
    public Vector3 Velocity;

    /// <summary>
    /// The previous position for velocity calculation.
    /// </summary>
    private Vector3 _lastPosition;

    /// <summary>
    /// Hash for the "VelocityX" parameter in the Animator.
    /// </summary>
    private static readonly int VelocityX = Animator.StringToHash("VelocityX");

    /// <summary>
    /// Hash for the "IsFacingRight" parameter in the Animator.
    /// </summary>
    private static readonly int IsFacingRight = Animator.StringToHash("IsFacingRight");

    /// <summary>
    /// Hash for the "IsGrounded" parameter in the Animator.
    /// </summary>
    private static readonly int
      IsGrounded = Animator.StringToHash("IsGrounded");

    /// <summary>
    /// Hash for the "DoubleJump" trigger in the Animator.
    /// </summary>
    private static readonly int
      DoubleJump = Animator.StringToHash("DoubleJump");

    /// <summary>
    /// Subscribes to the jump event when the component is activated.
    /// </summary>
    public override void OnActivate(Frame frame)
    {
      QuantumEvent.Subscribe<EventJumped>(this, OnCharacterJumped);
    }

    /// <summary>
    /// Handles the jump event by triggering the "DoubleJump" animation if the character has performed a double jump.
    /// </summary>
    private void OnCharacterJumped(EventJumped eventData)
    {
      var kcc = PredictedFrame.Get<KCC2D>(eventData.Entity);
      if (kcc._state == KCCState.DOUBLE_JUMPED && EntityRef.Equals(eventData.Entity))
      {
        Animator.SetTrigger(DoubleJump);
      }
    }

    /// <summary>
    /// Updates the character animations if the entity is the local character.
    /// </summary>
    public void Update()
    {
      if (ViewContext?.LocalCharacterView != null)
      {
        if (EntityView.EntityRef == ViewContext.LocalCharacterView.EntityRef)
        {
          UpdateCharacterAnimations(true);
        }
      }
    }

    /// <summary>
    /// Updates the character animations for non-local characters.
    /// </summary>
    public override void OnUpdateView()
    {
      if (ViewContext?.LocalCharacterView != null && ViewContext.LocalCharacterView.EntityRef != EntityView.EntityRef)
      {
        UpdateCharacterAnimations(false);
      }
    }

    /// <summary>
    /// Updates the character animations based on the character's movement and orientation.
    /// </summary>
    private void UpdateCharacterAnimations(bool isLocal)
    {
      if (PredictedFrame == null || PredictedFrame.Exists(EntityRef) == false) return;

      var characterMovement = PredictedFrame.Get<MovementData>(EntityRef);
      bool isFacingRight = isLocal ? ViewContext.LocalCharacterLastDirection.X > 0 : characterMovement.IsFacingRight;
      Animator.SetBool(IsFacingRight, isFacingRight);

      var kcc = PredictedFrame.Get<KCC2D>(EntityRef);
      Animator.SetBool(IsGrounded, kcc._state == KCCState.GROUNDED);

      Velocity = (transform.position - _lastPosition) /
                 Time.deltaTime; // Calculate the velocity based on position change
      _lastPosition = transform.position;

      var vel = Velocity.x;
      if (isFacingRight == false)
      {
        vel *= -1; // Flip the velocity if not facing right
      }

      Animator.SetFloat(VelocityX, vel); // Set the velocity on the Animator
    }
  }
}