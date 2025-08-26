namespace PlatformShooter2D
{
  using UnityEngine;

  /// <summary>
  /// Controls inverse kinematics (IK) for the Animator component
  /// </summary>
  public class IkControl : MonoBehaviour
  {
    /// <summary>
    /// Indicates whether IK is active.
    /// </summary>
    public bool IkActive = false;

    /// <summary>
    /// Target transform for the right hand IK.
    /// </summary>
    public Transform RightHandObj;

    /// <summary>
    /// Target transform for the left hand IK.
    /// </summary>
    public Transform LeftHandObj;

    /// <summary>
    /// Target transform for the look-at IK.
    /// </summary>
    public Transform LookObj;

    /// <summary>
    /// Reference to the Animator component.
    /// </summary>
    private Animator _animator; 

    /// <summary>
    /// Initializes the Animator reference on start.
    /// </summary>
    void Start()
    {
      _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Handles the IK pass for the Animator, adjusting positions and rotations
    /// for the hands and look direction based on the specified targets.
    /// </summary>
    void OnAnimatorIK()
    {
      if (_animator)
      {
        if (IkActive)
        {
          if (LookObj != null)
          {
            _animator.SetLookAtWeight(1);
            _animator.SetLookAtPosition(LookObj.position);
          }

          if (RightHandObj != null)
          {
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            _animator.SetIKPosition(AvatarIKGoal.RightHand, RightHandObj.position);
            _animator.SetIKRotation(AvatarIKGoal.RightHand, RightHandObj.rotation);
          }

          if (LeftHandObj != null)
          {
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            _animator.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandObj.position);
            _animator.SetIKRotation(AvatarIKGoal.LeftHand, LeftHandObj.rotation);
          }
        }
        else
        {
          _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
          _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
          _animator.SetLookAtWeight(0);
        }
      }
    }
  }
}
