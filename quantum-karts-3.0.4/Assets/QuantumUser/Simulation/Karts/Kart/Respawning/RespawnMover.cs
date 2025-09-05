using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct RespawnMover
    {
        public void Update(Frame f, Transform3D* transform3D)
        {
            RaceSettings settings = f.FindAsset(f.RuntimeConfig.RaceSettings);
            Progress = FPMath.Min(Progress + f.DeltaTime / settings.KartRespawnTime, FP._1);

            transform3D->Position = FPVector3.Lerp(StartPos, EndPos, Progress);
            transform3D->Rotation = FPQuaternion.Lerp(StartRot, EndRot, Progress);
        }
    }
}
