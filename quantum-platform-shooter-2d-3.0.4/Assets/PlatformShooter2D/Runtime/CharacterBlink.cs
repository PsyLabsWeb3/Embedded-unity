namespace PlatformShooter2D
{
  using UnityEngine;
  using Quantum;

  // ReSharper disable InvalidXmlDocComment
  /// <summary>
  /// A component that makes the character flash when receiving damage.
  /// </summary>
  public sealed class CharacterBlink : QuantumEntityViewComponent
  {
    [Header("Blink Settings")]
    /// <summary>
    /// The material used for the blinking effect.
    /// </summary>
    public Material BlinkDamageMaterial;

    /// <summary>
    /// Renderer for the boy character's mesh.
    /// </summary>
    public Renderer BoyMesh;

    /// <summary>
    /// Renderer for the girl character's mesh.
    /// </summary>
    public Renderer GirlMesh;

    /// <summary>
    /// The duration for which the character will flash.
    /// </summary>
    public float BlinkDuration = 0.1f;

    // Cached original materials for both meshes.
    /// <summary>
    /// Cached original materials for boy mesh.
    /// </summary>
    private Material[] _originalMaterialsBoy;
    
    /// <summary>
    /// Cached original materials for girl mesh.
    /// </summary>
    private Material[] _originalMaterialsGirl;

    /// <summary>
    /// Materials array used during the blink effect for boy character.
    /// </summary>
    private Material[] _blinkMaterialsBoy;
    
    /// <summary>
    /// Materials array used during the blink effect for girl character.
    /// </summary>
    private Material[] _blinkMaterialsGirl;

    /// <summary>
    /// Initializes the component, sets up materials, and subscribes to the blink event.
    /// </summary>
    public override void OnInitialize()
    {
      // Subscribe to the event for character blinking on damage.
      QuantumEvent.Subscribe<EventOnCharacterBlink>(this, HandleBlinkEvent, onlyIfActiveAndEnabled: true);

      // Set up materials for blinking.
      SetupMaterials();
    }

    /// <summary>
    /// Prepares the original and blinking materials for character mesh.
    /// </summary>
    private void SetupMaterials()
    {
      // Setup materials for the boy character, if present.
      if (BoyMesh != null)
      {
        _originalMaterialsBoy = BoyMesh.sharedMaterials;
        _blinkMaterialsBoy = new Material[_originalMaterialsBoy.Length];
        for (int i = 0; i < _blinkMaterialsBoy.Length; i++)
        {
          _blinkMaterialsBoy[i] = BlinkDamageMaterial;
        }
      }

      // Setup materials for the girl character, if present.
      if (GirlMesh != null)
      {
        _originalMaterialsGirl = GirlMesh.sharedMaterials;
        _blinkMaterialsGirl = new Material[_originalMaterialsGirl.Length];
        for (int i = 0; i < _blinkMaterialsGirl.Length; i++)
        {
          _blinkMaterialsGirl[i] = BlinkDamageMaterial;
        }
      }
    }

    /// <summary>
    /// Handles the character blink event.
    /// </summary>
    /// <param name="eventData">The event data containing information about the character to blink.</param>
    private void HandleBlinkEvent(EventOnCharacterBlink eventData)
    {
      // Check if the event is targeting this character entity.
      if (eventData.Character.Equals(EntityRef))
      {
        StartCoroutine(Blink());
      }
    }

    /// <summary>
    /// Executes the blink effect, switching to the blink material and then reverting to the original material after a delay.
    /// </summary>
    private System.Collections.IEnumerator Blink()
    {
      // Apply blinking materials to both meshes if they are assigned.
      if (BoyMesh != null)
      {
        BoyMesh.materials = _blinkMaterialsBoy;
      }

      if (GirlMesh != null)
      {
        GirlMesh.materials = _blinkMaterialsGirl;
      }

      // Wait for the duration of the blink effect.
      yield return new WaitForSeconds(BlinkDuration);

      // Restore the original materials to both meshes.
      if (BoyMesh != null)
      {
        BoyMesh.materials = _originalMaterialsBoy;
      }

      if (GirlMesh != null)
      {
        GirlMesh.materials = _originalMaterialsGirl;
      }
    }
  }
}
