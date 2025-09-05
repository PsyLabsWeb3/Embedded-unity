using System;
using Photon.Deterministic;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Quantum
{
    public unsafe partial class RuntimeConfig
    {
        public AssetRef<RaceSettings> RaceSettings;
        public byte AICount;
        public byte DriverCount = 12;

        public bool FillWithAI = true;

        public FP CountdownTime = 5;
        public FP MaxRaceTime = 300;
        public FP FinishingTime = 15;
        public FP FinishedTime = 15;
    }
}
