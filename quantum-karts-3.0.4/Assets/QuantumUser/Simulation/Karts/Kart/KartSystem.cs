using System.Collections.Generic;
using System.Linq;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe class KartSystem : SystemMainThreadFilter<KartSystem.Filter>, ISignalOnPlayerConnected, ISignalOnPlayerDisconnected,
        ISignalOnPlayerAdded, ISignalRaceStateChanged, ISignalPlayerFinished, ISignalOnComponentAdded<RaceProgress>,
        ISignalOnComponentRemoved<RaceProgress>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform3D* Transform3D;
            public Kart* Kart;
            public Wheels* Wheels;
            public KartInput* KartInput;
            public Drifting* Drifting;
            public RaceProgress* RaceProgress;
            public KartHitReceiver* KartHitReceiver;
        }

        public void OnPlayerConnected(Frame frame, PlayerRef player)
        {
            ToggleKartEntityAI(frame, FindPlayerKartEntity(frame, player), false);
        }

        public void OnPlayerDisconnected(Frame frame, PlayerRef player)
        {
            ToggleKartEntityAI(frame, FindPlayerKartEntity(frame, player), true);
        }

        public void RaceStateChanged(Frame frame, RaceState state)
        {
            if (state == RaceState.Waiting)
            {
                SpawnAIDrivers(frame);
                return;
            }

            if (state == RaceState.Countdown && frame.RuntimeConfig.FillWithAI)
            {
                FillWithAI(frame);
                return;
            }
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            Input input = default;

            if (!frame.Unsafe.TryGetPointerSingleton(out Race* race) || (race->CurrentRaceState < RaceState.InProgress))
            {
                if (frame.Unsafe.TryGetPointer(filter.Entity, out PlayerLink* playerLink)
                    && frame.GetPlayerInput(playerLink->Player)->Respawn.WasPressed)
                {
                    playerLink->Ready = !playerLink->Ready;
                    frame.Events.LocalPlayerReady(playerLink->Player, playerLink->Ready);
                }

                return;
            }

            filter.RaceProgress->Update(frame, filter);

            if (frame.Unsafe.TryGetPointer(filter.Entity, out RespawnMover* respawnMover))
            {
                // nothing happens when respawning
                return;
            }

            if (frame.Unsafe.TryGetPointer(filter.Entity, out AIDriver* ai))
            {
                ai->Update(frame, filter, ref input);
            }
            else if (frame.Unsafe.TryGetPointer(filter.Entity, out PlayerLink* playerLink))
            {
                input = *frame.GetPlayerInput(playerLink->Player);
            }

            if (input.Respawn)
            {
                frame.Add<RespawnMover>(filter.Entity);
            }

            if (input.Powerup.WasPressed && frame.Unsafe.TryGetPointer(filter.Entity, out KartWeapons* weapons))
            {
                weapons->UseWeapon(frame, filter);
            }

            filter.KartHitReceiver->Update(frame, filter);

            if (filter.KartHitReceiver->HitTimer > 0)
            {
                input.Direction = FPVector2.Zero;
                filter.Drifting->Direction = 0;
            }

            filter.KartInput->Update(frame, input);
            filter.Wheels->Update(frame);
            filter.Drifting->Update(frame, filter);
            filter.Kart->Update(frame, filter);
        }

        private void ToggleKartEntityAI(Frame frame, EntityRef kartEntity, bool useAI, AssetRef<AIDriverSettings> settings = default)
        {
            if (kartEntity == default) { return; }

            if (useAI)
            {
                AddResult result = frame.Add<AIDriver>(kartEntity);

                if (result != 0)
                {
                    AIDriver* drivingAI = frame.Unsafe.GetPointer<AIDriver>(kartEntity);

                    if (settings == default)
                    {
                        RaceSettings rs = frame.FindAsset(frame.RuntimeConfig.RaceSettings);
                        settings = rs.GetRandomAIConfig(frame);
                    }

                    drivingAI->SettingsRef = settings;
                    drivingAI->UpdateTarget(frame, kartEntity);
                }
            }
            else if (frame.Unsafe.TryGetPointer(kartEntity, out AIDriver* ai))
            {
                frame.Remove<AIDriver>(kartEntity);
            }
        }

        private EntityRef FindPlayerKartEntity(Frame frame, PlayerRef player)
        {
            List<EntityRef> results = new();
            frame.GetAllEntityRefs(results);
            return results.FirstOrDefault(
                e =>
                {
                    if (frame.Unsafe.TryGetPointer(e, out PlayerLink* pl))
                    {
                        return pl->Player == player;
                    }

                    return false;
                }
            );
        }

        private void SpawnAIDriver(Frame frame, AssetRef<AIDriverSettings> driverAsset)
        {
            if (driverAsset == null)
            {
                RaceSettings rs = frame.FindAsset(frame.RuntimeConfig.RaceSettings);
                driverAsset = rs.GetRandomAIConfig(frame);
            }

            var driverData = frame.FindAsset(driverAsset);
            EntityRef kartEntity = SpawnKart(frame, driverData.KartVisuals, driverData.KartStats);
            frame.Add<AIDriver>(kartEntity);

            if (frame.Unsafe.TryGetPointer(kartEntity, out AIDriver* ai) && frame.Unsafe.TryGetPointerSingleton(out Race* race))
            {
                ai->AIIndex = race->SpawnedAIDrivers++;
            }

            ToggleKartEntityAI(frame, kartEntity, true);
        }

        private void SpawnAIDrivers(Frame frame)
        {
            RaceSettings rs = frame.FindAsset(frame.RuntimeConfig.RaceSettings);
            byte count = frame.RuntimeConfig.AICount;

            for (int i = 0; i < count; i++)
            {
                SpawnAIDriver(frame, rs.GetRandomAIConfig(frame));
            }
        }

        private void FillWithAI(Frame frame)
        {
            int playerCount = frame.ComponentCount<Kart>();
            int missingDrivers = frame.RuntimeConfig.DriverCount - playerCount;

            if (missingDrivers <= 0)
            {
                return;
            }

            RaceSettings rs = frame.FindAsset(frame.RuntimeConfig.RaceSettings);

            for (int i = 0; i < missingDrivers; i++)
            {
                SpawnAIDriver(frame, rs.GetRandomAIConfig(frame));
            }
        }

        public void OnPlayerAdded(Frame frame, PlayerRef player, bool firstTime)
        {
            var data = frame.GetPlayerData(player);

            EntityRef kartEntity = SpawnKart(frame, data.KartVisuals, data.KartStats);

            var playerLink = new PlayerLink()
            {
                Player = player,
                Ready = false
            };
            frame.Add(kartEntity, playerLink);
        }

        private EntityRef SpawnKart(Frame frame, AssetRef<KartVisuals> visuals, AssetRef<KartStats> stats)
        {
            int driverIndex = frame.ComponentCount<Kart>();

            RaceSettings settings = frame.FindAsset(frame.RuntimeConfig.RaceSettings);
            var prototype = frame.FindAsset(settings.KartPrototype);
            var entity = frame.Create(prototype);

            frame.Unsafe.TryGetPointerSingleton<RaceTrack>(out RaceTrack* track);

            if (frame.Unsafe.TryGetPointer<Transform3D>(entity, out var transform))
            {
                track->GetStartPosition(frame, driverIndex, out FPVector3 pos, out FPQuaternion rot);
                transform->Position = pos;
                transform->Rotation = rot;
            }

            if (frame.Unsafe.TryGetPointer(entity, out RaceProgress* raceProgress))
            {
                raceProgress->Initialize(track->TotalLaps);
            }

            Kart* kartComp = frame.Unsafe.GetPointer<Kart>(entity);

            kartComp->VisualAsset = visuals.Id;
            kartComp->StatsAsset = stats.Id;

            return entity;
        }

        public void PlayerFinished(Frame f, EntityRef entity)
        {
            ToggleKartEntityAI(f, entity, true);
        }

        public void OnAdded(Frame f, EntityRef entity, RaceProgress* component)
        {
            component->LapTimes = f.AllocateList<FP>();
        }

        public void OnRemoved(Frame f, EntityRef entity, RaceProgress* component)
        {
            f.FreeList(component->LapTimes);
        }
    }
}
