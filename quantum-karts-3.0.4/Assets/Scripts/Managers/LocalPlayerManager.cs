using System.Collections;
using System.Collections.Generic;
using Photon.Deterministic;
using Quantum;
using UnityEngine;

public class LocalPlayerManager : MonoSingleton<LocalPlayerManager>
{
    [SerializeField] private FP _predictionCullingRange;
    [SerializeField] private FP _predictionCullingFowardOffset;
    
    public QuantumEntityView LocalPlayerView { get; private set; }
    public KartViewController LocalPlayerKartView { get; private set; }
    public EntityRef LocalPlayerEntity  { get; private set; }
    public PlayerRef LocalPlayerRef  { get; private set; }

    public void RegisterLocalPlayer(KartViewController localPlayerKartView)
    {
        LocalPlayerView = localPlayerKartView.EntityView;
        LocalPlayerKartView = localPlayerKartView;
        LocalPlayerEntity = localPlayerKartView.EntityView.EntityRef;
        LocalPlayerRef = localPlayerKartView.PlayerRef;

        var predictionCullingController = localPlayerKartView.gameObject.AddComponent<PredictionCullingController>();
        predictionCullingController.Range = _predictionCullingRange;
        predictionCullingController.FowardOffset = _predictionCullingFowardOffset;
    }
}
