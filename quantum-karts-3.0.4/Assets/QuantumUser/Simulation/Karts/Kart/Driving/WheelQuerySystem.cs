using System;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe class WheelQuerySystem : SystemMainThreadFilter<WheelQuerySystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Wheels* Wheels;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (!frame.Unsafe.TryGetPointer(filter.Entity, out Transform3D* kartTransform))
            {
                throw new Exception("Kart transform not found!");
            }

            Wheels* wheels = filter.Wheels;
            FPVector3 up = kartTransform->Up;

            for (int i = 0; i < wheels->Configs.Length; i++)
            {
                WheelConfig wheelConf = wheels->Configs[i];
                FPVector3 offset = up.Normalized * wheelConf.Height;

                wheels->WheelQueries[i] = frame.Physics3D.AddRaycastQuery(
                    kartTransform->TransformPoint(wheelConf.Position) + offset,
                    -up,
                    wheelConf.Height * 2,
                    true,
                    wheels->DriveableMask,
                    QueryOptions.ComputeDetailedInfo | QueryOptions.HitStatics
                );
            }
        }
    }
}
