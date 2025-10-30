using Photon.Deterministic;
using UnityEngine.Serialization;

namespace Quantum
{
    public unsafe partial class KartStats : AssetObject
    {
        public FPVector3 overlapShapeSize;
        public FPVector3 overlapShapeOffset;

        public LayerMask overlapLayerMask;

        public FPAnimationCurve acceleration;
        public FPAnimationCurve turningRate;
        public FPAnimationCurve frictionEffect;
        public FP maxSpeed;
        public FP minThrottle;
        public FP gravity;
        public FP drag;
        public FP rotationCorrectionRate;
        public FP rotationSmoothingThreshold;
        public FP maxTilt;
        public FP groundDistance;
    }
}