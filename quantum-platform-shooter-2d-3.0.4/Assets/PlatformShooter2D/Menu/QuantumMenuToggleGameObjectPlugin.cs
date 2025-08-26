using Quantum.Menu;
using UnityEngine;

public class QuantumMenuToggleGameObjectPlugin : QuantumMenuScreenPlugin
{
  public GameObject[] HideObjects;
  public GameObject[] ShowObjects;

  public override void Show(QuantumMenuUIScreen screen)
  {
    foreach (var go in HideObjects)
    {
      go.SetActive(false);
    }

    foreach (var go in ShowObjects)
    {
      go.SetActive(true);
    }
  }
}
