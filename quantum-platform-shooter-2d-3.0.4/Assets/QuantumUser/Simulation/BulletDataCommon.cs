
namespace Quantum
{
  using Photon.Deterministic;
  /// <summary>
  /// The <c>BulletDataCommon</c> is a normal bullet behaviour, deals damage on a character
  /// </summary>
  [System.Serializable]
  public class BulletDataCommon : BulletData
  {
    /// <inheritdoc cref="BulletData.BulletAction"/>
    public override unsafe void BulletAction(Frame frame, EntityRef bullet, EntityRef targetCharacter)
    {
      if (targetCharacter != EntityRef.None)
      {
        frame.Signals.OnCharacterHit(bullet, targetCharacter, Damage);
      }

      var fields = frame.Get<BulletFields>(bullet);
      var position = frame.Get<Transform2D>(bullet).Position;
      frame.Events.OnBulletDestroyed(bullet.GetHashCode(), fields.Source, position, fields.Direction, fields.BulletData);
      frame.Destroy(bullet);
    }
  }
}