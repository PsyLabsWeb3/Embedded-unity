using System;
using Photon.Deterministic;
using Quantum;
using UnityEngine;
using UnityEngine.Serialization;

namespace Quantum
{
    [Serializable]
    public unsafe partial class AIDriverSettings : AssetObject
    {
        public FP PredictionRange;
        public FP Difficulty;
        public FP DriftingAngle;
        public FP DriftingStopAngle;
        public FPAnimationCurve SteeringCurve;
        public AssetRef<KartVisuals> KartVisuals;
        public AssetRef<KartStats> KartStats;
        public string Nickname;
    }
}
