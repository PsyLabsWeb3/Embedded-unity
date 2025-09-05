using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public unsafe partial struct KartWeapons
    {
        public void GiveWeapon(Frame f, EntityRef entity, AssetRef<WeaponAsset> weaponAsset)
        {
            if (HeldWeapon != null)
            {
                // don't replace weapons
                return;
            }

            HeldWeapon = weaponAsset;
            RemainingUses = f.FindAsset(HeldWeapon).Uses;

            f.Events.WeaponCollected(entity, this);
        }

        public void UseWeapon(Frame f, KartSystem.Filter filter)
        {
            if (HeldWeapon == null)
            {
                return;
            }

            f.FindAsset(HeldWeapon).Activate(f, filter.Entity);

            if (--RemainingUses <= 0)
            {
                RemoveWeapon();
            }

            f.Events.WeaponUsed(filter.Entity, this);
        }

        public void RemoveWeapon()
        {
            HeldWeapon = null;
        }
    }
}