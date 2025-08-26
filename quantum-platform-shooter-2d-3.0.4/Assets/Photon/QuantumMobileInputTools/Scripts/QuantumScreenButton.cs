namespace QuantumMobileInputTools
{
  using UnityEngine;

  using UnityEngine.EventSystems;
  public class QuantumScreenButton : QuantumScreenControl, IPointerDownHandler, IPointerUpHandler
  {
    
    [SerializeField]
    private ControlMap _controlMap;
    
    public override void OnPointerUp(PointerEventData eventData)
    {
      QuantumLocalInputValuesControl.Instance.SendValueToControl(_controlMap, false);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
      QuantumLocalInputValuesControl.Instance.SendValueToControl(_controlMap, true);
    }

    public override void OnDrag(PointerEventData eventData)
    {
    }
  }
}
