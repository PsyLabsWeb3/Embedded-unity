using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public unsafe partial struct KartInput
    {
        public void Update(Frame frame, Input input)
        {
            FP steeringDiff = input.Direction.X - PreviousSteering;

            if (FPMath.Abs(steeringDiff) > FP._0_10)
            {
                Steering += FPMath.Sign(steeringDiff) * FP._0_10;
            }
            else
            {
                Steering = input.Direction.X;
            }

            Throttle = input.Direction.Y;
            Drifting = input.Drift;

            FP steeringMagnitude = FPMath.Abs(Steering);

            SameSteeringTime = FPMath.Sign(Steering) == FPMath.Sign(PreviousSteering) ? SameSteeringTime + frame.DeltaTime : 0;

            NoSteeringTime = FPMath.Abs(steeringMagnitude) < FP._0_05 ? NoSteeringTime + frame.DeltaTime : 0;

            DriftingInputTime = Drifting && PreviousDrifting ? DriftingInputTime + frame.DeltaTime : 0;

            PreviousSteering = Steering;
            PreviousDrifting = input.Drift.IsDown;
        }

        public FP GetTotalSteering()
        {
            return Steering + SteeringOffset;
        }
    }
}
