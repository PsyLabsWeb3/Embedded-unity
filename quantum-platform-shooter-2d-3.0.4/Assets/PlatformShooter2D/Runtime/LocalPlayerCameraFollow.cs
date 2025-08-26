namespace PlatformShooter2D
{
  using Quantum;
  using UnityEngine;

  /// <summary>
  ///  Controls the camera movement following the local character position.
  /// </summary>
  [RequireComponent(typeof(Camera))]
  public class LocalPlayerCameraFollow : QuantumSceneViewComponent<CustomViewContext>
  {
    /// <summary>
    /// The smoothing time for the camera's position on the X and Y axes.
    /// </summary>
    public float SmoothTime = 0.3f;

    /// <summary>
    /// The maximum speed at which the camera can follow the target.
    /// </summary>
    public float MaxSpeed = 10.0f;

    /// <summary>
    /// The horizontal offset for the camera's target position based on player direction.
    /// </summary>
    public float LookOffset = 10.0f;

    /// <summary>
    /// The smoothing time for the camera's Z-axis adjustments.
    /// </summary>
    public float ZSmoothTime = 0.5f;

    /// <summary>
    /// The current velocity of the camera.
    /// </summary>
    private Vector2 _currentVelocity;
    
    /// <summary>
    /// The distance before the first update.
    /// </summary>
    private float _originalDistance;
    
    private float _zVelocity = 0.0f;
    private float _zDistance = 0.0f;
    
    /// <summary>
    /// Reference to the camera that will be updated.
    /// </summary>
    private Camera _localCamera;

    /// <summary>
    /// Initializes the component and caches the camera reference.
    /// </summary>
    public override void OnInitialize()
    {
      _localCamera = GetComponent<Camera>();
      _originalDistance = _localCamera.transform.position.z;
    }

    /// <summary>
    /// Updates the camera's position to smoothly follow the local player's view.
    /// </summary>
    public override void OnUpdateView()
    {
      // Exit if there's no local player view to follow.
      if (ViewContext.LocalCharacterView == null)
      {
        return;
      }

      // Retrieve current and target positions for the camera.
      Vector2 cameraPosition = _localCamera.transform.position;
      Vector2 targetPosition = ViewContext.LocalCharacterView.transform.position;

      // Apply horizontal offset based on the player's look direction.
      targetPosition.x += LookOffset * ViewContext.LocalCharacterView.LookDirection;

      // Smoothly interpolate the camera's position on the X and Y axes.
      cameraPosition = Vector2.SmoothDamp(
        cameraPosition,
        targetPosition,
        ref _currentVelocity,
        SmoothTime,
        MaxSpeed,
        Time.deltaTime
      );

      // Smoothly adjust the Z-axis distance of the camera.
      var targetDistance = 0.0f; // This can be updated dynamically if needed.
      _zDistance = Mathf.SmoothDamp(_zDistance, targetDistance, ref _zVelocity, ZSmoothTime);

      // Update the camera's final position.
      _localCamera.transform.position = new Vector3(
        cameraPosition.x,
        cameraPosition.y,
        _originalDistance - _zDistance
      );
    }
  }
}
