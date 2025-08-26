namespace QuantumMobileInputTools
{
  using UnityEngine;

  using UnityEngine.EventSystems;

  public abstract class QuantumScreenControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
  {
    public abstract void OnPointerUp(PointerEventData eventData);

    public abstract void OnPointerDown(PointerEventData eventData);

    public abstract void OnDrag(PointerEventData eventData);
  }
}