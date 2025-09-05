namespace Quantum
{
    public unsafe partial class WeaponBoost : WeaponAsset
    {
        public AssetRef<BoostConfig> GivenBoostConfig;
        
        public override void Activate(Frame f, EntityRef sourceKartEntity)
        {
            if (f.Unsafe.TryGetPointer(sourceKartEntity, out KartBoost* kartBoost))
            {
                kartBoost->StartBoost(f, GivenBoostConfig, sourceKartEntity);
            }
        }

        public override bool AIShouldUse(Frame f, EntityRef aiKartEntity)
        {
            return true;
        }
    }
}
