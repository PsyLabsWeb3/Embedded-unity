using System;
using Photon.Deterministic;
using Quantum;
using UnityEngine;

public class CheckpointUtility : MonoBehaviour
{
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!transform.parent)
        {
            return;
        }

        int siblingIndex = transform.GetSiblingIndex();
        int siblingCount = transform.parent.childCount;

        QPrototypeCheckpoint component = GetComponent<QPrototypeCheckpoint>();

        component.Prototype.Index = (sbyte)siblingIndex;
        component.Prototype.Finish = siblingIndex == siblingCount - 1;
    }

    private void OnDrawGizmosSelected()
    {
        QPrototypeCheckpoint component = GetComponent<QPrototypeCheckpoint>();

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.TransformPoint(component.Prototype.AIOptimalLocalPosition.ToUnityVector3()), 1f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.TransformPoint(component.Prototype.AIBadLocalPosition.ToUnityVector3()), 1f);
    }
#endif
}
