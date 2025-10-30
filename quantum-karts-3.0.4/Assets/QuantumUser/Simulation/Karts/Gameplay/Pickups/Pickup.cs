namespace Quantum
{
    public unsafe partial struct Pickup
    {
        public void Update(Frame f, EntityRef entity)
        {
            if (RespawnTimer <= 0) { return; }

            RespawnTimer -= f.DeltaTime;

            if (RespawnTimer <= 0)
            {
                TogglePickedUp(f, entity, false);

                f.Events.OnPickupRespawn(entity);
            }
        }

        public void OnPickup(Frame f, EntityRef thisEntity, EntityRef kartEntity)
        {
            if (PickedUp) { return; }

            PickupAsset asset = f.FindAsset(Asset);

            asset.OnPickup(f, kartEntity);
            RespawnTimer = asset.GetRespawnTime(f);

            TogglePickedUp(f, thisEntity, true);

            f.Events.OnPickupCollected(thisEntity);
        }

        private void TogglePickedUp(Frame f, EntityRef entity, bool pickedUp)
        {
            PhysicsCollider3D* coll = f.Unsafe.GetPointer<PhysicsCollider3D>(entity);
            PickedUp = pickedUp;
        }
    }
}