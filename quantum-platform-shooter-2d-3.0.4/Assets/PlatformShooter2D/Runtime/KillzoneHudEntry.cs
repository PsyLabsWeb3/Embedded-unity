namespace PlatformShooter2D
{
  using System.Collections;
  using Quantum;
  using TMPro;
  using UnityEngine;

  /// <summary>
  /// Represents an entry in the kill notification HUD, displaying details about a character's death.
  /// </summary>
  // ReSharper disable InvalidXmlDocComment
  public class KillzoneHudEntry : QuantumSceneViewComponent<CustomViewContext>
  {
    [Header("References")] 
    /// <summary>
    /// Text element to display the kill notification.
    /// </summary>
    public TMP_Text EntryText;

    /// <summary>
    /// Canvas group to control the visibility of the entry.
    /// </summary>
    public CanvasGroup CanvasGroup;

    /// <summary>
    /// Curve to animate the alpha transparency of the entry.
    /// </summary>
    [Header("Configurations")] 
    public AnimationCurve AlphaCurve;

    /// <summary>
    /// Color used for the killer's name in the notification.
    /// </summary>
    public Color KillerNameColor = Color.yellow;

    /// <summary>
    /// Color used for the dead character's name in the notification.
    /// </summary>
    public Color DeadNameColor = Color.red;

    /// <summary>
    /// RectTransform used for positioning the entry during animations.
    /// </summary>
    public RectTransform GroupTransform;

    /// <summary>
    /// Curve to animate the entry's position.
    /// </summary>
    public AnimationCurve PositionCurve;

    /// <summary>
    /// Duration of the entry's animation when it appears.
    /// </summary>
    public float AnimateInDuration = 0.5f;

    /// <summary>
    /// Duration the entry remains visible before fading out.
    /// </summary>
    public float AliveTime = 8.0f;

    private EventOnCharacterDeath _deathEvent;

    /// <summary>
    /// Sets the death event data for this entry.
    /// </summary>
    public EventOnCharacterDeath DeathEvent
    {
      set => _deathEvent = value;
    }

    /// <summary>
    /// Initializes the entry with details about the character death and starts animations.
    /// </summary>
    /// <param name="frame">The Quantum frame containing event data.</param>
    public override void OnActivate(Frame frame)
    {
      var destroyedCharacter = frame.Get<PlayerLink>(_deathEvent.Character);
      var destroyedCharacterPlayer = frame.GetPlayerData(destroyedCharacter.Player);

      var sourceCharacter = frame.Get<PlayerLink>(_deathEvent.Killer);
      var sourceCharacterPlayer = frame.GetPlayerData(sourceCharacter.Player);

      StartCoroutine(AnimateIn());

      if (destroyedCharacter.Player == sourceCharacter.Player)
      {
        EntryText.text = string.Format(
          "<color={0}>{1}</color> self-destructed", 
          ColorToHex(DeadNameColor), 
          destroyedCharacterPlayer.PlayerNickname);
      }
      else
      {
        EntryText.text = string.Format(
          "<color={0}>{1}</color> destroyed <color={2}>{3}</color>", 
          ColorToHex(KillerNameColor),
          sourceCharacterPlayer.PlayerNickname,
          ColorToHex(DeadNameColor),
          destroyedCharacterPlayer.PlayerNickname);
      }

      StartCoroutine(AnimateOutAndDestroy());
    }

    /// <summary>
    /// Animates the entry sliding into position.
    /// </summary>
    private IEnumerator AnimateIn()
    {
      float originalX = GroupTransform.anchoredPosition.x;

      for (float t = 0.0f; t < AnimateInDuration; t += Time.deltaTime)
      {
        float x = originalX * PositionCurve.Evaluate(t / AnimateInDuration);

        Vector3 position = GroupTransform.anchoredPosition;
        position.x = x;
        GroupTransform.anchoredPosition = position;

        yield return null;
      }

      Vector3 finalPosition = GroupTransform.anchoredPosition;
      finalPosition.x = 0.0f;
      GroupTransform.anchoredPosition = finalPosition;
    }

    /// <summary>
    /// Animates the entry fading out and then destroys it.
    /// </summary>
    private IEnumerator AnimateOutAndDestroy()
    {
      float originalAlpha = CanvasGroup.alpha;

      for (float t = 0; t < AliveTime; t += Time.deltaTime)
      {
        var alpha = originalAlpha * AlphaCurve.Evaluate(t / AliveTime);
        CanvasGroup.alpha = alpha;

        yield return null;
      }

      Destroy(gameObject);
    }

    /// <summary>
    /// Converts a color to its hexadecimal string representation.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>Hexadecimal string of the color.</returns>
    private static string ColorToHex(Color32 color)
    {
      return $"#{color.r:X2}{color.g:X2}{color.b:X2}";
    }
  }
}
