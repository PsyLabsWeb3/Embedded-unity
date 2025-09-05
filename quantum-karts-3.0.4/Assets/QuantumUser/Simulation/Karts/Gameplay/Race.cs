using System.Collections.Generic;
using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public unsafe partial struct Race
    {
        public void Update(Frame frame)
        {
            CurrentStateTime += frame.DeltaTime;

            switch (CurrentRaceState)
            {
                case RaceState.None:
                    InitializeRace(frame);
                    break;
                case RaceState.Waiting:
                    Update_Waiting(frame);
                    break;
                case RaceState.Countdown:
                    Update_Countdown(frame);
                    break;
                case RaceState.InProgress:
                    Update_InProgress(frame);
                    break;
                case RaceState.Finishing:
                    Update_Finishing(frame);
                    break;
                case RaceState.Finished:
                    Update_Finished(frame);
                    break;
                default:
                    Debug.LogError("Unknown racestate");
                    break;
            }
        }

        public void ChangeState(Frame f, RaceState state)
        {
            CurrentRaceState = state;
            CurrentStateTime = FP._0;

            f.Events.OnRaceStateChanged(state);
            f.Signals.RaceStateChanged(state);
        }

        private void InitializeRace(Frame f)
        {
            ChangeState(f, RaceState.Waiting);
        }

        private void StartCountdown(Frame f)
        {
            ChangeState(f, RaceState.Countdown);

            StateTimer = FrameTimer.FromSeconds(f, f.RuntimeConfig.CountdownTime);
            f.Events.OnCountdownStart(f.RuntimeConfig.CountdownTime);
        }

        private void StartRace(Frame f)
        {
            ChangeState(f, RaceState.InProgress);

            foreach (var pair in f.Unsafe.GetComponentBlockIterator<RaceProgress>())
            {
                pair.Component->StartRace();
                f.Events.OnPlayerCompletedLap(pair.Entity);
            }

            StateTimer = FrameTimer.FromSeconds(f, f.RuntimeConfig.MaxRaceTime);
            f.Events.OnRaceStart();
        }

        private void RaceOver(Frame f)
        {
            ChangeState(f, RaceState.Finished);
            StateTimer = FrameTimer.FromSeconds(f, f.RuntimeConfig.FinishedTime);
            f.Events.OnRaceOver();
        }

        private void Update_Waiting(Frame f)
        {
            bool allReady = true;
            int playerCount = GetPlayerCount(f);

            foreach (var pair in f.Unsafe.GetComponentBlockIterator<PlayerLink>())
            {
                if (!pair.Component->Ready)
                {
                    allReady = false;
                }
            }

            if ((allReady && playerCount > 0) || playerCount == f.RuntimeConfig.DriverCount)
            {
                StartCountdown(f);
            }
        }

        private void Update_Countdown(Frame f)
        {
            if (StateTimer.IsRunning(f) == false)
            {
                StartRace(f);
            }
        }

        private void Update_InProgress(Frame f)
        {
            if (StateTimer.IsRunning(f) == false)
            {
                RaceOver(f);
            }
        }

        private void Update_Finishing(Frame f)
        {
            bool allFinished = true;

            foreach (var pair in f.Unsafe.GetComponentBlockIterator<RaceProgress>())
            {
                if (!pair.Component->Finished)
                {
                    allFinished = false;
                }
            }

            if (allFinished || StateTimer.IsRunning(f) == false)
            {
                RaceOver(f);
            }
        }

        private void Update_Finished(Frame f)
        {
            if (StateTimer.IsRunning(f) == false)
            {
                // TODO: GAME OVER KICK PLAYERS
            }
        }

        private int GetPlayerCount(Frame f)
        {
            int count = 0;

            foreach (var pair in f.Unsafe.GetComponentBlockIterator<PlayerLink>())
            {
                count++;
            }

            return count;
        }
    }
}