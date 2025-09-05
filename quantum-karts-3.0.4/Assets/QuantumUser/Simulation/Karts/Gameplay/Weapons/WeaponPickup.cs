using System;
using Photon.Deterministic;

namespace Quantum
{
    [Serializable]
    public unsafe partial class WeaponPickup : PickupAsset
    {
        public AssetRef<WeaponAsset> PredeterminedWeapon;

        public override void OnPickup(Frame f, EntityRef kartEntity)
        {
            if (f.Unsafe.TryGetPointer(kartEntity, out KartWeapons* kartWeapons))
            {
                kartWeapons->GiveWeapon(f, kartEntity, GetWeaponAsset(f));
            }
        }

        public AssetRef<WeaponAsset> GetWeaponAsset(Frame f)
        {
            RaceSettings rs = f.FindAsset(f.RuntimeConfig.RaceSettings);
            return PredeterminedWeapon != null ? PredeterminedWeapon : rs.GetRandomWeapon(f);
        }

        public override FP GetRespawnTime(Frame f)
        {
            RaceSettings rs = f.FindAsset(f.RuntimeConfig.RaceSettings);
            return rs.PickupRespawnTime;
        }
    }
}