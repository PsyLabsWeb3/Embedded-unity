using Photon.Deterministic;
using System.Linq;

namespace Quantum
{
    public unsafe partial struct DriftBoost
    {
        public void Update(Frame f, BoostSystem.Filter filter)
        {
            if (filter.Drifting->IsDrifting)
            {
                DriftTime += f.DeltaTime;
            }
            else if (DriftTime > 0)
            {
                // linq can't access local fields
                FP driftTime = DriftTime;
                RaceSettings settings = f.FindAsset(f.RuntimeConfig.RaceSettings);
                DriftBoostLevel driftBoost = settings.DriftBoosts.LastOrDefault(cfg => cfg.MinDriftTime < driftTime);

                if (driftBoost.BoostAsset != null && f.Unsafe.TryGetPointer(filter.Entity, out KartBoost* kartBoost))
                {
                    kartBoost->StartBoost(f, driftBoost.BoostAsset, filter.Entity);
                }

                DriftTime = 0;
            }
        }

        public void Interrupt()
        {
            DriftTime = 0;
        }
    }
}
