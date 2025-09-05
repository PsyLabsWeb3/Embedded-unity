using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public unsafe partial struct AIDriver
    {
        public void Update(Frame frame, KartSystem.Filter filter, ref Input input)
        {
            AIDriverSettings settings = frame.FindAsset(SettingsRef);

            FP distance = FPVector3.Distance(TargetLocation, filter.Transform3D->Position);
            FP distanceNext = FPVector3.Distance(TargetLocation, NextTargetLocation);
            FP predictionAmount = FPMath.InverseLerp(distance, distanceNext, settings.PredictionRange);

            FPVector3 toWaypoint = TargetLocation - filter.Transform3D->Position;
            FPVector3 toNextWaypoint = NextTargetLocation - filter.Transform3D->Position;

            FPVector3 flatVelocity = filter.Kart->Velocity;
            flatVelocity.Y = 0;
            toWaypoint.Y = 0;
            toNextWaypoint.Y = 0;

            StationaryTime = flatVelocity.SqrMagnitude < FP._7 ? StationaryTime + frame.DeltaTime : 0;

            if (StationaryTime > 5)
            {
                input.Respawn = true;
                StationaryTime = 0;
            }

            if (frame.Unsafe.TryGetPointer(filter.Entity, out KartWeapons* weapons))
            {
                LastWeaponTime += frame.DeltaTime;

                if (weapons->HeldWeapon != default
                    && LastWeaponTime > FP._0_50
                    && frame.FindAsset(weapons->HeldWeapon).AIShouldUse(frame, filter.Entity))
                {
                    input.Powerup = true;
                }
            }

            //Draw.Ray(filter.Transform3D->Position, toWaypoint, ColorRGBA.Green);
            //Draw.Ray(filter.Transform3D->Position, toNextWaypoint, ColorRGBA.Blue);

            FPVector3 targetDirection = FPVector3.Lerp(toWaypoint, toNextWaypoint, predictionAmount).Normalized;

            //Draw.Ray(filter.Transform3D->Position, targetDirection * 5, ColorRGBA.Magenta);

            FP turnAngle = FPVector3.Angle(toWaypoint, toNextWaypoint);
            FP signedAngle = FPVector3.SignedAngle(targetDirection, flatVelocity, FPVector3.Up);
            FP desiredDirection = FPMath.Sign(signedAngle);

            if (frame.Unsafe.TryGetPointer(filter.Entity, out Drifting* drifting))
            {
                bool shouldStartDrift = turnAngle >= settings.DriftingAngle && !drifting->IsDrifting;
                bool shouldEndDrift = turnAngle < settings.DriftingStopAngle && drifting->IsDrifting;

                input.Drift = !drifting->IsDrifting && shouldStartDrift || drifting->IsDrifting && shouldEndDrift;
            }

            FP steeringStrength = settings.SteeringCurve.Evaluate(FPMath.Abs(signedAngle));

            input.Direction = new FPVector2(FPMath.Clamp(-desiredDirection * steeringStrength, -1, 1), 1);
        }

        public void UpdateTarget(Frame frame, EntityRef entity)
        {
            RaceTrack* raceTrack = frame.Unsafe.GetPointerSingleton<RaceTrack>();
            RaceProgress* raceProgress = frame.Unsafe.GetPointer<RaceProgress>(entity);

            AIDriverSettings settings = frame.FindAsset(SettingsRef);

            TargetLocation = raceTrack->GetCheckpointTargetPosition(frame, raceProgress->TargetCheckpointIndex, settings.Difficulty);

            int nextIndex = raceProgress->TargetCheckpointIndex + 1;

            if (nextIndex >= raceTrack->GetCheckpoints(frame).Count)
            {
                nextIndex = 0;
            }

            NextTargetLocation = raceTrack->GetCheckpointTargetPosition(frame, nextIndex, settings.Difficulty);
        }
    }
}
