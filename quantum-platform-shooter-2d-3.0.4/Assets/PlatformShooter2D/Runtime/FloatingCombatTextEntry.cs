namespace PlatformShooter2D
{
  using TMPro;
  using UnityEngine;

  /// <summary>
  /// The floating text element handled by FloatingCombatText.
  /// </summary>
  // ReSharper disable InvalidXmlDocComment
  public class FloatingCombatTextEntry : MonoBehaviour
  {
    [Header("References")] 
    /// <summary>
    /// Text element used to display the damage or message.
    /// </summary>
    public TMP_Text TextElement;

    /// <summary>
    /// RectTransform for positioning and scaling the text.
    /// </summary>
    public RectTransform RectTransform;

    [Header("Configurations")] 
    /// <summary>
    /// // Curve for controlling the alpha transparency over time.
    /// </summary>
    public AnimationCurve AlphaCurve;

    /// <summary>
    /// Curve for horizontal offset over time.
    /// </summary>
    public AnimationCurve XOffsetCurve;

    /// <summary>
    /// Curve for vertical offset over time.
    /// </summary>
    public AnimationCurve YOffsetCurve;

    /// <summary>
    /// Curve for controlling the scale over time.
    /// </summary>
    public AnimationCurve ScaleCurve;

    /// <summary>
    /// Total duration of the animation.
    /// </summary>
    public float AnimationTime;

    /// <summary>
    /// Base offset for the text.
    /// </summary>
    public Vector2 Offset;

    /// <summary>
    /// Random variance applied to the offset.
    /// </summary>
    public Vector2 OffsetVariance;

    /// <summary>
    /// // Scale factor for the offset variance.
    /// </summary>
    public Vector2 OffsetVarianceScale = new(10.0f, 10.0f);

    /// <summary>
    /// Callback when the animation finishes.
    /// </summary>
    public System.Action<FloatingCombatTextEntry> OnAnimationFinished;

    /// <summary>
    /// Internal timer for tracking animation progress.
    /// </summary>
    private float _timer = -1;

    /// <summary>
    ///  Calculated target offset for the text.
    /// </summary>
    private Vector2 _targetOffset;

    /// <summary>
    /// Activates the floating combat text entry with the specified damage value and direction hint.
    /// </summary>
    /// <param name="damage">The amount of damage to display.</param>
    /// <param name="directionHint">The direction hint to position the text offset.</param>
    public void Activate(int damage, Vector2 directionHint)
    {
      TextElement.text = damage.ToString();
      TextElement.enabled = true;
      enabled = true;

      TextElement.rectTransform.anchoredPosition = Vector2.zero;
      TextElement.alpha = 1;
      _timer = 0;

      // Calculate the horizontal offset based on the direction hint.
      if (directionHint.x > 0.0f)
      {
        _targetOffset.x = Random.Range(0.0f, OffsetVariance.x) * OffsetVarianceScale.x + Offset.x;
      }
      else
      {
        _targetOffset.x = Random.Range(-OffsetVariance.x, 0.0f) * OffsetVarianceScale.x - Offset.x;
      }

      // Calculate the vertical offset with variance.
      _targetOffset.y = Random.Range(-OffsetVariance.y, OffsetVariance.y) * OffsetVarianceScale.y + Offset.y;
    }

    /// <summary>
    /// Deactivates the floating combat text entry and triggers the callback for reuse.
    /// </summary>
    public void Deactivate()
    {
      // Reset the animation timer.
      _timer = -1; 
      TextElement.enabled = false;
      enabled = false; 

      // Trigger the callback to return this entry to the pool.
      OnAnimationFinished?.Invoke(this);
    }

    /// <summary>
    /// Updates the floating combat text animation each frame.
    /// Handles alpha, position, and scale.
    /// </summary>
    private void Update()
    {
      _timer += Time.deltaTime; 
      float time = Mathf.Clamp01(_timer / AnimationTime); 
      
      TextElement.alpha = AlphaCurve.Evaluate(time);

      // Update the position based on the offset curves.
      var position = _targetOffset;
      position.x *= XOffsetCurve.Evaluate(time);
      position.y *= YOffsetCurve.Evaluate(time);
      TextElement.rectTransform.anchoredPosition = position;

      // Update the scale based on the scale curve.
      var scale = ScaleCurve.Evaluate(time);
      TextElement.rectTransform.localScale = new Vector3(scale, scale, scale);

      // Deactivate the entry when the animation time is exceeded.
      if (_timer > AnimationTime)
      {
        Deactivate();
      }
    }
  }
}