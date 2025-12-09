using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public unsafe partial struct Hazard
    {
        public void Update(Frame f, EntityRef entityRef)
        {
            if (TotalTime >= TimeAlive)
            {
                MarkedForDestruction = true;
            }
            else
            {
                TotalTime += f.DeltaTime;
            }
        }
    }
}
