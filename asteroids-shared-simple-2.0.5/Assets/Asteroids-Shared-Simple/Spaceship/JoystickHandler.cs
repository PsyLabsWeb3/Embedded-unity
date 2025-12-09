using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoystickHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform knob;
    public float maxDistance = 50f;
    private Vector2 startPoint;
    private Vector2 inputVector;
    private RectTransform rectTransform;

    public Vector2 Input => inputVector;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out startPoint);
        UpdateKnob(eventData);
        Debug.Log("🟢 OnPointerDown called");
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateKnob(eventData);
        Debug.Log("🟢 OnDrag called");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        knob.anchoredPosition = Vector2.zero;
        Debug.Log("🟢 OnPointerUP called");
    }

    private void UpdateKnob(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localPoint);
        Vector2 delta = localPoint - startPoint;
        delta = Vector2.ClampMagnitude(delta, maxDistance);
        inputVector = delta / maxDistance;
        knob.anchoredPosition = delta;
        Debug.Log($"🟢 Joystick '{gameObject.name}' Input: {inputVector}");
    }
}