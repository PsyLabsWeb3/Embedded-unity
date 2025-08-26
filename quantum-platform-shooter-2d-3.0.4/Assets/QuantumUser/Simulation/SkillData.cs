namespace Quantum
{
  using Photon.Deterministic;
  using UnityEngine;
  /// <summary>
  /// The <c>SkillData</c> is a data asset for store skill information
  /// </summary>
  public class SkillData : AssetObject
  {
    [Tooltip("Prototype reference to spawn bullet projectiles")]
    public AssetRef<EntityPrototype> SkillPrototype;
    [Tooltip("Time delay until the skill activation.")]
    public FP ActivationDelay;
    [Tooltip("Damage applied in the target character.")]
    public FP Damage;
    [Tooltip("Shape to apply skill affect.")]
    public Shape2DConfig ShapeConfig;
  }
}
