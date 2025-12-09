using System.Collections;
using System.Collections.Generic;
using Photon.Deterministic;
using Quantum;
using UnityEngine;

public class PredictionCullingController : MonoBehaviour
{
    public FP Range;
    public FP FowardOffset;

    void Update()
    {
        var pos = transform.position.ToFPVector3() + (transform.forward.ToFPVector3() * FowardOffset);
        QuantumRunner.Default.Game.SetPredictionArea(pos, Range);
    }
}
