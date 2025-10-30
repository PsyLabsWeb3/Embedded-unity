using System;
using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    [Serializable]
    unsafe partial class WeaponDebug : WeaponAsset
    {
        public override void Activate(Frame f, EntityRef sourceKartEntity)
        {
            Debug.Log("Activate debug weapon");
        }

        public override bool AIShouldUse(Frame f, EntityRef aiKartEntity)
        {
            return true;
        }
    }
}