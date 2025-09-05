using System;
using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public unsafe class KartHitQuerySystem : SystemMainThreadFilter<KartHitQuerySystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public KartHitReceiver* HitReceiver;
            public Kart* Kart;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (frame.Unsafe.TryGetPointer(filter.Entity, out Transform3D* transform))
            {
                KartStats stats = frame.FindAsset(filter.Kart->StatsAsset);
                
                filter.HitReceiver->HazardQuery = frame.Physics3D.AddOverlapShapeQuery(
                    transform->Position + stats.overlapShapeOffset + filter.Kart->Velocity * frame.DeltaTime,
                    transform->Rotation,
                    filter.HitReceiver->GetHazardShape(),
                    filter.HitReceiver->HazardLayerMask
                );
            }
        }
    }
}
