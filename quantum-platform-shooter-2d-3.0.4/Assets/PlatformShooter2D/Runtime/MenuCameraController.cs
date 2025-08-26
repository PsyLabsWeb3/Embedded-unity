namespace PlatformShooter2D
{
  using Quantum;
  using UnityEngine;

  /// <summary>
  /// Controls the menu camera, enabling it only when no QuantumRunner is active.
  /// </summary>
  public class MenuCameraController : MonoBehaviour
  {
    private Camera _camera;

    /// <summary>
    /// Initializes the component and caches the camera reference.
    /// </summary>
    private void Start()
    {
      _camera = GetComponent<Camera>();
    }

    /// <summary>
    /// Updates the camera's enabled state based on the presence of a QuantumRunner.
    /// </summary>
    private void Update()
    {
      // Enable the menu camera only if no QuantumRunner is active.
      _camera.enabled = QuantumRunner.Default == null;
    }
  }
}