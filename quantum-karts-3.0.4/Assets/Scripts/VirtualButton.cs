using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class VirtualButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Button Settings")]
    [SerializeField] private bool _toggleMode = false;
    [SerializeField] private float _cooldownTime = 0f;
    
    [Header("Visual Feedback")]
    [SerializeField] private Image _buttonImage;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _pressedColor = Color.gray;
    [SerializeField] private Color _disabledColor = Color.red;
    [SerializeField] private float _colorTransitionSpeed = 5f;
    
    [Header("Animation")]
    [SerializeField] private bool _useScaleAnimation = true;
    [SerializeField] private float _pressedScale = 0.95f;
    [SerializeField] private float _animationSpeed = 10f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _pressSound;
    [SerializeField] private AudioClip _releaseSound;
    
    private bool _isPressed = false;
    private bool _isToggled = false;
    private bool _isOnCooldown = false;
    private Vector3 _originalScale;
    private Color _targetColor;
    private float _targetScale;
    
    public bool IsPressed 
    { 
        get 
        { 
            if (_toggleMode)
                return _isToggled;
            return _isPressed;
        } 
    }
    
    public bool IsOnCooldown => _isOnCooldown;
    
    private void Start()
    {
        // Store original scale
        _originalScale = transform.localScale;
        _targetScale = 1f;
        
        // Initialize visual state
        if (_buttonImage == null)
            _buttonImage = GetComponent<Image>();
            
        _targetColor = _normalColor;
        
        // Setup audio source if not assigned
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
    }
    
    private void Update()
    {
        // Smooth color transition
        if (_buttonImage != null)
        {
            _buttonImage.color = Color.Lerp(_buttonImage.color, _targetColor, Time.deltaTime * _colorTransitionSpeed);
        }
        
        // Smooth scale animation
        if (_useScaleAnimation)
        {
            Vector3 targetScaleVector = _originalScale * _targetScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScaleVector, Time.deltaTime * _animationSpeed);
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isOnCooldown)
            return;
            
        Debug.Log($"VirtualButton '{gameObject.name}' pressed!");
            
        if (_toggleMode)
        {
            _isToggled = !_isToggled;
            UpdateVisualState();
            
            if (_isToggled)
                PlayPressSound();
            else
                PlayReleaseSound();
        }
        else
        {
            _isPressed = true;
            UpdateVisualState();
            PlayPressSound();
        }
        
        // Start cooldown if specified
        if (_cooldownTime > 0f)
        {
            StartCoroutine(CooldownCoroutine());
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (_toggleMode || _isOnCooldown)
            return;
            
        _isPressed = false;
        UpdateVisualState();
        PlayReleaseSound();
    }
    
    private void UpdateVisualState()
    {
        if (_isOnCooldown)
        {
            _targetColor = _disabledColor;
            _targetScale = 1f;
        }
        else if (IsPressed)
        {
            _targetColor = _pressedColor;
            _targetScale = _pressedScale;
        }
        else
        {
            _targetColor = _normalColor;
            _targetScale = 1f;
        }
    }
    
    private void PlayPressSound()
    {
        if (_audioSource != null && _pressSound != null)
        {
            _audioSource.PlayOneShot(_pressSound);
        }
    }
    
    private void PlayReleaseSound()
    {
        if (_audioSource != null && _releaseSound != null)
        {
            _audioSource.PlayOneShot(_releaseSound);
        }
    }
    
    private IEnumerator CooldownCoroutine()
    {
        _isOnCooldown = true;
        UpdateVisualState();
        
        yield return new WaitForSeconds(_cooldownTime);
        
        _isOnCooldown = false;
        UpdateVisualState();
    }
    
    // Public methods for external control
    public void SetPressed(bool pressed)
    {
        if (_toggleMode)
        {
            _isToggled = pressed;
        }
        else
        {
            _isPressed = pressed;
        }
        UpdateVisualState();
    }
    
    public void SetToggleMode(bool toggle)
    {
        _toggleMode = toggle;
        if (!toggle)
        {
            _isToggled = false;
        }
        UpdateVisualState();
    }
    
    public void SetCooldownTime(float cooldown)
    {
        _cooldownTime = cooldown;
    }
    
    public void TriggerPress()
    {
        OnPointerDown(null);
    }
    
    public void TriggerRelease()
    {
        OnPointerUp(null);
    }
}