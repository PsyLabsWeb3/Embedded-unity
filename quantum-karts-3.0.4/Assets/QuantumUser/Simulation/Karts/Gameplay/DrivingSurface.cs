using Photon.Deterministic;
using System;

namespace Quantum
{
    [Serializable]
    public unsafe partial class DrivingSurface : AssetObject
    {
        public FP SpeedMultiplier = 1;
        public FP FrictionMultiplier = 1;
        public FP DriftingFrictionMultiplier = 1;
        public FP HandlingMultiplier = 1;
        public bool Offroad = false;
        public SurfaceEffect Effect = SurfaceEffect.None;
    }
}
