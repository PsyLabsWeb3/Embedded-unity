using System;
using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public unsafe class KartCollisionQuerySystem : SystemMainThreadFilter<KartCollisionQuerySystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Kart* KartComp;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (frame.Unsafe.TryGetPointer(filter.Entity, out Transform3D* transform))
            {
                KartStats stats = frame.FindAsset(filter.KartComp->StatsAsset);
                
                filter.KartComp->OverlapQuery = frame.Physics3D.AddOverlapShapeQuery(
                    transform->Position + stats.overlapShapeOffset + filter.KartComp->Velocity * frame.DeltaTime,
                    transform->Rotation,
                    filter.KartComp->GetOverlapShape(frame),
                    stats.overlapLayerMask,
                    QueryOptions.HitAll | QueryOptions.ComputeDetailedInfo
                );
            }
        }
    }
}
