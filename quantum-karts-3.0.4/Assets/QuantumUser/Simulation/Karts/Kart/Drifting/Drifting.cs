using Photon.Deterministic;
using Quantum.Prototypes;
using UnityEngine;

namespace Quantum
{
    public unsafe partial struct Drifting
    {
        public bool IsDrifting => Direction != 0;

        public void Update(Frame frame, KartSystem.Filter filter)
        {
            Kart* kart = filter.Kart;
            Transform3D* transform = filter.Transform3D;
            int desiredDirection = FPMath.RoundToInt(FPMath.Sign(filter.KartInput->Steering));

            if (CanStartDrift(filter.KartInput, kart, desiredDirection))
            {
                Direction = desiredDirection;
            }
            else if (IsDrifting && ShouldEndDrift(filter.KartInput, kart))
            {
                Direction = 0;
            }

            FPVector3 accelerationDirection = FPVector3.Lerp(transform->Right * -Direction, transform->Forward, ForwardFactor);

            filter.KartInput->SteeringOffset = MaxSteeringOffset * Direction;

            if (IsDrifting)
            {
                kart->ExternalForce += accelerationDirection * SideAcceleration * filter.KartInput->Throttle;
            }
        }

        private bool CanStartDrift(KartInput* kartInput, Kart* kart, int desiredDirection)
        {
            if (desiredDirection == Direction) { return false; }

            if (!kartInput->Drifting.WasPressed) { return false; }

            if (kart->AirTime > MaxAirTime) { return false; }

            if (kart->Velocity.SqrMagnitude < MinimumSpeed * MinimumSpeed) { return false; }

            if (FPMath.Abs(kartInput->Steering) < FP._0_05) { return false; }

            if (kart->IsOffroad) { return false; }

            return true;
        }

        private bool ShouldEndDrift(KartInput* kartInput, Kart* kart)
        {
            if (kart->IsOffroad) { return true; }

            if (kartInput->Drifting.WasPressed) { return true; }

            if (kart->AirTime > MaxAirTime) { return true; }

            if (kart->Velocity.SqrMagnitude < MinimumSpeed * MinimumSpeed) { return true; }

            if (kart->SidewaysSpeedSqr < MinSidewaysSpeedSqr && kartInput->NoSteeringTime > MaxNoSteerTime)
            {
                return true;
            }

            if (FPMath.Sign(kartInput->Steering) != Direction && kartInput->SameSteeringTime > MaxOppositeSteerTime)
            {
                return true;
            }

            return false;
        }
    }
}
