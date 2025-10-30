using UnityEngine;

namespace Quantum
{
    public abstract partial class WeaponAsset :AssetObject
    {
        [Header("Unity")]
        public Sprite WeaponSprite;

        public string WeaponName;

        /// <summary>
        /// How many times weapon can be used in total
        /// </summary>
        [Header("Quantum")]
        public byte Uses = 1;

        /// <summary>
        /// Activates the weapon
        /// </summary>
        /// <param name="f">Game frame</param>
        /// <param name="sourceKartEntity">Kart entity which used the weapon</param>
        public abstract void Activate(Frame f, EntityRef sourceKartEntity);

        /// <summary>
        /// Contains weapon specific AI behaviour
        /// </summary>
        /// <param name="f">Game frame</param>
        /// <param name="aiKartEntity">AI kart entity</param>
        /// <returns>Whether or not the AI driver should activate the weapon this frame</returns>
        public abstract bool AIShouldUse(Frame f, EntityRef aiKartEntity);
    }
}