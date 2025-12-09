using System.Collections;
using System.Collections.Generic;
using Quantum;
using UnityEngine;

public unsafe class VisualKartSpawner : QuantumEntityViewComponent<CustomViewContext>
{
    public override void OnActivate(Frame frame)
    {
        if (frame.Unsafe.TryGetPointer(EntityRef, out Kart* kart))
        {
            KartVisuals visuals = frame.FindAsset(kart->VisualAsset);
            GameObject visualKart = Instantiate(visuals.KartPrefab, transform);

            visualKart.transform.localPosition = visuals.LocalOffset;

            var kartView = visualKart.GetComponent<KartViewController>();
            kartView.Initialize(Game);

            PlayerName nameDisplay = GetComponentInChildren<PlayerName>();

            if (LocalPlayerManager.Instance.LocalPlayerKartView == kartView)
            {
                nameDisplay.gameObject.SetActive(false);
            }
            else
            {
                nameDisplay.SetName(kartView.DriverName);
            }
        }
        else
        {
            Debug.Log("kart comp not found");
        }
    }
}