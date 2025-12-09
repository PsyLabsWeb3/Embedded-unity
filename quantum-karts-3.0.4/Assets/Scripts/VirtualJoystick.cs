using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Deterministic;
using Quantum;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Joystick Settings")]
    [SerializeField] private float _maxDistance = 100f;
    [SerializeField] private bool _snapToCenter = true;
    [SerializeField] private float _deadZone = 0.1f;
    
    [Header("Visual Components")]
    [SerializeField] private RectTransform _background;
    [SerializeField] private RectTransform _handle;
    
    private Vector2 _startPosition;
    private Vector2 _inputVector;
    private bool _isDragging = false;
    private Camera _uiCamera;
    
    public FPVector2 Direction 
    { 
        get 
        {
            if (_inputVector.magnitude < _deadZone)
                return FPVector2.Zero;
            return _inputVector.ToFPVector2(); 
        } 
    }
    
    public bool IsActive => _isDragging;
    
    private void Start()
    {
        // Get the camera used for UI rendering
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            _uiCamera = canvas.worldCamera;
        }
        
        // Store initial positions
        if (_background != null)
            _startPosition = _background.anchoredPosition;
        
        // Reset to center
        ResetJoystick();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        _isDragging = true;
        
        Debug.Log($"Joystick OnPointerDown - _snapToCenter: {_snapToCenter}");
        
        // Optional: Move joystick background to touch position
        if (_snapToCenter && _background != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform.parent as RectTransform, 
                eventData.position, 
                _uiCamera, 
                out localPoint);
            
            _background.anchoredPosition = localPoint;
            Debug.Log($"Joystick moved background to: {localPoint}");
        }
        
        OnDrag(eventData);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        _isDragging = false;
        ResetJoystick();
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging || _background == null || _handle == null)
            return;
        
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _background, 
            eventData.position, 
            _uiCamera, 
            out localPoint))
        {
            // Clamp the handle position within the max distance
            Vector2 clampedPosition = Vector2.ClampMagnitude(localPoint, _maxDistance);
            _handle.anchoredPosition = clampedPosition;
            
            // Calculate input vector (normalized)
            // Make sure we handle the coordinate system properly
            if (_maxDistance > 0)
            {
                _inputVector = clampedPosition / _maxDistance;
            }
            else
            {
                _inputVector = Vector2.zero;
            }
            
            // Debug logging
            Debug.Log($"Joystick - Screen: {eventData.position}, Local: {localPoint}, Clamped: {clampedPosition}, Input: {_inputVector}, MaxDist: {_maxDistance}");
        }
    }
    
    private void ResetJoystick()
    {
        if (_handle != null)
        {
            _handle.anchoredPosition = Vector2.zero;
        }
        
        if (_background != null && _snapToCenter)
        {
            _background.anchoredPosition = _startPosition;
        }
        
        _inputVector = Vector2.zero;
    }
    
    // Debug visualization
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && _isDragging)
        {
            Gizmos.color = Color.red;
            Vector3 worldPos = transform.position;
            Gizmos.DrawWireSphere(worldPos, 1f);
            
            Gizmos.color = Color.green;
            Vector3 direction = new Vector3(_inputVector.x, 0, _inputVector.y);
            Gizmos.DrawRay(worldPos, direction * 2f);
        }
    }
    
    // Public methods for external control
    public void SetMaxDistance(float distance)
    {
        _maxDistance = distance;
    }
    
    public void SetDeadZone(float deadZone)
    {
        _deadZone = Mathf.Clamp01(deadZone);
    }
    
    public Vector2 GetRawInputVector()
    {
        return _inputVector;
    }
    
    [ContextMenu("Debug Joystick State")]
    public void DebugJoystickState()
    {
        Debug.Log($"=== JOYSTICK DEBUG ===");
        Debug.Log($"_inputVector: {_inputVector}");
        Debug.Log($"Direction (FPVector2): {Direction}");
        Debug.Log($"_maxDistance: {_maxDistance}");
        Debug.Log($"_deadZone: {_deadZone}");
        Debug.Log($"_snapToCenter: {_snapToCenter}");
        Debug.Log($"_isDragging: {_isDragging}");
        Debug.Log($"Background position: {(_background != null ? _background.anchoredPosition.ToString() : "NULL")}");
        Debug.Log($"Handle position: {(_handle != null ? _handle.anchoredPosition.ToString() : "NULL")}");
        Debug.Log($"===================");
    }
}