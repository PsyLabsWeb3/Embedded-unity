namespace PlatformShooter2D
{
  using UnityEngine;

  /// <summary>
  /// Handles the visualization of a bullet's trajectory using a LineRenderer component.
  /// </summary>
  [RequireComponent(typeof(LineRenderer))]
  public class BulletLineRenderer : MonoBehaviour
  {
    /// <summary>
    /// The length of the bullet's line trail.
    /// </summary>
    public float Length = 1;

    /// <summary>
    /// Reference to the LineRenderer component.
    /// </summary>
    private LineRenderer _lineRenderer;
    
    /// <summary>
    /// Tracks the last recorded position of the bullet.
    /// </summary>
    private Vector3 _lastPos;

    /// <summary>
    /// Called when the script instance is being loaded. Initializes the LineRenderer.
    /// </summary>
    public void Awake()
    {
      _lineRenderer = GetComponent<LineRenderer>();
    }

    /// <summary>
    /// Resets the LineRenderer positions when the object is disabled.
    /// </summary>
    public void OnDisable()
    {
      _lineRenderer.SetPosition(0, transform.position);
      _lineRenderer.SetPosition(1, transform.position);
      _lastPos = transform.position;
    }

    /// <summary>
    /// Updates the LineRenderer to reflect the bullet's movement each frame.
    /// </summary>
    public void Update()
    {
      // Calculate the direction of the bullet's movement.
      var direction = Vector3.Normalize(transform.position - _lastPos);

      // Update the LineRenderer's start and end positions based on the bullet's trajectory.
      _lineRenderer.SetPosition(0, transform.position + direction / Length);
      _lineRenderer.SetPosition(1, transform.position + direction / Length - direction * Length);

      // Update the last recorded position.
      _lastPos = transform.position;
    }
  }
}