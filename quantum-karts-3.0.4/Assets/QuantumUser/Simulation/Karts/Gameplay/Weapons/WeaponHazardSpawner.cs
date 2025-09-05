using Photon.Deterministic;
using UnityEngine.Serialization;

namespace Quantum
{
    public unsafe partial class WeaponHazardSpawner : WeaponAsset
    {
        public AssetRef<EntityPrototype> PrototypeRef;
        public FPVector3 LocalSpawnOffset;
        public FPVector3 LocalForceToApply;

        public override void Activate(Frame f, EntityRef sourceKartEntity)
        {
            EntityPrototype prototype = f.FindAsset(PrototypeRef);
            EntityRef spawnedEntity = f.Create(prototype);

            f.Unsafe.TryGetPointer(sourceKartEntity, out Transform3D* kartTransform);

            if (f.Unsafe.TryGetPointer(spawnedEntity, out Transform3D* hazardTransform))
            {
                hazardTransform->Position =
                    kartTransform->Position
                    + kartTransform->TransformDirection(
                        LocalSpawnOffset
                    ); // kartTransform->Forward * LocalSpawnOffset.Z + kartTransform->Up * LocalSpawnOffset.Y;
                hazardTransform->Rotation = FPQuaternion.LookRotation(kartTransform->Forward, FPVector3.Up);
            }

            if (f.Unsafe.TryGetPointer(spawnedEntity, out Hazard* hazard))
            {
                hazard->SpawnedBy = sourceKartEntity;
            }

            if (LocalForceToApply != default && f.Unsafe.TryGetPointer(spawnedEntity, out PhysicsBody3D* physicsBody3D))
            {
                physicsBody3D->AddLinearImpulse(kartTransform->TransformDirection(LocalForceToApply));
            }
        }

        public override bool AIShouldUse(Frame f, EntityRef aiKartEntity)
        {
            return true;
        }
    }
}
