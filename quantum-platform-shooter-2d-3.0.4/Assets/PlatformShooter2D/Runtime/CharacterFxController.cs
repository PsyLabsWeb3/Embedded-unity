namespace PlatformShooter2D
{
  using Quantum;
  using UnityEngine;

  /// <summary>
  /// Handles visual effects for a player character, such as landing, jumping, double-jumping, dying, and respawning.
  /// </summary>
  public class CharacterFxController : QuantumEntityViewComponent
  {
    /// <summary>
    /// Particle systems for various character actions.
    /// </summary>
    public ParticleSystem LandingParticle;
    
    /// <summary>
    /// Particle effect triggered upon double jumping.
    /// </summary>
    public ParticleSystem DoubleJumpParticle;  
    
    /// <summary>
    /// Particle effect triggered upon death.
    /// </summary>
    public ParticleSystem ExplosionParticle;
    
    /// <summary>
    /// Particle effect triggered upon jumping.
    /// </summary>
    public ParticleSystem JumpParticle; 
     
    /// <summary>
    /// Particle effect triggered upon respawning.
    /// </summary>
    public ParticleSystem RespawnParticle;      
    
    /// <summary>
    /// Parent object containing the character's body renderers.
    /// </summary>
    public GameObject BodyParent;

    /// <summary>
    /// Parent object containing the character's UI renderers.
    /// </summary>
    public GameObject UIParent;    

    /// <summary>
    /// Arrays to store renderers for toggling visibility during death and respawn
    /// </summary>
    private Renderer[] _characterMeshes;  // Renderers for the character's body
    
    /// <summary>
    /// Arrays to store renderers for toggling UI visibility during death and respawn
    /// </summary>
    private Renderer[] _characterUI;

    /// <summary>
    /// Called when the component is activated. Subscribes to character events and initializes renderers.
    /// </summary>
    /// <param name="frame">The current Quantum frame.</param>
    public override void OnActivate(Frame frame)
    {
      // Subscribe to gameplay events
      QuantumEvent.Subscribe<EventLanded>(this, OnLanded);
      QuantumEvent.Subscribe<EventOnCharacterDeath>(this, OnDeath);
      QuantumEvent.Subscribe<EventOnCharacterRespawn>(this, OnRespawn);
      QuantumEvent.Subscribe<EventJumped>(this, OnJump);

      // Cache the renderers for the body and UI
      _characterMeshes = BodyParent.GetComponentsInChildren<Renderer>(true);
      _characterUI = UIParent.GetComponentsInChildren<Renderer>();
    }

    /// <summary>
    /// Handles the jump event and triggers the appropriate particle effect based on the jump state.
    /// </summary>
    /// <param name="eventData">Data about the jump event.</param>
    private void OnJump(EventJumped eventData)
    {
      // Check if the event applies to this entity
      if (EntityRef.Equals(eventData.Entity))
      {
        var kcc = PredictedFrame.Get<KCC2D>(eventData.Entity);

        // Play the jump particle effect for single or double jumps
        if (kcc._state == KCCState.JUMPED)
        {
          JumpParticle.Play();
        }
        if (kcc._state == KCCState.DOUBLE_JUMPED)
        {
          DoubleJumpParticle.Play();
        }
      }
    }

    /// <summary>
    /// Handles the landing event and triggers the landing particle effect.
    /// </summary>
    /// <param name="eventData">Data about the landing event.</param>
    private void OnLanded(EventLanded eventData)
    {
      // Check if the event applies to this entity
      if (EntityRef.Equals(eventData.Entity))
      {
        LandingParticle.Play();
      }
    }

    /// <summary>
    /// Handles the death event. Plays the explosion particle effect and disables renderers.
    /// </summary>
    /// <param name="eventData">Data about the death event.</param>
    private void OnDeath(EventOnCharacterDeath eventData)
    {
      // Check if the event applies to this entity
      if (!EntityRef.Equals(eventData.Character))
        return;

      // Play explosion effect
      ExplosionParticle.Play();

      // Disable all body and UI renderers
      foreach (Renderer r in _characterMeshes)
        r.enabled = false;
      foreach (Renderer r in _characterUI)
        r.enabled = false;
    }

    /// <summary>
    /// Handles the respawn event. Plays the respawn particle effect and enables renderers.
    /// </summary>
    /// <param name="eventData">Data about the respawn event.</param>
    private void OnRespawn(EventOnCharacterRespawn eventData)
    {
      // Check if the event applies to this entity
      if (!EntityRef.Equals(eventData.Character))
        return;

      // Play respawn effect
      RespawnParticle.Play();

      // Re-enable all body and UI renderers
      foreach (Renderer r in _characterMeshes)
        r.enabled = true;
      foreach (Renderer r in _characterUI)
        r.enabled = true;
    }
  }
}
