using Photon.Deterministic;
using Quantum.Physics3D;
using Quantum.Prototypes;
using UnityEngine;

namespace Quantum
{
    public unsafe partial struct Wheels
    {
        public void Update(Frame frame)
        {
            for (int i = 0; i < WheelStatuses.Length; i++)
            {
                WheelStatus* wheelStatus = WheelStatuses.GetPointer(i);

                var hits = frame.Physics3D.GetQueryHits(WheelQueries[i]);

                wheelStatus->Grounded = false;

                if (hits.Count > 0)
                {
                    Hit3D hit = hits[0]; // only 1 hit

                    //Draw.Box(hit.Point, new FPVector3(FP._0_05, FP._0_05, FP._0_05));
                    //Draw.Line(hit.Point, hit.Point + hit.Normal, ColorRGBA.Green);

                    if (hit.Normal.Y >= GroundedMinimumYFactor)
                    {
                        DrivingSurface surface = frame.FindAsset<DrivingSurface>(hit.GetStaticCollider(frame).StaticData.Asset.Id);
                        
                        wheelStatus->Grounded = true;
                        wheelStatus->HitNormal = hit.Normal;
                        wheelStatus->HitPoint = hit.Point;
                        wheelStatus->HitSurface = surface;
                    }
                }
            }
        }
    }
}
