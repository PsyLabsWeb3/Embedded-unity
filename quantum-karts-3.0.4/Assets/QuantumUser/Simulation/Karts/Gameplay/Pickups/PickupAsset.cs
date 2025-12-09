using System;
using Photon.Deterministic;

namespace Quantum
{
    public abstract partial class PickupAsset: AssetObject
    {
        public abstract FP GetRespawnTime(Frame f);
        public abstract void OnPickup(Frame f, EntityRef kartEntity);
    }
}
