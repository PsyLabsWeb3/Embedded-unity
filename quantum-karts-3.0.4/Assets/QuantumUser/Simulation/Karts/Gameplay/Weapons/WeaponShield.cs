using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class WeaponShield : WeaponAsset
    {
        public FP duration;
        
        public override void Activate(Frame f, EntityRef sourceKartEntity)
        {
            if (f.Unsafe.TryGetPointer(sourceKartEntity, out KartHitReceiver* hr))
            {
                hr->GiveImmunity(f, sourceKartEntity, duration);
            }
        }

        public override bool AIShouldUse(Frame f, EntityRef aiKartEntity)
        {
            return true;
        }
    }
}
