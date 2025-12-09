using System;
using System.Collections;
using System.Collections.Generic;
using Quantum;
using UnityEngine;
using UnityEngine.UI;

public class PickupViewController : QuantumCallbacks
{
    [SerializeField] private GameObject ToggleObject;

    private EntityRef _cachedRef;

    private EntityRef _entityRef
    {
        get
        {
            if (_cachedRef == default)
            {
                _cachedRef = GetComponent<QuantumEntityView>().EntityRef;
            }

            return _cachedRef;
        }
    }
    
    private QuantumGame _game;

    public override void OnUnitySceneLoadDone(QuantumGame game)
    {
        _game = game;
    }

    public override void OnGameStart(QuantumGame game, bool first)
    {
        _game = game;
    }

    private void Awake()
    {
        QuantumEvent.Subscribe(this, (EventOnPickupCollected e) => OnPickupCollected(e));
        QuantumEvent.Subscribe(this, (EventOnPickupRespawn e) => OnPickupRespawn(e));
    }

    private void OnPickupCollected(EventOnPickupCollected callback)
    {
        if (callback.PickupEntity != _entityRef) { return; }

        ToggleVisible(false);
    }

    private void OnPickupRespawn(EventOnPickupRespawn callback)
    {
        if (callback.PickupEntity != _entityRef) { return; }

        ToggleVisible(true);
    }

    private void ToggleVisible(bool visible)
    {
        ToggleObject.SetActive(visible);
    }
}