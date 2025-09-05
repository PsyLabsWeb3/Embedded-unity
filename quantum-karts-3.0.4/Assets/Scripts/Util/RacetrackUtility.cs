using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quantum;

public class RacetrackUtility : MonoBehaviour
{
    [SerializeField] private Transform _checkpointParent;

    [ContextMenu("Update Checkpoints")]
    private void UpdateCheckpoints()
    {
        QPrototypeRaceTrack raceTrack = GetComponent<QPrototypeRaceTrack>();

        if (raceTrack.Prototype.Checkpoints.Length != _checkpointParent.childCount)
        {
            raceTrack.Prototype.Checkpoints = new QuantumEntityPrototype [_checkpointParent.childCount];

            for (int i = 0; i < _checkpointParent.childCount; i++)
            {
                raceTrack.Prototype.Checkpoints[i] = _checkpointParent.GetChild(i).GetComponent<QuantumEntityPrototype>();
            }
        }
    }
}
