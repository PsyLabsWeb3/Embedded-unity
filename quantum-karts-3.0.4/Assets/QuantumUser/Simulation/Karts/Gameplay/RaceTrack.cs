using System.Collections.Generic;
using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum
{
    public unsafe partial struct RaceTrack
    {
        public QList<EntityRef> GetCheckpoints(Frame frame)
        {
            return frame.ResolveList(Checkpoints);
        }

        public void GetStartPosition(Frame frame, int playerIndex, out FPVector3 pos, out FPQuaternion rot)
        {
            var positions = frame.ResolveList(StartPositions);
            frame.Unsafe.TryGetPointer(positions[playerIndex], out Transform3D* transform3D);
            pos = transform3D->Position;
            rot = transform3D->Rotation;
        }

        public EntityRef GetCheckpointEntity(Frame frame, int index)
        {
            var checkpoints = GetCheckpoints(frame);

            if (index < 0)
            {
                return checkpoints[^-index];
            }

            if (index > checkpoints.Count)
            {
                return checkpoints[index % checkpoints.Count];
            }

            return checkpoints[index];
        }

        public Checkpoint* GetCheckpoint(Frame frame, int index, out EntityRef entity)
        {
            entity = GetCheckpointEntity(frame, index);
            return frame.Unsafe.GetPointer<Checkpoint>(entity);
        }

        public FPVector3 GetCheckpointForward(Frame frame, int index)
        {
            GetCheckpoint(frame, index, out EntityRef entity);
            Transform3D* transform3D = frame.Unsafe.GetPointer<Transform3D>(entity);
            return transform3D->Forward;
        }

        public FPVector3 GetCheckpointTargetPosition(Frame frame, int index, FP quality)
        {
            Checkpoint* checkpoint = GetCheckpoint(frame, index, out EntityRef entity);
            Transform3D* transform3D = frame.Unsafe.GetPointer<Transform3D>(entity);
            FPVector3 target = FPVector3.Lerp(checkpoint->AIBadLocalPosition, checkpoint->AIOptimalLocalPosition, quality);
            target += frame.Global->RngSession.Next(-FP._3, FP._3) * transform3D->Right;
            return transform3D->TransformPoint(target);
        }
    }
}
