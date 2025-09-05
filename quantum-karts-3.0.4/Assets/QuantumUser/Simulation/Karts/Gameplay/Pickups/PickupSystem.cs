namespace Quantum
{
    public unsafe class PickupSystem : SystemMainThreadFilter<PickupSystem.Filter>, ISignalOnTriggerEnter3D
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Pickup* Pickup;
        }
        
        public override void Update(Frame frame, ref Filter filter)
        {
            filter.Pickup->Update(frame, filter.Entity);
        }

        public void OnTriggerEnter3D(Frame f, TriggerInfo3D info)
        {
            if (f.Unsafe.TryGetPointer<Kart>(info.Entity, out var kart) == false)
                return;

            if (f.Unsafe.TryGetPointer<Pickup>(info.Other, out var pickup) == false)
                return;

            pickup->OnPickup(f, info.Other, info.Entity);
        }
    }
}
