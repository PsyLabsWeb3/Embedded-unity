
namespace Quantum
{
  using Photon.Deterministic;
  using UnityEngine;
  /// <summary>
  /// The <c>SkillInventoryData</c> is a data asset for store how the character manages skill cast rate,
  /// cast force and the asset reference to the <c>SkillInventoryData</c>
  /// </summary>
  public class SkillInventoryData : AssetObject
  {
    [Tooltip("Time delay between skill casting.")]
    public FP CastRate;
    [Tooltip("The physics force applied to the skill casted.")]
    public FP CastForce;
    [Tooltip("The asset reference to the SkillData.")]
    public AssetRef<SkillData> SkillData;
  }
}
