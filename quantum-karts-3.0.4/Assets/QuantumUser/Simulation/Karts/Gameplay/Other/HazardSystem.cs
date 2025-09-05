using Photon.Deterministic;

namespace Quantum
{
    public unsafe class HazardSystem : SystemMainThreadFilter<HazardSystem.Filter>, ISignalOnCollision3D
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Hazard* Hazard;
            public Transform3D* Transform3D;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            filter.Hazard->Update(f, filter.Entity);

            if (!filter.Hazard->MarkedForDestruction)
            {
                return;
            }

            if (filter.Hazard->DamageRadius > FP._0)
            {
                var hits = f.Physics3D.OverlapShape(
                    filter.Transform3D->Position,
                    FPQuaternion.Identity,
                    Shape3D.CreateSphere(filter.Hazard->DamageRadius),
                    f.SimulationConfig.KartLayer
                );

                if (hits.Count > 0)
                {
                    for (int i = 0; i < hits.Count; i++)
                    {
                        var hit = hits[i];
                        if (f.Unsafe.TryGetPointer(hit.Entity, out KartHitReceiver* hitReceiver))
                        {
                            hitReceiver->TakeHit(f, hit.Entity, filter.Hazard->HitDuration);
                        }
                    }
                }
            }

            f.Destroy(filter.Entity);
        }

        public void OnCollision3D(Frame f, CollisionInfo3D info)
        {
            if (f.Unsafe.TryGetPointer<Hazard>(info.Entity, out Hazard* hazard) == false)
                return;

            if (!info.IsStatic)
                return;

            if (FPVector3.Dot(info.ContactNormal, FPVector3.Up) < FP._0_75)
            {
                hazard->MarkedForDestruction = true;
            }
        }
    }
}
