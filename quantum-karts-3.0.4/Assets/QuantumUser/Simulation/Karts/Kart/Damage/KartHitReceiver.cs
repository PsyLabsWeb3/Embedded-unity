using Photon.Deterministic;
using Quantum.Physics3D;

namespace Quantum
{
    public unsafe partial struct KartHitReceiver
    {
        public void Update(Frame frame, KartSystem.Filter filter)
        {
            if (HitTimer > 0)
            {
                HitTimer -= frame.DeltaTime;

                if (HitTimer <= 0)
                {
                    HitTimer = 0;
                    frame.Events.OnHitEnd(filter.Entity);
                }
            }

            if (ImmunityTimer > 0)
            {
                ImmunityTimer -= frame.DeltaTime;

                if (ImmunityTimer <= 0)
                {
                    ImmunityTimer = 0;
                    frame.Events.OnImmunityEnd(filter.Entity);
                }
            }

            if (HitCooldownTimer > 0)
            {
                HitCooldownTimer -= frame.DeltaTime;
            }

            var hits = frame.Physics3D.GetQueryHits(HazardQuery);

            if (hits.Count > 0)
            {
                for (int i = 0; i < hits.Count; i++)
                {
                    Hit3D overlap = hits[i];

                    if (overlap.Entity == filter.Entity || !frame.Unsafe.TryGetPointer(overlap.Entity, out Hazard* hazard))
                    {
                        continue;
                    }

                    hazard->MarkedForDestruction = true;

                    if (hazard->DamageRadius == FP._0)
                    {
                        TakeHit(frame, filter.Entity, hazard->HitDuration);
                    }
                }
            }
        }

        public void GiveImmunity(Frame f, EntityRef entityRef, FP duration)
        {
            ImmunityTimer = FPMath.Max(ImmunityTimer, duration);

            f.Events.OnImmunityStart(entityRef);
        }

        public void TakeHit(Frame f, EntityRef entityRef, FP duration)
        {
            if (HitCooldownTimer > 0 || ImmunityTimer > 0)
            {
                return;
            }

            if (f.Unsafe.TryGetPointer(entityRef, out KartBoost* boost))
            {
                boost->Interrupt();
            }

            HitTimer = duration;

            f.Events.OnHitStart(entityRef);
        }

        public Shape3D GetHazardShape()
        {
            // TODO proper shape from kart prototype
            return Shape3D.CreateBox(HitShapeSize / FP._2);
        }
    }
}