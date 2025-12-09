using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using Fusion.Photon.Realtime;
using UnityEngine;

public static class RegionPicker
{
    public static async Task<string> GetBestRegionViaRunnerAsync(NetworkRunner runnerPrefab, string fallback = "us", int timeoutMs = 6000)
    {
        var tempGO = new GameObject("TempRunnerForRegion", typeof(NetworkRunner));
        var runner = tempGO.GetComponent<NetworkRunner>();
        runner.ProvideInput = false;

        string bestRegion = fallback;
        var cts = new CancellationTokenSource(timeoutMs);

        try
        {
            var regions = await NetworkRunner.GetAvailableRegions(null, cts.Token);
            if (regions != null && regions.Count > 0)
            {
                var valid = regions.Where(r => r.RegionPing >= 0).ToList();
                if (valid.Count > 0)
                {
                    bestRegion = valid.OrderBy(r => r.RegionPing).First().RegionCode;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"🌐 [RegionPicker] Error al obtener regiones: {ex.Message}. Usando fallback '{fallback}'");
        }

        GameObject.Destroy(tempGO);
        Debug.Log($"🌍 [RegionPicker] Región seleccionada: {bestRegion}");
        return bestRegion;
    }
}