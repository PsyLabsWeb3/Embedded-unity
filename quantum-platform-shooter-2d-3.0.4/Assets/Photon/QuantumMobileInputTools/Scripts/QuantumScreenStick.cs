namespace QuantumMobileInputTools
{
  using UnityEngine;

  using UnityEngine.EventSystems;
  using UnityEngine.Serialization;

  public class QuantumScreenStick : QuantumScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
  {
    [SerializeField]
    private ControlMap _controlMap;
    
    public float MovementRange
    {
      get => _movementRange;
      set => _movementRange = value;
    }

    [FormerlySerializedAs("movementRange")] [SerializeField]
    private float _movementRange = 50;

    private Vector3 _startPos;
    private Vector2 _pointerDownPos;

    private void Start()
    {
      _startPos = ((RectTransform)transform).anchoredPosition;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
      if (eventData == null)
      {
        throw new System.ArgumentNullException(nameof(eventData));
      }

      RectTransformUtility.ScreenPointToLocalPointInRectangle
      (transform.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera,
        out _pointerDownPos);
    }

    public override void OnDrag(PointerEventData eventData)
    {
      if (eventData == null)
      {
        throw new System.ArgumentNullException(nameof(eventData));
      }

      RectTransformUtility.ScreenPointToLocalPointInRectangle
      (transform.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera,
        out var position);
      var delta = position - _pointerDownPos;

      delta = Vector2.ClampMagnitude(delta, MovementRange);
      ((RectTransform)transform).anchoredPosition = _startPos + (Vector3)delta;

      var newPos = new Vector2(delta.x / MovementRange, delta.y / MovementRange);
      QuantumLocalInputValuesControl.Instance.SendValueToControl(_controlMap, newPos);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
      ((RectTransform)transform).anchoredPosition = _startPos;
      QuantumLocalInputValuesControl.Instance.SendValueToControl(_controlMap, Vector2.zero);
    }
  }
}
