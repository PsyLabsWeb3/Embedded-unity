using System;
using Photon.Deterministic;

namespace Quantum
{
    [Serializable]
    public unsafe partial class RaceSettings : AssetObject
    {
        public AssetRef<EntityPrototype> KartPrototype;
        
        public FP PickupRespawnTime = FP._1;

        public FP KartRespawnTime = FP._1;


        public WeaponAsset[] WeaponAssets;

        public AssetRef<AIDriverSettings>[] AIPlayerDatas;

        public DriftBoostLevel[] DriftBoosts;

        public AssetRef<AIDriverSettings> GetRandomAIConfig(Frame f)
        {
            return AIPlayerDatas[f.Global->RngSession.Next(0, AIPlayerDatas.Length)];
        }
        
        public WeaponAsset GetRandomWeapon(Frame f)
        {
            return WeaponAssets[f.Global->RngSession.Next(0, WeaponAssets.Length)];
        }
    }
}
