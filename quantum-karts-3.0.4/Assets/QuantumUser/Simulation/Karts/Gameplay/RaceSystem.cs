using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public unsafe class RaceSystem : SystemMainThread, ISignalOnTriggerEnter3D
    {
        private List<ProgressWrapper> ProgressWrappers = new(32);
        private RaceProgressComparer raceProgressComparer = new();

        public struct Filter
        {
            public EntityRef Entity;
            public RaceProgress* RaceProgress;
        }

        private struct ProgressWrapper
        {
            public RaceProgress* RaceProgress;
            public EntityRef Entity;
        }

        public override void Update(Frame frame)
        {
            Race* race = frame.Unsafe.GetPointerSingleton<Race>();

            UpdatePositions(frame);

            race->Update(frame);
        }

        public void OnTriggerEnter3D(Frame f, TriggerInfo3D info)
        {
            if (f.Unsafe.TryGetPointer<RaceProgress>(info.Entity, out var playerProgress) == false)
                return;

            if (f.Unsafe.TryGetPointer<Checkpoint>(info.Other, out var checkpoint) == false)
                return;

            Race* race = f.Unsafe.GetPointerSingleton<Race>();

            f.Unsafe.TryGetPointerSingleton<RaceTrack>(out RaceTrack* track);

            bool alreadyFinished = playerProgress->Finished;

            if (playerProgress->CheckpointReached(f, checkpoint, info.Entity, out bool lapCompleted))
            {
                if (f.Unsafe.TryGetPointer(info.Entity, out AIDriver* drivingAI))
                {
                    drivingAI->UpdateTarget(f, info.Entity);
                }
            }

            if (alreadyFinished)
            {
                return;
            }

            if (lapCompleted) { f.Events.OnPlayerCompletedLap(info.Entity); }

            if (playerProgress->Finished)
            {
                f.Events.OnPlayerFinished(info.Entity);
                f.Signals.PlayerFinished(info.Entity);
            }

            if (playerProgress->Finished && race->CurrentRaceState == RaceState.InProgress)
            {
                FirstPlayerFinished(f, info.Entity);
            }
        }

        private void FirstPlayerFinished(Frame f, EntityRef kartEntity)
        {
            Race* race = f.Unsafe.GetPointerSingleton<Race>();
            race->ChangeState(f, RaceState.Finishing);
            race->StateTimer = FrameTimer.FromSeconds(f, f.RuntimeConfig.FinishingTime);
            f.Events.OnFirstPlayerFinish(kartEntity);
        }

        private void UpdatePositions(Frame f)
        {
            Race* race = f.Unsafe.GetPointerSingleton<Race>();

            if (f.Number % race->PositionCalcInterval != 0) { return; }

            ProgressWrappers.Clear();

            f.Unsafe.TryGetPointerSingleton(out RaceTrack* raceTrack);

            foreach (var pair in f.Unsafe.GetComponentBlockIterator<RaceProgress>())
            {
                pair.Component->UpdateDistanceToCheckpoint(f, pair.Entity, raceTrack);
                ProgressWrappers.Add(new() { RaceProgress = pair.Component, Entity = pair.Entity });
            }

            ProgressWrappers.Sort(raceProgressComparer);

            for (int i = 0; i < ProgressWrappers.Count; i++)
            {
                ProgressWrappers[i].RaceProgress->SetRacePosition((sbyte)(i + 1));
            }

            f.Events.OnPositionsUpdated();
        }

        private class RaceProgressComparer : IComparer<ProgressWrapper>
        {
            int IComparer<ProgressWrapper>.Compare(ProgressWrapper A, ProgressWrapper B)
            {
                FP aTime = A.RaceProgress->GetFinishTime();
                FP bTime = B.RaceProgress->GetFinishTime();

                // both finished
                if (aTime > 0 && bTime > 0)
                    return aTime.CompareTo(bTime);

                // other finished
                if (aTime > 0 != bTime > 0)
                    return aTime > 0 ? -1 : 1;

                // negate lap and checkpoint index comparisons because higher better

                int lapResult = A.RaceProgress->CurrentLap.CompareTo(B.RaceProgress->CurrentLap);
                if (lapResult != 0) return -lapResult;

                int checkpointResult = A.RaceProgress->TargetCheckpointIndex.CompareTo(B.RaceProgress->TargetCheckpointIndex);
                if (checkpointResult != 0) return -checkpointResult;

                int distanceResult = A.RaceProgress->DistanceToCheckpoint.CompareTo(B.RaceProgress->DistanceToCheckpoint);
                return distanceResult != 0 ? distanceResult : -1;
            }
        }
    }
}
