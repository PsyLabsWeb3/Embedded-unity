using Fusion;
using FusionExamples.Tanknarok;
using UnityEngine;

public class MobileInput : MonoBehaviour
{
	[SerializeField] private RectTransform _leftJoy;
	[SerializeField] private RectTransform _leftKnob;
	[SerializeField] private RectTransform _rightJoy;
	[SerializeField] private RectTransform _rightKnob;
	private Transform _canvas;

	public Vector2 MoveInput { get; private set; }
	public Vector2 AimInput { get; private set; }

	private bool _leftDown;
	private bool _rightDown;

	private bool _leftReleased;
	private bool _rightReleased;

	public bool LeftReleasedThisFrame() => _leftReleased;
	public bool RightReleasedThisFrame() => _rightReleased;

	private void Awake()
	{
		_canvas = GetComponentInParent<Canvas>().transform;
	}

	private void LateUpdate()
	{
		_leftReleased = false;
		_rightReleased = false;
	}
private void SetJoy(RectTransform joy, RectTransform knob, bool active, Vector2 center, Vector2 current, out Vector2 result)
{
	center /= _canvas.localScale.x;
	current /= _canvas.localScale.x;

	joy.gameObject.SetActive(active);
	joy.anchoredPosition = center;

	Vector2 direction = current - center;
	if (direction.magnitude > knob.rect.width / 2)
		direction = direction.normalized * (knob.rect.width / 2);

	knob.anchoredPosition = direction;

	result = active ? direction / (knob.rect.width / 2) : Vector2.zero;

	if (joy == _leftJoy)
		Debug.Log($"📱 MoveInput: {result}");
	else if (joy == _rightJoy)
		Debug.Log($"🎯 AimInput: {result}");
}


	public void SetLeft(bool active, Vector2 down, Vector2 current)
	{
		if (_leftDown && !active)
			_leftReleased = true;

		_leftDown = active;

		SetJoy(_leftJoy, _leftKnob, active, down, current, out Vector2 move);
		MoveInput = move;
	}

	public void SetRight(bool active, Vector2 down, Vector2 current)
	{
		if (_rightDown && !active)
			_rightReleased = true;

		_rightDown = active;

		SetJoy(_rightJoy, _rightKnob, active, down, current, out Vector2 aim);
		AimInput = aim;
	}

private void Update()
{
    if (!Application.isMobilePlatform && !Application.isEditor)
        return;

    bool leftHandled = false;
    bool rightHandled = false;

    for (int i = 0; i < Input.touchCount; i++)
    {
        Touch touch = Input.GetTouch(i);
        Vector2 pos = touch.position;

        if (pos.x < Screen.width / 2)
        {
            // Lado izquierdo - joystick de movimiento
            if (!leftHandled)
            {
                Debug.Log($"📱 Left touch: {touch.phase}, pos: {pos}");
                if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    SetLeft(true, touch.position, touch.position);
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    SetLeft(false, Vector2.zero, Vector2.zero);

                leftHandled = true;
            }
        }
        else
        {
            // Lado derecho - joystick de apuntado
            if (!rightHandled)
            {
                Debug.Log($"🎯 Right touch: {touch.phase}, pos: {pos}");
                if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    SetRight(true, touch.position, touch.position);
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    SetRight(false, Vector2.zero, Vector2.zero);

                rightHandled = true;
            }
        }
    }

    if (!leftHandled) SetLeft(false, Vector2.zero, Vector2.zero);
    if (!rightHandled) SetRight(false, Vector2.zero, Vector2.zero);
}

}
