using System.Collections.Generic;
using System.Linq;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe class BoostSystem : SystemMainThreadFilter<BoostSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public DriftBoost* DriftBoost;
            public KartBoost* KartBoost;
            public Drifting* Drifting;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            filter.KartBoost->Update(frame);
            filter.DriftBoost->Update(frame, filter);
        }
    }
}
