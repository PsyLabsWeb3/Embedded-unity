using System;

namespace Quantum
{
    [Serializable]
    public partial class WeaponSelection : AssetObject
    {
        public AssetRef<WeaponAsset>[] WeaponAssets;
    }
}