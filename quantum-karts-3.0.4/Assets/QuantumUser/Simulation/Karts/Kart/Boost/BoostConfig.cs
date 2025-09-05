using System;
using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    [Serializable]
    public unsafe partial class BoostConfig : AssetObject
    {
        public FP Duration;
        public FP AccelerationBonus;
        public FP MaxSpeedBonus;
        
        public Color ExhaustColor;
    }
}
