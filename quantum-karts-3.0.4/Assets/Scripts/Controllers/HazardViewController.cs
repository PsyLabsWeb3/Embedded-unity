using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quantum;

public class HazardViewController : QuantumCallbacks
{
    [SerializeField] private GameObject _destroyVisuals;

    public void OnEntityDestroyed(QuantumGame game)
    {
        Instantiate(_destroyVisuals, transform.position, Quaternion.identity);
    }
}
