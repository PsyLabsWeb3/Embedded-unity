using Photon.Deterministic;
using Quantum.Physics3D;
using Quantum.Prototypes;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Quantum
{
    public unsafe partial struct RaceProgress
    {
        public void Initialize(sbyte totalLaps)
        {
            TotalLaps = totalLaps;
            LapTimer = 0;
            CurrentLap = 0;
        }

        public void StartRace()
        {
            LapTimer = 0;
            CurrentLap = 0;
        }

        public FP GetFinishTime()
        {
            if (!Finished) { return -1; }

            return FinishTime;
        }

        public void Update(Frame frame, KartSystem.Filter filter)
        {
            LapTimer += frame.DeltaTime;
        }

        public void SetRacePosition(sbyte position)
        {
            Position = position;
        }

        public void UpdateDistanceToCheckpoint(Frame f, EntityRef entity, RaceTrack* raceTrack)
        {
            var checkpoint = raceTrack->GetCheckpointEntity(f, TargetCheckpointIndex);

            Transform3D* targetTransform = f.Unsafe.GetPointer<Transform3D>(checkpoint);
            Transform3D* ownTransform = f.Unsafe.GetPointer<Transform3D>(entity);

            DistanceToCheckpoint = FPVector3.Distance(targetTransform->Position, ownTransform->Position);
        }

        public bool CheckpointReached(Frame frame, Checkpoint* checkpoint, EntityRef entity, out bool lap)
        {
            lap = false;

            bool wrongWay = (checkpoint->Index < TargetCheckpointIndex - 1) ||
                            (checkpoint->Index > TargetCheckpointIndex && !checkpoint->Finish);

            if (wrongWay != LastWrongWay)
            {
                LastWrongWay = wrongWay;
                AlertWrongWay(frame, entity);
            }

            if (wrongWay)
            {
                return false;
            }

            if (checkpoint->Index == TargetCheckpointIndex)
            {
                TargetCheckpointIndex = (sbyte)(checkpoint->Finish ? 0 : TargetCheckpointIndex + 1);

                if (!checkpoint->Finish || Finished)
                {
                    return true;
                }

                var times = frame.ResolveList(LapTimes);
                times.Add(LapTimer);
                LapTimer = 0;

                lap = true;

                if (++CurrentLap >= TotalLaps)
                {
                    FinishTime = 0;
                    for (int i = 0; i < times.Count; i++)
                    {
                        FinishTime += times[i];
                    }

                    Finished = true;
                }

                return true;
            }

            if (TargetCheckpointIndex != 0)
            {
                Debug.LogWarning("Invalid checkpoint case");
            }
            return false;
        }

        private void AlertWrongWay(Frame f, EntityRef entity)
        {
            if (f.Unsafe.TryGetPointer(entity, out PlayerLink* link))
            {
                f.Events.OnWrongWayChanged(link->Player, LastWrongWay);
            }
        }
    }
}