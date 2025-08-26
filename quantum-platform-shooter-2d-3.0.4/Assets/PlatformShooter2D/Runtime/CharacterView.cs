namespace PlatformShooter2D
{
  using Quantum;
  using UnityEngine;

  /// <summary>
  /// Handles the visual representation and behavior of a character in the game.
  /// </summary>
  public class CharacterView : QuantumEntityViewComponent<CustomViewContext>
  {
    /// <summary>
    /// The transform representing the character's body.
    /// </summary>
    public Transform Body;

    /// <summary>
    /// Animator component controlling the character's animations.
    /// </summary>
    public Animator CharacterAnimator;

    /// <summary>
    /// Direction the character is currently facing: 1 for right, -1 for left.
    /// </summary>
    [HideInInspector] public int LookDirection;

    /// <summary>
    /// Rotation of the body when facing right.
    /// </summary>
    private readonly Vector3 _rightRotation = Vector3.zero;

    /// <summary>
    /// Rotation of the body when facing left.
    /// </summary>
    private readonly Vector3 _leftRotation = new(0, 180, 0);

    /// <summary>
    /// Cached hash for the "IsFacingRight" parameter in the Animator.
    /// </summary>
    private static readonly int IsFacingRight = Animator.StringToHash("IsFacingRight");

    /// <summary>
    /// Called when the character view is activated. Sets up local player linkage if applicable.
    /// </summary>
    /// <param name="frame">The current Quantum frame.</param>
    public override void OnActivate(Frame frame)
    {
      // Retrieve the PlayerLink component for this entity.
      PlayerLink playerLink = VerifiedFrame.Get<PlayerLink>(EntityRef);

      // Check if the entity belongs to the local player and set the local view context.
      if (Game.PlayerIsLocal(playerLink.Player))
      {
        ViewContext.LocalCharacterView = this;
      }
    }

    /// <summary>
    /// Updates the character's view based on its current facing direction.
    /// </summary>
    public override void OnUpdateView()
    {
      if (CharacterAnimator.GetBool(IsFacingRight))
      {
        // Rotate to face right.
        Body.localRotation = Quaternion.Euler(_rightRotation); 
        // Update look direction to right.
        LookDirection = 1; 
      }
      else
      {
        // Rotate to face left.
        Body.localRotation = Quaternion.Euler(_leftRotation);
        // Update look direction to left.
        LookDirection = -1;
      }
    }
  }
}