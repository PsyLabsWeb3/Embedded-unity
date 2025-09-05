using System;
using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public unsafe class KartRespawnSystem : SystemMainThreadFilter<KartRespawnSystem.Filter>, ISignalOnTriggerEnter3D
    {
        public struct Filter
        {
            public EntityRef Entity;
            public RespawnMover* Mover;
            public Transform3D* Transform3D;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (!filter.Mover->Initialized)
            {
                RespawnKart(frame, filter.Entity);
            }

            filter.Mover->Update(frame, filter.Transform3D);

            if (filter.Mover->Progress >= FP._1)
            {
                frame.Remove<RespawnMover>(filter.Entity);
            }
        }

        public void OnTriggerEnter3D(Frame f, TriggerInfo3D info)
        {
            if (!info.IsStatic || info.StaticData.Layer != 12)
            {
                return;
            }

            RespawnKart(f, info.Entity);
        }

        public void RespawnKart(Frame f, EntityRef kartEntity)
        {
            if (f.Unsafe.TryGetPointer<RaceProgress>(kartEntity, out var playerProgress) == false)
                return;

            if (!f.Unsafe.TryGetPointerSingleton(out RaceTrack* track))
            {
                return;
            }

            if (!f.Unsafe.TryGetPointer(kartEntity, out RespawnMover* mover))
            {
                f.Add<RespawnMover>(kartEntity);
            }

            if (f.Unsafe.TryGetPointer(kartEntity, out mover))
            {
                Transform3D* kartTransform = f.Unsafe.GetPointer<Transform3D>(kartEntity);

                FPVector3 pos = track->GetCheckpointTargetPosition(f, playerProgress->TargetCheckpointIndex - 1, FP._0_50);
                FPQuaternion rot = FPQuaternion.LookRotation(track->GetCheckpointForward(f, playerProgress->TargetCheckpointIndex - 1), FPVector3.Up);

                mover->StartPos = kartTransform->Position;
                mover->StartRot = kartTransform->Rotation;
                mover->EndPos = pos;
                mover->EndRot = rot;

                mover->Initialized = true;
            }
        }
    }
}
