namespace Quantum
{
  using Photon.Deterministic;
  using UnityEngine;
  
  /// <summary>
  /// The <c>BulletDataExplosive</c> is a explosive bullet behavior.
  /// Explodes on impact, deals damage to all characters in the ExplosionRadius.
  /// </summary>
  [System.Serializable]
  public class BulletDataExplosive : BulletData
  {
    [Tooltip("Shape used to perform damage.")]
    public Shape2DConfig ExplosionShape;
    
    /// <inheritdoc cref="BulletData.BulletAction"/>
    public override unsafe void BulletAction(Frame frame, EntityRef bullet, EntityRef targetCharacter)
    {
      Explode(frame, bullet, targetCharacter);

      if (targetCharacter != EntityRef.None)
      {
        frame.Signals.OnCharacterHit(bullet, targetCharacter, Damage);
      }

      var bulletFields = frame.Get<BulletFields>(bullet);
      var bulletPosition = frame.Get<Transform2D>(bullet).Position;

      frame.Events.OnBulletDestroyed(bullet.GetHashCode(), bulletFields.Source, bulletPosition, bulletFields.Direction, Guid);
      frame.Destroy(bullet);
    }
    
    /// <summary>
    /// Post hit effects should happen on this function call.
    /// 
    /// Eg.: Explosions, damage calculations, etc.
    /// </summary>
    /// <param name="frame">The Quantum game Frame</param>
    /// <param name="bullet">The bullet EntityRef that triggered the function call</param>
    /// <param name="targetCharacter">The target of the bullet (it EntityRef.None when hitting a static collider)</param>
    private unsafe void Explode(Frame frame, EntityRef bullet, EntityRef character)
    {
      var bulletTransform = frame.Get<Transform2D>(bullet);
      var hits = frame.Physics2D.OverlapShape(bulletTransform, ExplosionShape.CreateShape(frame));
      for (int i = 0; i < hits.Count; i++)
      {
        EntityRef entity = hits[i].Entity;

        // Only consider character for damage
        if (entity == EntityRef.None || frame.Has<Status>(entity) == false)
        {
          continue;
        }

        // Only deal damages to character not behind walls 
        Transform2D currentBotTransform = frame.Get<Transform2D>(entity);
        if (LineOfSightHelper.HasLineOfSight(frame, bulletTransform.Position, currentBotTransform.Position) == false)
        {
          continue;
        }

        // Don't hit the target character, we deal his damage in another place so it doesn't suffer falloff
        if (entity == character)
        {
          continue;
        }

        var distance = FPVector2.Distance(bulletTransform.Position, currentBotTransform.Position);
        var damagePercentage = 1 - distance / (ExplosionShape.CircleRadius);
        damagePercentage = FPMath.Clamp01(damagePercentage);
        var damage = Damage * damagePercentage;

        frame.Signals.OnCharacterHit(bullet, entity, damage);
      }
    }
  }
}