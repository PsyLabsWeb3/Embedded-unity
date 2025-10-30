using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct KartBoost
    {
        public void Update(Frame f)
        {
            if (TimeRemaining <= 0)
            {
                return;
            }

            TimeRemaining -= f.DeltaTime;

            if (TimeRemaining <= 0)
            {
                CurrentBoost = null;
            }
        }

        public void StartBoost(Frame f, AssetRef<BoostConfig> config, EntityRef kartEntity)
        {
            BoostConfig boost = f.FindAsset(config);
            CurrentBoost = config;
            TimeRemaining = boost.Duration;

            f.Events.OnBoostStart(kartEntity, this);
        }

        public void Interrupt()
        {
            CurrentBoost = null;
            TimeRemaining = 0;
        }
    }
}
