using System.Collections;
using System.Collections.Generic;
using Quantum;
using Quantum.Menu;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class KartSelector : MonoBehaviour
{
    [SerializeField] private List<KartVisuals> _kartVisuals;
    [SerializeField] private TMP_Dropdown _visualDropdown;

    [SerializeField] private List<KartStats> _kartStats;
    [SerializeField] private TMP_Dropdown _statDropdown;
    [SerializeField] private QuantumMenuUIMain _quantumMenuUIMain;

    // Start is called before the first frame update
    void Start()
    {
        _visualDropdown.ClearOptions();
        _statDropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> visualData = new();
        List<TMP_Dropdown.OptionData> statsData = new();

        foreach (KartVisuals kv in _kartVisuals)
        {
            visualData.Add(new TMP_Dropdown.OptionData(kv.name));
        }

        foreach (KartStats ks in _kartStats)
        {
            statsData.Add(new TMP_Dropdown.OptionData(ks.name));
        }

        _visualDropdown.AddOptions(visualData);
        _statDropdown.AddOptions(statsData);
    }

     public void OnVisualsChange(int index)
     {
         _quantumMenuUIMain.ConnectionArgs.RuntimePlayers[0].KartVisuals = _kartVisuals[index];
     }
    
     public void OnStatsChange(int index)
     {
         _quantumMenuUIMain.ConnectionArgs.RuntimePlayers[0].KartStats = _kartStats[index];
     }
}
